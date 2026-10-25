import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext.tsx';

export function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="app-container">
      {/* Banner de Check-up Pendente (TDAH onboarding) */}
      {user?.precisaCheckup && (
        <aside className="checkup-banner" role="banner">
          <div className="checkup-banner-content">
            <span className="checkup-banner-icon" aria-hidden="true">
              ✨
            </span>
            <p>
              <strong>Você ainda não fez o Check-up inicial.</strong> Leva menos
              de 1 minuto para organizar seus prazos essenciais!
            </p>
            <Link to="/checkup" className="btn btn-checkup-action">
              Fazer Check-up agora
            </Link>
          </div>
        </aside>
      )}

      {/* Header Principal */}
      <header className="app-header">
        <div className="header-inner">
          <div className="brand">
            <Link to="/" className="brand-logo" aria-label="Em Dia - Página inicial">
              <span className="brand-icon">📅</span>
              <span className="brand-name">Em Dia</span>
            </Link>
            <span className="brand-badge">Radar TDAH</span>
          </div>

          <nav className="nav-menu" aria-label="Navegação principal">
            <NavLink
              to="/"
              end
              className={({ isActive }) =>
                `nav-link ${isActive ? 'nav-link-active' : ''}`
              }
            >
              <span aria-hidden="true">🎯</span> Radar
            </NavLink>
            <NavLink
              to="/itens"
              className={({ isActive }) =>
                `nav-link ${isActive ? 'nav-link-active' : ''}`
              }
            >
              <span aria-hidden="true">📋</span> Meus Itens
            </NavLink>
            <NavLink
              to="/veiculos"
              className={({ isActive }) =>
                `nav-link ${isActive ? 'nav-link-active' : ''}`
              }
            >
              <span aria-hidden="true">🚗</span> Veículos
            </NavLink>
            <NavLink
              to="/perfil"
              className={({ isActive }) =>
                `nav-link ${isActive ? 'nav-link-active' : ''}`
              }
            >
              <span aria-hidden="true">⚙️</span> Perfil
            </NavLink>
          </nav>

          <div className="header-user">
            <span className="user-greeting">Olá, {user?.nome?.split(' ')[0] || 'você'}</span>
            <button
              type="button"
              className="btn btn-outline btn-sm btn-logout"
              onClick={handleLogout}
              aria-label="Sair da conta"
            >
              Sair
            </button>
          </div>
        </div>
      </header>

      {/* Conteúdo Principal */}
      <main className="app-main">
        <Outlet />
      </main>

      <footer className="app-footer">
        <p>
          Em Dia &bull; Radar de vencimentos para adultos com TDAH &bull; UNICV
          Engenharia de Software
        </p>
      </footer>
    </div>
  );
}
