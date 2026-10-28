import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import Constants from 'expo-constants';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';
import type { ProblemDetails } from './types';

// Cliente HTTP do app (mesmas regras do front web — spec §5.6): token de acesso só em memória;
// em 401, tenta renovar uma única vez e repete a requisição; se falhar, volta ao login.
// Diferença do navegador (DV17): o token de renovação fica no SecureStore do aparelho, não em cookie.

const CHAVE_REFRESH = 'emdia.refreshToken';
const PORTA_API = 5196;

/**
 * Endereço da API. No Expo Go, o celular acessa o computador pelo IP da rede local — o mesmo do servidor
 * Metro (hostUri). Pode ser trocado com a variável EXPO_PUBLIC_API_URL.
 */
export function descobrirUrlDaApi(): string {
  const definida = process.env.EXPO_PUBLIC_API_URL;
  if (definida) return definida.replace(/\/$/, '');

  const host = Constants.expoConfig?.hostUri?.split(':')[0];
  if (host) return `http://${host}:${PORTA_API}/api/v1`;

  // Emulador Android sem Metro na rede: 10.0.2.2 é o computador
  return Platform.OS === 'android' ? `http://10.0.2.2:${PORTA_API}/api/v1` : `http://localhost:${PORTA_API}/api/v1`;
}

export const API_URL = descobrirUrlDaApi();

let accessToken: string | null = null;
let aoPerderSessao: (() => void) | null = null;

export const setAccessToken = (token: string | null) => {
  accessToken = token;
};

export const onSessaoExpirada = (callback: (() => void) | null) => {
  aoPerderSessao = callback;
};

export const guardarRefresh = (valor: string | null) =>
  valor ? SecureStore.setItemAsync(CHAVE_REFRESH, valor) : SecureStore.deleteItemAsync(CHAVE_REFRESH);

export const lerRefresh = () => SecureStore.getItemAsync(CHAVE_REFRESH);

export const api = axios.create({
  baseURL: API_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json', 'X-Cliente': 'mobile' },
});

api.interceptors.request.use((config) => {
  if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
  return config;
});

let renovacao: Promise<string> | null = null;

/** Troca o token de renovação guardado por um novo par de tokens (rotação — spec §4.3). */
export function renovarToken(): Promise<string> {
  renovacao ??= (async () => {
    const refresh = await lerRefresh();
    if (!refresh) throw new Error('Sem sessão salva');
    const { data } = await axios.post<{ accessToken: string; refreshToken: string }>(
      `${API_URL}/auth/refresh`, null,
      { headers: { 'X-Cliente': 'mobile', 'X-Refresh-Token': refresh }, timeout: 15000 },
    );
    setAccessToken(data.accessToken);
    await guardarRefresh(data.refreshToken);
    return data.accessToken;
  })().finally(() => {
    renovacao = null;
  });
  return renovacao;
}

const rotasDeAutenticacao = ['/auth/login', '/auth/registrar', '/auth/refresh'];

api.interceptors.response.use(
  (resposta) => resposta,
  async (erro: AxiosError) => {
    const original = erro.config as (InternalAxiosRequestConfig & { _repetida?: boolean }) | undefined;
    const ehAuth = rotasDeAutenticacao.some((r) => original?.url?.includes(r));

    if (erro.response?.status === 401 && original && !original._repetida && !ehAuth) {
      original._repetida = true;
      try {
        const novo = await renovarToken();
        original.headers.Authorization = `Bearer ${novo}`;
        return api(original);
      } catch {
        setAccessToken(null);
        await guardarRefresh(null);
        aoPerderSessao?.();
      }
    }
    return Promise.reject(erro);
  },
);

/** Extrai o ProblemDetails de um erro da API (spec §3.5). */
export function problema(erro: unknown): ProblemDetails | null {
  if (axios.isAxiosError(erro) && erro.response?.data && typeof erro.response.data === 'object') {
    return { status: erro.response.status, ...(erro.response.data as ProblemDetails) };
  }
  return null;
}

/** Mensagem em linguagem simples para mostrar perto da ação que falhou. */
export function mensagemDeErro(erro: unknown): string {
  const p = problema(erro);
  if (p?.detail) return p.detail;
  if (p?.status === 400) {
    const primeiro = p.errors && Object.values(p.errors)[0]?.[0];
    return primeiro ?? 'Confira os campos destacados.';
  }
  if (axios.isAxiosError(erro) && !erro.response)
    return `Não conseguimos falar com o servidor (${API_URL}). Confira se a API está rodando e se o celular está na mesma rede Wi-Fi.`;
  return 'Algo deu errado. Tente de novo em instantes.';
}

/** Erros 400 por campo, com o nome do campo da API (ex.: "dataBase"). */
export function errosPorCampo(erro: unknown): Record<string, string> {
  const p = problema(erro);
  if (p?.status !== 400 || !p.errors) return {};
  return Object.fromEntries(Object.entries(p.errors).map(([campo, msgs]) => [campo, msgs[0]]));
}
