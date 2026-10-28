import { useState, type FormEvent } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext.tsx';
import logoImg from '../../assets/logo.png';
import { Icon } from '../../components/Icon.tsx';
import './AtlassianAuth.css';

interface AuthPageAtlassianProps {
  initialMode: 'login' | 'cadastro';
}

export function AuthPageAtlassian({ initialMode }: AuthPageAtlassianProps) {
  const { login, registrar } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [mode, setMode] = useState<'login' | 'cadastro'>(initialMode);
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(false);

  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);

    if (mode === 'cadastro' && !nome.trim()) {
      setErro('Por favor, informe seu nome completo.');
      return;
    }

    if (!email.trim() || !senha) {
      setErro('Preencha seu e-mail e senha.');
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      setErro('Informe um e-mail válido, como voce@exemplo.com.');
      return;
    }

    if (senha.length < 6) {
      setErro('A senha deve conter no mínimo 6 caracteres.');
      return;
    }

    try {
      setCarregando(true);
      if (mode === 'cadastro') {
        await registrar({ nome: nome.trim(), email: email.trim(), senha });
        navigate('/checkup', { replace: true });
      } else {
        const usuario = await login({ email: email.trim(), senha });
        if (usuario.precisaCheckup) {
          navigate('/checkup', { replace: true });
        } else {
          navigate(from, { replace: true });
        }
      }
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { data?: { mensagem?: string } } };
        setErro(
          axErr.response?.data?.mensagem ||
            (mode === 'cadastro'
              ? 'Erro ao realizar cadastro.'
              : 'E-mail ou senha incorretos.')
        );
      } else {
        setErro('Falha de conexão com o servidor. Tente novamente.');
      }
    } finally {
      setCarregando(false);
    }
  };



  return (
    <div className="atlassian-page">
      {/* ── 2. Top Navigation Bar ──────────────────────────────────────────── */}
      <header className="atlassian-header">
        <div className="atlassian-header-inner">
          <div className="atlassian-nav-left">
            <a href="/login" className="atlassian-brand" aria-label="Em Dia — início">
              <img src={logoImg} alt="Em Dia" className="atlassian-logo-img" />
            </a>
            <a href="#showcase" className="atlassian-nav-item">Como funciona <Icon name="arrow-right" size={15} /></a>
          </div>

          <div className="atlassian-nav-right">
            {mode === 'cadastro' ? (
              <button type="button" onClick={() => { setMode('login'); setErro(null); }} className="nav-link-login">
                Já tenho conta
              </button>
            ) : (
              <button type="button" onClick={() => { setMode('cadastro'); setErro(null); }} className="nav-link-login">
                Criar conta
              </button>
            )}
            <button type="button" onClick={() => { setMode('cadastro'); setErro(null); }} className="btn-header-cta">
              Começar grátis <Icon name="arrow-right" size={16} />
            </button>
          </div>
        </div>
      </header>

      {/* ── 3. Hero Section (2 Colunas) ────────────────────────────────────── */}
      <main className="atlassian-hero-section">
        <div className="atlassian-hero-grid">
          {/* Lado Esquerdo: Headline e Descrição de Alto Impacto */}
          <div className="hero-left">
            <div className="hero-grid-bg" aria-hidden="true" />
            <h1 className="hero-headline">
              Sua vida já tem coisa demais para lembrar.
            </h1>
            <p className="hero-subhead">
              O Em Dia reúne prazos de documentos, veículo, casa, saúde e garantias em um Radar visual pensado para ajudar você a agir no momento certo — sem depender só da memória.
            </p>

            <div className="hero-badges-row">
              <span className="hero-badge-pill">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                  <polyline points="22 4 12 14.01 9 11.01" />
                </svg>
                Radar Visual de 4 Anéis
              </span>
              <span className="hero-badge-pill">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <circle cx="12" cy="12" r="10" />
                  <circle cx="12" cy="12" r="6" />
                  <circle cx="12" cy="12" r="2" />
                </svg>
                Modo Foco para reduzir distrações
              </span>
              <span className="hero-badge-pill">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <rect x="1" y="3" width="15" height="13" />
                  <polygon points="16 8 20 8 23 11 23 16 16 16 16 8" />
                  <circle cx="5.5" cy="18.5" r="2.5" />
                  <circle cx="18.5" cy="18.5" r="2.5" />
                </svg>
                Controle de Odômetro para Veículos
              </span>
            </div>
          </div>

          {/* Lado Direito: Card de Autenticação / Cadastro */}
          <div className="hero-right">
            <div className="atlassian-auth-card">
              {/* Abas Alternadoras */}
              <div className="auth-tabs-header">
                <button
                  type="button"
                  className={`auth-tab-btn ${mode === 'cadastro' ? 'active' : ''}`}
                  onClick={() => {
                    setMode('cadastro');
                    setErro(null);
                  }}
                >
                  Fazer cadastro
                </button>
                <button
                  type="button"
                  className={`auth-tab-btn ${mode === 'login' ? 'active' : ''}`}
                  onClick={() => {
                    setMode('login');
                    setErro(null);
                  }}
                >
                  Entrar
                </button>
              </div>

              {erro && (
                <div className="alert-atlassian-error" role="alert">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <circle cx="12" cy="12" r="10" />
                    <line x1="12" y1="8" x2="12" y2="12" />
                    <line x1="12" y1="16" x2="12.01" y2="16" />
                  </svg>
                  <span>{erro}</span>
                </div>
              )}

              <form onSubmit={handleSubmit} className="atlassian-form" noValidate>
                {mode === 'cadastro' && (
                  <div className="form-field">
                    <label htmlFor="nome" className="form-label">
                      Seu nome ou apelido
                    </label>
                    <input
                      id="nome"
                      type="text"
                      className="atlassian-input"
                      placeholder="Ex: Felipe Nadab"
                      value={nome}
                      onChange={(e) => setNome(e.target.value)}
                      required
                    />
                  </div>
                )}

                <div className="form-field">
                  <label htmlFor="email" className="form-label">
                    E-mail
                  </label>
                  <input
                    id="email"
                    type="email"
                    className="atlassian-input"
                    placeholder="voce@empresa.com"
                    value={email}
                    onChange={(e) => { setEmail(e.target.value); if (erro) setErro(null); }}
                    required
                    aria-invalid={!!erro}
                    autoComplete="email"
                  />
                  <span className="input-helper">
                    Seu e-mail é usado para acessar sua conta e seus prazos.
                  </span>
                </div>

                <div className="form-field">
                  <label htmlFor="senha" className="form-label">
                    Senha {mode === 'cadastro' ? '(mínimo 6 caracteres)' : ''}
                  </label>
                  <input
                    id="senha"
                    type="password"
                    className="atlassian-input"
                    placeholder="••••••••"
                    value={senha}
                    onChange={(e) => { setSenha(e.target.value); if (erro) setErro(null); }}
                    required
                    aria-invalid={!!erro}
                    autoComplete={mode === 'cadastro' ? 'new-password' : 'current-password'}
                  />
                </div>

                <button
                  type="submit"
                  className="btn-atlassian-submit"
                  disabled={carregando}
                >
                  {carregando
                    ? 'Processando...'
                    : mode === 'cadastro'
                    ? 'Fazer cadastro'
                    : 'Entrar no Em Dia'}
                </button>
              </form>

            </div>
          </div>
        </div>
      </main>

      {/* ── 4. Showcase / Mockup do Produto (Inferior) ────────────────────────── */}
      <section id="showcase" className="atlassian-showcase-section" aria-labelledby="showcase-title">
        <div className="showcase-intro">
          <span className="section-eyebrow">UMA VISÃO MAIS CLARA</span>
          <h2 id="showcase-title">Os prazos no lugar certo.</h2>
          <p>Prévia ilustrativa da organização por urgência. Seus itens reais aparecem depois de entrar na conta.</p>
        </div>
        <div className="showcase-window">
          {/* Topbar do Mockup */}
          <div className="window-topbar">
            <div className="window-breadcrumb">
              <span className="breadcrumb-icon"><Icon name="check-circle" size={15} /></span>
              <span>Em Dia App · Radar & Tarefas</span>
            </div>

            <div className="window-nav-tabs">
              <span className="mock-tab">Linha do Tempo</span>
              <span className="mock-tab">Backlog</span>
              <span className="mock-tab active">Quadro</span>
              <span className="mock-tab">Lista +</span>
            </div>
          </div>

          {/* Kanban / Cards do Radar */}
          <div className="mock-board-body">
            {/* Coluna 1: A Fazer */}
            <div className="mock-column">
              <div className="mock-column-header">
                <span>Atrasados & Hoje</span>
  
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Renovar CNH</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-danger">Atrasado</span>
                  <span>Documento</span>
                </div>
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Pagar IPVA 2026</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-warning">Vence Hoje</span>
                  <span>Veículo · Sefaz</span>
                </div>
                {/* Tooltip do Modo Foco idêntico ao balão do Liam */}
                <div className="mock-agent-pill">
                  Modo Foco · até 3 itens prioritários
                </div>
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Aluguel do Mês</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-danger">Atrasado</span>
                  <span>Casa · Pix</span>
                </div>
              </div>
            </div>

            {/* Coluna 2: Em Andamento */}
            <div className="mock-column">
              <div className="mock-column-header">
                <span>Próximos 7 Dias</span>
  
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Revisão de 10.000 km</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-warning">Faltam 4 dias</span>
                  <span>Carro · Odômetro</span>
                </div>
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Vacina da Gripe</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-success">Amanhã 09:00</span>
                  <span>Saúde · UBS</span>
                </div>
              </div>
            </div>

            {/* Coluna 3: Concluídos */}
            <div className="mock-column">
              <div className="mock-column-header">
                <span>Resolvidos & Em Dia</span>
  
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Seguro Residencial</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-success">Em dia</span>
                  <span>Casa · Pago</span>
                </div>
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Manutenção Filtro de Água</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-success">Em dia</span>
                  <span>Produto · OK</span>
                </div>
              </div>

              <div className="mock-card">
                <div className="mock-card-title">Licenciamento do Veículo</div>
                <div className="mock-card-footer">
                  <span className="mock-status-pill mock-pill-success">Em dia</span>
                  <span>Veículo · OK</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
