import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { useQuery } from '@tanstack/react-query';
import { catalogoApi } from '../../api/endpoints.ts';
import type { CheckupResponse, Pergunta } from '../../api/types.ts';

/** Respostas do check-up: true = sim, false = não; ausente = pulada. */
type Respostas = Record<string, boolean>;

interface CheckupEstado {
  perguntas: Pergunta[] | undefined;
  carregando: boolean;
  erro: boolean;
  recarregar: () => void;
  respostas: Respostas;
  responder: (codigo: string, resposta: boolean | null) => void;
  indice: number;
  setIndice: (i: number) => void;
  resultado: CheckupResponse | null;
  setResultado: (r: CheckupResponse) => void;
}

const Contexto = createContext<CheckupEstado | null>(null);

/** Guarda as respostas entre as telas T03 → T04 → T05. */
export function CheckupProvider({ children }: { children: ReactNode }) {
  const consulta = useQuery({ queryKey: ['checkup', 'perguntas'], queryFn: catalogoApi.perguntas, staleTime: Infinity });
  const [respostas, setRespostas] = useState<Respostas>({});
  const [indice, setIndice] = useState(0);
  const [resultado, setResultado] = useState<CheckupResponse | null>(null);

  const valor = useMemo<CheckupEstado>(() => ({
    perguntas: consulta.data,
    carregando: consulta.isPending,
    erro: consulta.isError,
    recarregar: () => void consulta.refetch(),
    respostas,
    responder: (codigo, resposta) => setRespostas((r) => {
      const novo = { ...r };
      if (resposta === null) delete novo[codigo];
      else novo[codigo] = resposta;
      return novo;
    }),
    indice,
    setIndice,
    resultado,
    setResultado,
  }), [consulta, respostas, indice, resultado]);

  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useCheckup() {
  const ctx = useContext(Contexto);
  if (!ctx) throw new Error('useCheckup precisa estar dentro de <CheckupProvider>.');
  return ctx;
}
