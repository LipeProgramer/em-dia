import { useState, type FormEvent } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext.tsx';
import logoImg from '../../assets/logo.png';
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
  const [showBanner, setShowBanner] = useState(true);

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

  const handleGoogleDemo = async () => {
    // Autenticação rápida de demonstração com um clique
    setEmail('felipe@emdia.com');
    setSenha('SenhaForte@123');
    try {
      setCarregando(true);
      await login({ email: 'felipe@emdia.com', senha: 'SenhaForte@123' });
      navigate(from, { replace: true });
    } catch {
      setErro('Use o formulário para entrar ou registrar.');
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="atlassian-page">
      {/* ── 1. Announcement Bar ────────────────────────────────────────────── */}
      {showBanner && (
        <div className="atlassian-announcement">
          <span>
            87% dos adultos com TDAH relatam menos estresse ao usar o radar visual. Apenas 13% sentem atrito.{' '}
            <a href="#showcase">Veja como funciona o radar</a>
          </span>
          <button
            type="button"
            className="announcement-close"
            onClick={() => setShowBanner(false)}
            aria-label="Fechar aviso"
          >
            ×
          </button>
        </div>
      )}

      {/* ── 2. Top Navigation Bar ──────────────────────────────────────────── */}
      <header className="atlassian-header">
        <div className="atlassian-header-inner">
          <div className="atlassian-nav-left">
            <a href="/" className="atlassian-brand">
              <img src={logoImg} alt="Em Dia Logo" className="atlassian-logo-img" />
            </a>

            <nav className="atlassian-nav-menu" aria-label="Menu principal">
              <button type="button" className="atlassian-nav-item">
                Funções
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M6 9l6 6 6-6" />
                </svg>
              </button>
              <button type="button" className="atlassian-nav-item">
                Soluções
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M6 9l6 6 6-6" />
                </svg>
              </button>
              <button type="button" className="atlassian-nav-item">
                Guias
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M6 9l6 6 6-6" />
                </svg>
              </button>
              <button type="button" className="atlassian-nav-item">
                Modelos
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M6 9l6 6 6-6" />
                </svg>
              </button>
              <button type="button" className="atlassian-nav-item">
                Preços
              </button>
            </nav>
          </div>

          <div className="atlassian-nav-right">
            <button type="button" className="btn-search-icon" aria-label="Pesquisar">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
              </svg>
            </button>

            {mode === 'cadastro' ? (
              <button
                type="button"
                onClick={() => {
                  setMode('login');
                  setErro(null);
                }}
                className="nav-link-login"
              >
                Entrar
              </button>
            ) : (
              <button
                type="button"
                onClick={() => {
                  setMode('cadastro');
                  setErro(null);
                }}
                className="nav-link-login"
              >
                Fazer cadastro
              </button>
            )}

            <button
              type="button"
              onClick={() => {
                setMode('cadastro');
                setErro(null);
              }}
              className="btn-header-cta"
            >
              Obtenha grátis
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
              Foco nos resultados, não na administração
            </h1>
            <p className="hero-subhead">
              Radar inteligente de vencimentos e lembretes para adultos com TDAH que elimina a
              ansiedade e evita multas e esquecimentos desnecessários.
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
                Modo Foco Anti-Procrastinação
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
                    onChange={(e) => setEmail(e.target.value)}
                    required
                    autoComplete="email"
                  />
                  <span className="input-helper">
                    Use um e-mail corporativo ou pessoal para ter acesso completo ao radar.
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
                    onChange={(e) => setSenha(e.target.value)}
                    required
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

              <div className="atlassian-divider">
                <span>Ou continuar com</span>
              </div>

              <button
                type="button"
                className="btn-google"
                onClick={handleGoogleDemo}
                title="Acesso rápido para demonstração"
              >
                <svg width="18" height="18" viewBox="0 0 24 24">
                  <path
                    fill="#4285F4"
                    d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                  />
                  <path
                    fill="#34A853"
                    d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                  />
                  <path
                    fill="#FBBC05"
                    d="M5.84 14.1c-.22-.66-.35-1.36-.35-2.1s.13-1.44.35-2.1V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.62z"
                  />
                  <path
                    fill="#EA4335"
                    d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"
                  />
                </svg>
                Google
              </button>
            </div>
          </div>
        </div>
      </main>

      {/* ── 4. Showcase / Mockup do Produto (Inferior) ────────────────────────── */}
      <section id="showcase" className="atlassian-showcase-section">
        <div className="showcase-window">
          {/* Topbar do Mockup */}
          <div className="window-topbar">
            <div className="window-breadcrumb">
              <span className="breadcrumb-icon">✓</span>
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
                <span className="mock-badge-count">3</span>
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
                  🎯 Modo Foco: 3 itens prioritários
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
                <span className="mock-badge-count">2</span>
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
                <span className="mock-badge-count">7</span>
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
