import React, {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
  type ReactNode,
} from 'react';
import { radarApi, itensApi } from '../api/services';
import type {
  ItemResponse,
  RadarResponse,
  ConcluirItemRequest,
  NotificacaoItem,
} from '../api/types';
import { useAuth } from './AuthContext';

interface RadarCounts {
  atrasados: number;
  hoje: number;
  proximos: number;
  emDia: number;
  total: number;
}

interface RadarContextType {
  radarData: RadarResponse | null;
  itens: ItemResponse[];
  counts: RadarCounts;
  isLoading: boolean;
  refreshing: boolean;
  modoFocoAtivo: boolean;
  itensFoco: ItemResponse[];
  notificacoes: NotificacaoItem[];
  carregarDados: () => Promise<void>;
  alternarModoFoco: () => void;
  concluirItem: (id: string, req?: ConcluirItemRequest) => Promise<void>;
}

const RadarContext = createContext<RadarContextType | undefined>(undefined);

export function RadarProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const [radarData, setRadarData] = useState<RadarResponse | null>(null);
  const [itens, setItens] = useState<ItemResponse[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [modoFocoAtivo, setModoFocoAtivo] = useState(false);

  const carregarDados = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      setIsLoading(true);
      const [radarRes, itensRes] = await Promise.all([
        radarApi.obter().catch(() => null),
        itensApi.listar().catch(() => []),
      ]);

      if (radarRes) setRadarData(radarRes);
      if (itensRes) setItens(itensRes);
    } catch {
      // Ignora erro de rede temporário
    } finally {
      setIsLoading(false);
      setRefreshing(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      carregarDados();
    } else {
      setRadarData(null);
      setItens([]);
    }
  }, [isAuthenticated, carregarDados]);

  // Contadores para o Radar Donut e cabeçalho
  const counts: RadarCounts = {
    atrasados: itens.filter((i) => i.diasRestantes < 0).length,
    hoje: itens.filter((i) => i.diasRestantes === 0).length,
    proximos: itens.filter((i) => i.diasRestantes > 0 && i.diasRestantes <= 7).length,
    emDia: itens.filter((i) => i.diasRestantes > 7).length,
    total: itens.length,
  };

  // 3 itens prioritários para o Modo Foco (TDAH-friendly)
  // Ordena por criticidade: atrasados primeiro, depois hoje, depois menor prazo
  const itensFoco = [...itens]
    .sort((a, b) => a.diasRestantes - b.diasRestantes)
    .slice(0, 3);

  const alternarModoFoco = () => {
    setModoFocoAtivo((prev) => !prev);
  };

  const concluirItem = async (id: string, req?: ConcluirItemRequest) => {
    await itensApi.concluir(id, req || { dataConclusao: new Date().toISOString() });
    await carregarDados();
  };

  // Notificações inteligentes geradas a partir dos itens do radar
  const notificacoes: NotificacaoItem[] = [
    ...itens
      .filter((i) => i.diasRestantes <= 0)
      .map((i) => ({
        id: `aviso-${i.id}`,
        titulo: i.diasRestantes < 0 ? `Lembrete: ${i.nome} está atrasado` : `Lembrete: ${i.nome} vence hoje`,
        mensagem:
          i.diasRestantes < 0
            ? `Seu vencimento está com ${Math.abs(i.diasRestantes)} dia(s) de atraso. Evite penalidades!`
            : `Hoje é o dia limite para resolver este compromisso.`,
        tipo: 'lembrete' as const,
        data: 'Hoje',
        lida: false,
      })),
    {
      id: 'sistema-bem-vindo',
      titulo: 'Radar Em Dia Ativado',
      mensagem: 'Seu radar de vencimentos e lembretes inteligentes está pronto para manter sua rotina tranquila.',
      tipo: 'sistema' as const,
      data: 'Recente',
      lida: true,
    },
  ];

  return (
    <RadarContext.Provider
      value={{
        radarData,
        itens,
        counts,
        isLoading,
        refreshing,
        modoFocoAtivo,
        itensFoco,
        notificacoes,
        carregarDados,
        alternarModoFoco,
        concluirItem,
      }}
    >
      {children}
    </RadarContext.Provider>
  );
}

export function useRadar() {
  const context = useContext(RadarContext);
  if (!context) {
    throw new Error('useRadar deve ser usado dentro de um RadarProvider');
  }
  return context;
}
