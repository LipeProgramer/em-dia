import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { ProblemDetails } from './types.ts';

// Cliente HTTP (spec §1.5 e §5.6): uma instância do axios com a URL base, que adiciona o token
// de acesso em cada requisição. O token fica só em memória — nunca no localStorage (spec §4.3).

let accessToken: string | null = null;
let aoPerderSessao: (() => void) | null = null;

export const setAccessToken = (token: string | null) => {
  accessToken = token;
};

export const getAccessToken = () => accessToken;

/** Chamado quando a renovação falha: a sessão é limpa e o usuário vai para o login. */
export const onSessaoExpirada = (callback: (() => void) | null) => {
  aoPerderSessao = callback;
};

export const api = axios.create({
  baseURL: '/api/v1',
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
  return config;
});

// Renovação compartilhada: várias requisições com 401 ao mesmo tempo esperam o mesmo refresh.
let renovacao: Promise<string> | null = null;

export async function renovarToken(): Promise<string> {
  renovacao ??= axios
    .post<{ accessToken: string }>('/api/v1/auth/refresh', null, { withCredentials: true })
    .then(({ data }) => {
      setAccessToken(data.accessToken);
      return data.accessToken;
    })
    .finally(() => {
      renovacao = null;
    });
  return renovacao;
}

const rotasDeAutenticacao = ['/auth/login', '/auth/registrar', '/auth/refresh'];

// 401: tenta /auth/refresh uma única vez e repete a requisição; se falhar, leva ao login.
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
        aoPerderSessao?.();
      }
    }
    return Promise.reject(erro);
  },
);

/** Extrai o ProblemDetails de um erro da API. */
export function problema(erro: unknown): ProblemDetails | null {
  if (axios.isAxiosError(erro) && erro.response?.data && typeof erro.response.data === 'object') {
    return { status: erro.response.status, ...(erro.response.data as ProblemDetails) };
  }
  return null;
}

/** Mensagem em linguagem simples para mostrar perto da ação que falhou (campo "detail"). */
export function mensagemDeErro(erro: unknown): string {
  const p = problema(erro);
  if (p?.detail) return p.detail;
  if (p?.status === 400) return 'Confira os campos destacados.';
  if (axios.isAxiosError(erro) && !erro.response) return 'Sem conexão com o servidor. Tente de novo.';
  return 'Algo deu errado. Tente de novo em instantes.';
}
