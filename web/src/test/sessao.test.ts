import axios, { type InternalAxiosRequestConfig } from 'axios';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, getAccessToken, onSessaoExpirada, setAccessToken } from '../api/client.ts';
import { erroDaApi } from './utils.tsx';

describe('Cliente HTTP (spec §5.6)', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    api.defaults.adapter = undefined;
    setAccessToken(null);
    onSessaoExpirada(null);
  });

  it('CT49 · token expira durante o uso → renovação transparente e a ação continua', async () => {
    setAccessToken('token-velho');
    const refresh = vi.spyOn(axios, 'post').mockResolvedValue({ data: { accessToken: 'token-novo' } });
    const cabecalhos: string[] = [];
    api.defaults.adapter = async (config: InternalAxiosRequestConfig) => {
      cabecalhos.push(String(config.headers.Authorization));
      if (config.headers.Authorization === 'Bearer token-velho') throw erroDaApi(401, {});
      return { data: { ok: true }, status: 200, statusText: 'OK', headers: {}, config };
    };

    const resposta = await api.get('/radar');

    expect(resposta.data).toEqual({ ok: true });
    expect(refresh).toHaveBeenCalledWith('/api/v1/auth/refresh', null, { withCredentials: true });
    expect(cabecalhos).toEqual(['Bearer token-velho', 'Bearer token-novo']);
    expect(getAccessToken()).toBe('token-novo');
  });

  it('se a renovação falhar, limpa a sessão e avisa para ir ao login', async () => {
    setAccessToken('token-velho');
    vi.spyOn(axios, 'post').mockRejectedValue(erroDaApi(401, {}));
    const perdeu = vi.fn();
    onSessaoExpirada(perdeu);
    api.defaults.adapter = async () => { throw erroDaApi(401, {}); };

    await expect(api.get('/radar')).rejects.toBeTruthy();

    expect(perdeu).toHaveBeenCalledOnce();
    expect(getAccessToken()).toBeNull();
  });
});
