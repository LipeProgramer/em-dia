import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { checkupApi } from '../../api/services.ts';
import { Icon, type IconName } from '../../components/Icon.tsx';
import { useAuth } from '../auth/AuthContext.tsx';

interface PerguntaCheckup {
  codigo: string;
  icone: IconName;
  titulo: string;
  descricao: string;
}

const PERGUNTAS: PerguntaCheckup[] = [
  {
    codigo: 'TEM_PET',
    icone: 'paw',
    titulo: 'Você tem animal de estimação?',
    descricao: 'Vamos monitorar as vacinas e vermífugos anuais do seu pet.',
  },
  {
    codigo: 'COMPROU_ELETRONICO',
    icone: 'monitor',
    titulo: 'Comprou eletrônicos ou eletrodomésticos recentes?',
    descricao: 'Acompanhe os prazos de garantia do fabricante para não perder direitos.',
  },
  {
    codigo: 'TEM_PURIFICADOR',
    icone: 'droplet',
    titulo: 'Possui purificador ou filtro de água em casa?',
    descricao: 'Lembretes periódicos para substituição do refil e água limpa.',
  },
  {
    codigo: 'TEM_CARRO',
    icone: 'car',
    titulo: 'Possui automóvel ou motocicleta?',
    descricao: 'Prazos de IPVA, seguro e manutenção preventiva.',
  },
  {
    codigo: 'TEM_CNH',
    icone: 'id-card',
    titulo: 'Você possui Carteira Nacional de Habilitação (CNH)?',
    descricao: 'Alertas com antecedência para renovação de exames e taxas.',
  },
];

export function CheckupView() {
  const navigate = useNavigate();
  const { refreshUser } = useAuth();

  const [respostas, setRespostas] = useState<Record<string, boolean>>({
    TEM_PET: false,
    COMPROU_ELETRONICO: false,
    TEM_PURIFICADOR: false,
    TEM_CARRO: false,
    TEM_CNH: false,
  });

  const [salvando, setSalvando] = useState(false);
  const [concluidoSucesso, setConcluidoSucesso] = useState(false);
  const [totalCriados, setTotalCriados] = useState(0);
  const [erro, setErro] = useState<string | null>(null);

  const toggleResposta = (codigo: string, valor: boolean) => {
    setRespostas((prev) => ({ ...prev, [codigo]: valor }));
  };

  const handleFinalizar = async () => {
    setErro(null);
    try {
      setSalvando(true);
      const res = await checkupApi.executar({ respostas });
      setTotalCriados(res.itensCriados);
      setConcluidoSucesso(true);
      await refreshUser();
    } catch {
      setErro('Ocorreu um erro ao processar seu check-up. Tente novamente.');
    } finally {
      setSalvando(false);
    }
  };

  if (concluidoSucesso) {
    return (
      <div className="checkup-container">
        <div className="checkup-success-card">
          <div className="checkup-success-icon" aria-hidden="true">
            <Icon name="check-circle" size={30} />
          </div>
          <h1>Check-up Concluído!</h1>
          <p className="checkup-success-text">
            {totalCriados > 0 ? (
              <>
                Criamos <strong>{totalCriados} item(ns) automático(s)</strong> no
                seu Radar para você não esquecer de nada.
              </>
            ) : (
              <>
                Seu check-up foi registrado com sucesso! Você pode cadastrar itens
                personalizados a qualquer momento.
              </>
            )}
          </p>
          <button
            type="button"
            className="btn btn-primary btn-lg btn-block"
            onClick={() => navigate('/')}
          >
            Ver meu Radar agora
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="checkup-container">
      <div className="checkup-card">
        <header className="checkup-header">
          <span className="checkup-step-tag">Check-up Inicial TDAH</span>
          <h1>Configuração Rápida em 1 Minuto</h1>
          <p>
            Selecione o que faz parte da sua rotina. Nós cuidaremos de calcular os
            prazos e alertar você antes que vençam.
          </p>
        </header>

        {erro && (
          <div className="alert alert-error" role="alert">
            <Icon name="alert-circle" size={17} /> {erro}
          </div>
        )}

        <div className="checkup-questions-list">
          {PERGUNTAS.map((p) => {
            const selecionado = respostas[p.codigo];
            return (
              <div
                key={p.codigo}
                className={`checkup-question-item ${
                  selecionado ? 'question-active' : ''
                }`}
              >
                <div className="question-info">
                  <span className="question-icon" aria-hidden="true">
                    <Icon name={p.icone} size={22} />
                  </span>
                  <div>
                    <h2 className="question-title">{p.titulo}</h2>
                    <p className="question-desc">{p.descricao}</p>
                  </div>
                </div>

                <div className="question-actions">
                  <button
                    type="button"
                    className={`btn btn-toggle ${!selecionado ? 'btn-toggle-active' : ''}`}
                    onClick={() => toggleResposta(p.codigo, false)}
                    aria-pressed={!selecionado}
                  >
                    Não
                  </button>
                  <button
                    type="button"
                    className={`btn btn-toggle ${selecionado ? 'btn-toggle-active-yes' : ''}`}
                    onClick={() => toggleResposta(p.codigo, true)}
                    aria-pressed={selecionado}
                  >
                    Sim
                  </button>
                </div>
              </div>
            );
          })}
        </div>

        <div className="checkup-footer">
          <button
            type="button"
            className="btn btn-primary btn-lg btn-block"
            onClick={handleFinalizar}
            disabled={salvando}
          >
            {salvando ? 'Criando seus itens...' : 'Finalizar e Gerar Radar'}
          </button>
        </div>
      </div>
    </div>
  );
}
