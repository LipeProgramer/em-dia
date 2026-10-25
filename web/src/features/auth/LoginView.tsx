import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext.tsx';

export function LoginView() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(false);

  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);

    if (!email || !senha) {
      setErro('Preencha seu e-mail e senha.');
      return;
    }

    try {
      setCarregando(true);
      const usuario = await login({ email, senha });
      if (usuario.precisaCheckup) {
        navigate('/checkup', { replace: true });
      } else {
        navigate(from, { replace: true });
      }
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { data?: { mensagem?: string } } };
        setErro(axErr.response?.data?.mensagem || 'E-mail ou senha incorretos.');
      } else {
        setErro('Falha de conexão com o servidor. Tente novamente.');
      }
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <div className="auth-icon" aria-hidden="true">
            📅
          </div>
          <h1 className="auth-title">Entrar no Em Dia</h1>
          <p className="auth-subtitle">
            Seu radar de vencimentos e prazos sem sobrecarga mental.
          </p>
        </div>

        {erro && (
          <div className="alert alert-error" role="alert">
            <span aria-hidden="true">⚠️</span> {erro}
          </div>
        )}

        <form onSubmit={handleSubmit} className="auth-form" noValidate>
          <div className="form-group">
            <label htmlFor="email">E-mail</label>
            <input
              id="email"
              type="email"
              className="form-control"
              placeholder="seu.email@exemplo.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="senha">Senha</label>
            <input
              id="senha"
              type="password"
              className="form-control"
              placeholder="••••••••"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
              autoComplete="current-password"
            />
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-block btn-lg"
            disabled={carregando}
          >
            {carregando ? 'Entrando...' : 'Entrar no sistema'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Não tem uma conta ainda?{' '}
            <Link to="/cadastro" className="auth-link">
              Cadastre-se gratuitamente
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
