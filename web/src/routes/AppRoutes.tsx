import { Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute.tsx';
import { Layout } from '../components/Layout.tsx';
import { LoginView } from '../features/auth/LoginView.tsx';
import { CadastroView } from '../features/auth/CadastroView.tsx';
import { CheckupView } from '../features/checkup/CheckupView.tsx';
import { RadarView } from '../features/radar/RadarView.tsx';
import { ItensView } from '../features/itens/ItensView.tsx';
import { ItemFormView } from '../features/itens/ItemFormView.tsx';
import { VeiculosView } from '../features/veiculos/VeiculosView.tsx';
import { PerfilView } from '../features/perfil/PerfilView.tsx';

export function AppRoutes() {
  return (
    <Routes>
      {/* Rotas Públicas */}
      <Route path="/login" element={<LoginView />} />
      <Route path="/cadastro" element={<CadastroView />} />

      {/* Rotas Protegidas com Layout */}
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<RadarView />} />
        <Route path="checkup" element={<CheckupView />} />
        <Route path="itens" element={<ItensView />} />
        <Route path="itens/novo" element={<ItemFormView />} />
        <Route path="itens/:id/editar" element={<ItemFormView />} />
        <Route path="veiculos" element={<VeiculosView />} />
        <Route path="perfil" element={<PerfilView />} />
      </Route>

      {/* Fallback */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
