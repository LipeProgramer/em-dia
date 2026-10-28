import { forwardRef, useEffect, useId, useRef, type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import type { StatusItem } from '../api/types.ts';
import { Icon, type IconName } from './Icon.tsx';

// Componentes de interface acessíveis (spec §5.3 e §5.5): área de toque ≥ 44 px, erro ligado ao campo
// por aria-describedby e status sempre com cor + símbolo + texto.

type Variante = 'primario' | 'secundario' | 'perigo' | 'texto';

export const Button = forwardRef<HTMLButtonElement, ButtonHTMLAttributes<HTMLButtonElement> & {
  variante?: Variante;
  carregando?: boolean;
  icone?: IconName;
}>(function Button({ variante = 'primario', carregando, icone, children, className = '', disabled, ...props }, ref) {
  return (
    <button
      ref={ref}
      className={`btn btn-${variante} ${className}`}
      disabled={disabled || carregando}
      aria-busy={carregando || undefined}
      {...props}
    >
      {icone && <Icon name={icone} size={18} />}
      <span>{carregando ? 'Aguarde…' : children}</span>
    </button>
  );
});

interface FieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  erro?: string;
  ajuda?: string;
}

/** Campo com rótulo, ajuda e mensagem de erro logo abaixo, ligada por aria-describedby. */
export const Field = forwardRef<HTMLInputElement, FieldProps>(function Field({ label, erro, ajuda, id, ...props }, ref) {
  const gerado = useId();
  const campoId = id ?? gerado;
  const ajudaId = ajuda ? `${campoId}-ajuda` : undefined;
  const erroId = erro ? `${campoId}-erro` : undefined;
  return (
    <div className={`field ${erro ? 'field-invalido' : ''}`}>
      <label htmlFor={campoId}>{label}</label>
      <input
        ref={ref}
        id={campoId}
        aria-invalid={erro ? true : undefined}
        aria-describedby={[ajudaId, erroId].filter(Boolean).join(' ') || undefined}
        {...props}
      />
      {ajuda && <p id={ajudaId} className="field-ajuda">{ajuda}</p>}
      {erro && (
        <p id={erroId} className="field-erro">
          <Icon name="alert-circle" size={15} /> {erro}
        </p>
      )}
    </div>
  );
});

const status: Record<StatusItem, { texto: string; icone: IconName; classe: string }> = {
  Vencido: { texto: 'Vencido', icone: 'alert-circle', classe: 'vencido' },
  Atencao: { texto: 'Atenção', icone: 'alert-triangle', classe: 'atencao' },
  EmDia: { texto: 'Em dia', icone: 'check-circle', classe: 'em-dia' },
  Encerrado: { texto: 'Concluído', icone: 'minus', classe: 'encerrado' },
};

export const textoStatus = (s: StatusItem) => status[s].texto;

/** Status com cor, símbolo e texto — nunca só a cor (RNF02). */
export function StatusBadge({ status: s }: { status: StatusItem }) {
  const info = status[s];
  return (
    <span className={`status status-${info.classe}`}>
      <Icon name={info.icone} size={14} strokeWidth={2.4} />
      {info.texto}
    </span>
  );
}

export function EmptyState({ titulo, texto, acao }: { titulo: string; texto?: string; acao?: ReactNode }) {
  return (
    <div className="estado-vazio">
      <Icon name="sparkles" size={28} />
      <p className="estado-titulo">{titulo}</p>
      {texto && <p>{texto}</p>}
      {acao}
    </div>
  );
}

export function ErrorState({ mensagem, tentarDeNovo }: { mensagem?: string; tentarDeNovo: () => void }) {
  return (
    <div className="estado-erro" role="alert">
      <Icon name="alert-circle" size={26} />
      <p>{mensagem ?? 'Não conseguimos carregar agora.'}</p>
      <Button variante="secundario" onClick={tentarDeNovo}>Tentar de novo</Button>
    </div>
  );
}

/** Esqueleto de carregamento. */
export function Skeleton({ linhas = 3 }: { linhas?: number }) {
  return (
    <div className="esqueleto" aria-busy="true" aria-label="Carregando">
      {Array.from({ length: linhas }, (_, i) => <div key={i} className="esqueleto-linha" />)}
    </div>
  );
}

export function PageHeader({ titulo, voltar, children }: { titulo: string; voltar?: string; children?: ReactNode }) {
  return (
    <header className="page-header">
      {voltar && (
        <Link to={voltar} className="btn-voltar" aria-label="Voltar">
          <Icon name="arrow-left" size={20} />
        </Link>
      )}
      <h1>{titulo}</h1>
      {children && <div className="page-header-acoes">{children}</div>}
    </header>
  );
}

export function Alerta({ children, tipo = 'erro' }: { children: ReactNode; tipo?: 'erro' | 'sucesso' }) {
  return (
    <div className={`alerta alerta-${tipo}`} role={tipo === 'erro' ? 'alert' : 'status'}>
      <Icon name={tipo === 'erro' ? 'alert-circle' : 'check-circle'} size={18} />
      <span>{children}</span>
    </div>
  );
}

/** Barra de progresso dos passos. */
export function Progresso({ valor, rotulo }: { valor: number; rotulo: string }) {
  return (
    <div className="progresso" role="progressbar" aria-label={rotulo} aria-valuemin={0} aria-valuemax={100}
         aria-valuenow={Math.round(valor * 100)}>
      <div className="progresso-barra" style={{ width: `${Math.round(valor * 100)}%` }} />
    </div>
  );
}

/** Janela modal com foco preso no conteúdo e fechamento por Esc (usa o elemento nativo <dialog>). */
export function Dialog({ aberto, titulo, aoFechar, children }: {
  aberto: boolean;
  titulo: string;
  aoFechar: () => void;
  children: ReactNode;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  const tituloId = useId();

  useEffect(() => {
    const d = ref.current;
    if (!d) return;
    if (aberto && !d.open) {
      if (typeof d.showModal === 'function') d.showModal();
      else d.setAttribute('open', '');
    } else if (!aberto && d.open) {
      if (typeof d.close === 'function') d.close();
      else d.removeAttribute('open');
    }
  }, [aberto]);

  return (
    <dialog
      ref={ref}
      className="dialog"
      aria-labelledby={tituloId}
      onCancel={(e) => {
        e.preventDefault();
        aoFechar();
      }}
    >
      {aberto && (
        <>
          <div className="dialog-topo">
            <h2 id={tituloId}>{titulo}</h2>
            <button type="button" className="btn-icone" onClick={aoFechar} aria-label="Fechar">
              <Icon name="close" size={20} />
            </button>
          </div>
          {children}
        </>
      )}
    </dialog>
  );
}
