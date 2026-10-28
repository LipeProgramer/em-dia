import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { AxiosError, AxiosHeaders } from 'axios';

/** Envolve o componente com QueryClient e roteador em memória. */
export function Providers({ children, rota = '/' }: { children: ReactNode; rota?: string }) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return (
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={[rota]}>{children}</MemoryRouter>
    </QueryClientProvider>
  );
}

/** Erro do axios com um ProblemDetails, como a API devolveria. */
export function erroDaApi(status: number, data: object) {
  const config = { headers: new AxiosHeaders() };
  return new AxiosError('erro', String(status), config, null, {
    status, statusText: '', headers: {}, config, data,
  });
}
