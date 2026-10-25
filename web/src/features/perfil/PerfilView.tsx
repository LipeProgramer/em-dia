import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext.tsx';
import { authApi, preferenciasApi } from '../../api/services.ts';

export function PerfilView() {
  const { user, updateNome, logout } = useAuth();
  const navigate = useNavigate();

  // Estado do nome
  const [nome, setNome] = useState(user?.nome || '');
  const [salvandoNome, setSalvandoNome] = useState(false);
  const [msgNome, setMsgNome] = useState<{ tipo: 'ok' | 'erro'; texto: string } | null>(
    null
  );

  // Estado das preferências
  const [receberEmail, setReceberEmail] = useState(true);
  const [maxReavisos, setMaxReavisos] = useState<number>(2);
  const [intervaloReavisoDias, setIntervaloReavisoDias] = useState<number>(3);
  const [modoFocoPadrao, setModoFocoPadrao] = useState(false);
  const [salvandoPrefs, setSalvandoPrefs] = useState(false);
  const [msgPrefs, setMsgPrefs] = useState<{ tipo: 'ok' | 'erro'; texto: string } | null>(
    null
  );

  // Carregar preferências
  useEffect(() => {
    preferenciasApi
      .obter()
      .then((p) => {
        setReceberEmail(p.receberEmail);
        setMaxReavisos(p.maxReavisos);
        setIntervaloReavisoDias(p.intervaloReavisoDias);
        setModoFocoPadrao(p.modoFocoPadrao);
      })
      .catch(() => {
        // Ignora silenciosamente se novo
      });
  }, []);

  const handleSalvarNome = async (e: FormEvent) => {
    e.preventDefault();
    setMsgNome(null);
    if (!nome.trim()) {
      setMsgNome({ tipo: 'erro', texto: 'O nome não pode ser vazio.' });
      return;
    }
    try {
      setSalvandoNome(true);
      await updateNome(nome.trim());
      setMsgNome({ tipo: 'ok', texto: 'Nome atualizado com sucesso!' });
    } catch {
      setMsgNome({ tipo: 'erro', texto: 'Erro ao atualizar o nome.' });
    } finally {
      setSalvandoNome(false);
    }
  };

  const handleSalvarPrefs = async (e: FormEvent) => {
    e.preventDefault();
    setMsgPrefs(null);
    try {
      setSalvandoPrefs(true);
      await preferenciasApi.atualizar({
        receberEmail,
        maxReavisos,
        intervaloReavisoDias,
        modoFocoPadrao,
      });
      setMsgPrefs({ tipo: 'ok', texto: 'Preferências salvas com sucesso!' });
    } catch {
      setMsgPrefs({ tipo: 'erro', texto: 'Erro ao salvar preferências.' });
    } finally {
      setSalvandoPrefs(false);
    }
  };

  const handleExcluirConta = async () => {
    const confirmacao = window.prompt(
      'ATENÇÃO: Esta ação é definitiva e apagará todos os seus itens, veículos e dados conforme a LGPD.\n\nPara confirmar, digite EXCLUIR abaixo:'
    );
    if (confirmacao === 'EXCLUIR') {
      try {
        await authApi.excluirConta();
        await logout();
        navigate('/login');
      } catch {
        alert('Erro ao excluir conta. Tente novamente.');
      }
    }
  };

  return (
    <div className="perfil-page">
      <header className="page-header">
        <div>
          <h1>Meu Perfil</h1>
          <p>Gerencie seus dados pessoais, notificações e configurações de foco.</p>
        </div>
      </header>

      <div className="perfil-grid">
        {/* Card: Dados Básicos */}
        <section className="card perfil-card" aria-labelledby="dados-pessoais-title">
          <h2 id="dados-pessoais-title" className="card-title">
            👤 Dados da Conta
          </h2>
          <div className="user-email-display">
            <span className="label">E-mail cadastrado:</span>
            <strong>{user?.email}</strong>
          </div>

          {msgNome && (
            <div
              className={`alert ${msgNome.tipo === 'ok' ? 'alert-success' : 'alert-error'}`}
              role="alert"
            >
              {msgNome.texto}
            </div>
          )}

          <form onSubmit={handleSalvarNome} className="perfil-form">
            <div className="form-group">
              <label htmlFor="nomePerfil">Nome ou Apelido</label>
              <input
                id="nomePerfil"
                type="text"
                className="form-control"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                required
              />
            </div>
            <button
              type="submit"
              className="btn btn-primary btn-sm"
              disabled={salvandoNome}
            >
              {salvandoNome ? 'Salvando...' : 'Atualizar Nome'}
            </button>
          </form>
        </section>

        {/* Card: Preferências de Notificação e Foco */}
        <section className="card perfil-card" aria-labelledby="prefs-title">
          <h2 id="prefs-title" className="card-title">
            🔔 Preferências de Aviso e Foco
          </h2>

          {msgPrefs && (
            <div
              className={`alert ${msgPrefs.tipo === 'ok' ? 'alert-success' : 'alert-error'}`}
              role="alert"
            >
              {msgPrefs.texto}
            </div>
          )}

          <form onSubmit={handleSalvarPrefs} className="perfil-form">
            <div className="form-check checkbox-group">
              <input
                id="checkEmail"
                type="checkbox"
                className="checkbox-input"
                checked={receberEmail}
                onChange={(e) => setReceberEmail(e.target.checked)}
              />
              <label htmlFor="checkEmail" className="checkbox-label">
                <strong>Receber avisos por e-mail</strong>
                <span className="sub-label">
                  Notificações antecipadas às 8h da manhã
                </span>
              </label>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="maxReavisosInput">Máximo de Reavisos</label>
                <input
                  id="maxReavisosInput"
                  type="number"
                  min={1}
                  max={5}
                  className="form-control"
                  value={maxReavisos}
                  onChange={(e) => setMaxReavisos(parseInt(e.target.value, 10))}
                  disabled={!receberEmail}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="intervaloDiasInput">Intervalo (Dias)</label>
                <input
                  id="intervaloDiasInput"
                  type="number"
                  min={1}
                  max={15}
                  className="form-control"
                  value={intervaloReavisoDias}
                  onChange={(e) =>
                    setIntervaloReavisoDias(parseInt(e.target.value, 10))
                  }
                  disabled={!receberEmail}
                  required
                />
              </div>
            </div>

            <div className="form-check checkbox-group">
              <input
                id="checkModoFoco"
                type="checkbox"
                className="checkbox-input"
                checked={modoFocoPadrao}
                onChange={(e) => setModoFocoPadrao(e.target.checked)}
              />
              <label htmlFor="checkModoFoco" className="checkbox-label">
                <strong>Modo Foco como padrão no Radar</strong>
                <span className="sub-label">
                  Abrir o radar mostrando apenas os 3 itens mais urgentes
                </span>
              </label>
            </div>

            <button
              type="submit"
              className="btn btn-primary btn-sm"
              disabled={salvandoPrefs}
            >
              {salvandoPrefs ? 'Salvando...' : 'Salvar Preferências'}
            </button>
          </form>
        </section>

        {/* Card: Zona de Perigo / LGPD */}
        <section className="card card-danger" aria-labelledby="perigo-title">
          <h2 id="perigo-title" className="card-title text-danger">
            ⚠️ Zona de Perigo (LGPD)
          </h2>
          <p className="danger-desc">
            Excluir sua conta removerá todos os seus itens cadastrados, histórico,
            veículos e preferências de forma permanente.
          </p>
          <button
            type="button"
            className="btn btn-danger"
            onClick={handleExcluirConta}
          >
            Excluir Minha Conta Definitivamente
          </button>
        </section>
      </div>
    </div>
  );
}
