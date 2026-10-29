import { Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom';
import { useSession } from '../features/auth/useSession.tsx';
import { LoginPage } from '../features/auth/LoginPage.tsx';
import { CadastroPage } from '../features/auth/CadastroPage.tsx';
import { EsqueciSenhaPage, RedefinirSenhaPage } from '../features/auth/RecuperarSenhaPages.tsx';
import { CheckupProvider } from '../features/checkup/CheckupContext.tsx';
import { PerguntaPage } from '../features/checkup/PerguntaPage.tsx';
import { DadosPage } from '../features/checkup/DadosPage.tsx';
import { ResultadoPage } from '../features/checkup/ResultadoPage.tsx';
import { RadarPage } from '../features/radar/RadarPage.tsx';
import { ItensPage } from '../features/itens/ItensPage.tsx';
import { NovoItemPage, EditarItemPage } from '../features/itens/ItemFormPage.tsx';
import { DetalheItemPage } from '../features/itens/DetalheItemPage.tsx';
import { HistoricoPage } from '../features/historico/HistoricoPage.tsx';
import { PerfilPage } from '../features/perfil/PerfilPage.tsx';
import { VeiculosPage } from '../features/veiculos/VeiculosPage.tsx';
import { Skeleton } from '../components/ui.tsx';
import { AppLayout } from '../components/AppLayout.tsx';

/** Rotas públicas (T01, T02): se já estiver logado, vai para o Radar. */
export function PublicRoute() {
  const { usuario, carregando } = useSession();
  if (carregando) return <Skeleton />;
  return usuario ? <Navigate to="/" replace /> : <Outlet />;
}

/** Rotas privadas: sem sessão, vai para T01; depois do login, volta para onde estava. */
export function PrivateRoute() {
  const { usuario, carregando } = useSession();
  const location = useLocation();
  if (carregando) return <Skeleton />;
  if (!usuario) return <Navigate to="/entrar" replace state={{ de: location.pathname + location.search }} />;
  return <Outlet />;
}

/** Portão do check-up: enquanto precisaCheckup for verdadeiro, qualquer rota privada leva a T03. */
export function CheckupGate() {
  const { usuario } = useSession();
  const location = useLocation();
  if (usuario?.precisaCheckup && !location.pathname.startsWith('/checkup')) return <Navigate to="/checkup" replace />;
  return <Outlet />;
}

/** Rotas e telas previstas (spec §5.1). */
export function AppRoutes() {
  return (
    <Routes>
      <Route element={<PublicRoute />}>
        <Route path="/entrar" element={<LoginPage />} />
        <Route path="/cadastro" element={<CadastroPage />} />
      </Route>
      {/* Fora da rota pública: o link do e-mail abre mesmo com outra sessão aberta neste aparelho */}
      <Route path="/esqueci-senha" element={<EsqueciSenhaPage />} />
      <Route path="/redefinir-senha" element={<RedefinirSenhaPage />} />

      <Route element={<PrivateRoute />}>
        <Route element={<CheckupGate />}>
          <Route path="/checkup" element={<CheckupProvider><Outlet /></CheckupProvider>}>
            <Route index element={<PerguntaPage />} />
            <Route path="dados" element={<DadosPage />} />
            <Route path="resultado" element={<ResultadoPage />} />
          </Route>

          <Route element={<AppLayout />}>
            <Route index element={<RadarPage />} />
            <Route path="/itens" element={<ItensPage />} />
            <Route path="/itens/novo" element={<NovoItemPage />} />
            <Route path="/itens/:id" element={<DetalheItemPage />} />
            <Route path="/itens/:id/editar" element={<EditarItemPage />} />
            <Route path="/historico" element={<HistoricoPage />} />
            <Route path="/perfil" element={<PerfilPage />} />
            <Route path="/perfil/veiculos" element={<VeiculosPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
