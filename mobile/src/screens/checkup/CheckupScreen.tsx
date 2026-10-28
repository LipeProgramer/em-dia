import React, { useEffect, useMemo, useState } from 'react';
import { Switch, Text, View } from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import type { CheckupRequest, CheckupResponse, Modelo, Pergunta } from '../../api/types';
import { catalogoApi } from '../../api/services';
import { mensagemDeErro, problema } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { Alerta, CampoData, Carregando, ErroTela, Escolha, Progresso, StatusBadge, Tela, estilos } from '../../components/ui';
import { colors, radius, spacing, typography } from '../../theme';
import { nomesCategorias } from '../../utils/formato';
import { inteiro, validarData, validarVeiculo, type Erros } from '../../utils/validacao';

type Props = NativeStackScreenProps<RootStackParamList, 'Checkup' | 'PrimeiroCheckup'>;

const CATEGORIA_VEICULO = 1;
const rotuloData: Record<Modelo['tipoVencimento'], string> = {
  DataUnica: 'Quando vence?',
  RecorrentePorTempo: 'Quando foi a última vez?',
  UsoOuTempo: 'Data da última vez',
  Calculado: 'Data da compra',
};

type DadosItem = { incluir: boolean; data: string; kmBase: string };

/**
 * Check-up (T03 → T04 → T05): uma pergunta sim/não por tela; depois completar datas, km e veículo;
 * por fim, o resultado por área (E10 → E11). Funciona no primeiro acesso e ao refazer pelo Perfil.
 */
export function CheckupScreen({ navigation }: Props) {
  const { usuario, atualizarUsuario } = useAuth();
  const [perguntas, setPerguntas] = useState<Pergunta[] | null>(null);
  const [erroCarga, setErroCarga] = useState<unknown>(null);
  const [etapa, setEtapa] = useState<'perguntas' | 'dados' | 'resultado'>('perguntas');
  const [indice, setIndice] = useState(0);
  const [respostas, setRespostas] = useState<Record<string, boolean>>({});
  const [veiculo, setVeiculo] = useState({ apelido: '', tipo: 'Carro' as 'Carro' | 'Moto', kmAtual: '', mediaKmMes: '1000' });
  const [itens, setItens] = useState<Record<string, DadosItem>>({});
  const [erros, setErros] = useState<Erros>({});
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [resultado, setResultado] = useState<CheckupResponse | null>(null);

  const carregar = () => {
    setErroCarga(null);
    catalogoApi.perguntas().then(setPerguntas).catch(setErroCarga);
  };
  useEffect(carregar, []);

  const modelos = useMemo(
    () => (perguntas ?? []).filter((p) => respostas[p.codigo]).flatMap((p) => p.modelos),
    [perguntas, respostas],
  );
  const dadosDe = (id: string): DadosItem => itens[id] ?? { incluir: true, data: '', kmBase: '' };
  const mudarItem = (id: string, parcial: Partial<DadosItem>) => setItens((i) => ({ ...i, [id]: { ...dadosDe(id), ...parcial } }));

  if (erroCarga) return <Tela titulo="Check-up"><ErroTela erro={erroCarga} tentarDeNovo={carregar} /></Tela>;
  if (!perguntas) return <Carregando />;

  // ── T03 · Perguntas ────────────────────────────────────────────────────
  if (etapa === 'perguntas') {
    const total = perguntas.length;
    const p = perguntas[indice];
    const areas = [...new Set(perguntas.map((x) => x.categoriaId))];
    const posicaoArea = areas.indexOf(p.categoriaId);
    const nomeArea = nomesCategorias[p.categoriaId] ?? 'Área';
    const irPara = (proximo: number) => {
      if (proximo >= total) setEtapa('dados');
      else setIndice(proximo);
    };
    const definir = (codigos: string[], r: boolean | null) => setRespostas((atual) => {
      const novo = { ...atual };
      for (const c of codigos) {
        if (r === null) delete novo[c];
        else novo[c] = r;
      }
      return novo;
    });
    const responder = (r: boolean | null) => {
      definir([p.codigo], r);
      irPara(indice + 1);
    };
    // Pula o resto da área: as perguntas que faltam ficam sem resposta
    const pularArea = () => {
      definir(perguntas.filter((x, i) => i >= indice && x.categoriaId === p.categoriaId).map((x) => x.codigo), null);
      const proxima = perguntas.findIndex((x, i) => i > indice && x.categoriaId !== p.categoriaId);
      irPara(proxima === -1 ? total : proxima);
    };
    return (
      <Tela titulo="Check-up" voltar={!usuario?.precisaCheckup}>
        <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: 6 }} accessibilityLabel="Áreas do check-up">
          {areas.map((id, i) => (
            <View key={id} style={{ paddingHorizontal: 10, paddingVertical: 4, borderRadius: radius.full, borderWidth: 1,
              borderColor: i <= posicaoArea ? colors.primary : colors.border, backgroundColor: i === posicaoArea ? colors.primary : 'transparent' }}>
              <Text style={{ fontSize: 12, fontWeight: '700', color: i === posicaoArea ? '#0F172A' : i < posicaoArea ? colors.primary : colors.textMuted }}>
                {i < posicaoArea ? '\u2713 ' : ''}{nomesCategorias[id]}
              </Text>
            </View>
          ))}
        </View>
        <Text style={estilos.textoApoio}>Área {posicaoArea + 1} de {areas.length} · Pergunta {indice + 1} de {total}</Text>
        <Progresso valor={(indice + 1) / total} />
        <View style={{ paddingVertical: spacing.xxxl, gap: spacing.sm }}>
          <Text style={{ color: colors.primary, fontWeight: '800', textAlign: 'center', letterSpacing: 1 }}>{nomeArea.toUpperCase()}</Text>
          <Text style={[typography.h1, { color: colors.textPrimary, textAlign: 'center' }]} accessibilityRole="header">{p.texto}</Text>
          {!!p.ajuda && <Text style={[estilos.textoApoio, { textAlign: 'center' }]}>{p.ajuda}</Text>}
        </View>
        <Button title="Sim" onPress={() => responder(true)} fullWidth size="lg" />
        <Button title="Não" variant="outline" onPress={() => responder(false)} fullWidth size="lg" />
        <Button title="Pular esta pergunta" variant="ghost" onPress={() => responder(null)} fullWidth />
        <Button title={`Pular a área ${nomeArea}`} variant="ghost" onPress={pularArea} fullWidth />
        {indice > 0 && <Button title="Pergunta anterior" variant="ghost" onPress={() => setIndice(indice - 1)} fullWidth />}
        <Text style={[estilos.textoApoio, { textAlign: 'center' }]}>Você pode ajustar tudo depois.</Text>
      </Tela>
    );
  }

  // ── T05 · Resultado ────────────────────────────────────────────────────
  if (etapa === 'resultado' && resultado) {
    return (
      <Tela titulo="Resultado do check-up">
        {resultado.areasSemItens === resultado.areas.length ? (
          <>
            <Text style={[typography.h2, { color: colors.primary }]}>Seu Radar ainda está vazio</Text>
            <Text style={estilos.textoApoio}>
              Você não marcou nada no check-up, então ainda não sabemos se algum prazo precisa de atenção.
              Que tal refazer ou adicionar o primeiro item?
            </Text>
          </>
        ) : (
          <>
            <Text style={[typography.h2, { color: colors.primary }]}>{resultado.areasEmDia} de {resultado.areas.length} áreas em dia</Text>
            <Text style={estilos.textoApoio}>
              {resultado.itensCriados} prazo(s) adicionados ao seu Radar.
              {resultado.areasSemItens > 0 ? ` ${resultado.areasSemItens} área(s) ainda sem nada cadastrado.` : ''}
            </Text>
          </>
        )}
        {resultado.areas.map((a) => {
          const destaque = resultado.destaques.find((d) => d.categoriaId === a.categoriaId);
          return (
            <View key={a.categoriaId} style={[estilos.cartao, a.status === null && { borderStyle: 'dashed' }]}>
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text style={estilos.subtitulo}>{a.categoria}</Text>
                {a.status ? <StatusBadge status={a.status} /> : (
                  <View style={{ paddingHorizontal: 8, paddingVertical: 3, borderRadius: radius.full, borderWidth: 1, borderStyle: 'dashed', borderColor: colors.borderLight }}
                        accessibilityLabel="Status: sem itens">
                    <Text style={{ color: colors.textMuted, fontSize: 12, fontWeight: '700' }}>Sem itens</Text>
                  </View>
                )}
              </View>
              <Text style={estilos.textoApoio}>
                {a.quantidadeItens === 0 ? 'Nada cadastrado por aqui ainda.'
                  : destaque ? `${destaque.nome}: ${destaque.textoPrazo}` : `${a.quantidadeItens} item(ns) acompanhados`}
              </Text>
            </View>
          );
        })}
        <Button title={resultado.areasSemItens === resultado.areas.length ? 'Ir para o Radar' : 'Ver meu radar'} fullWidth onPress={() => {
          if (usuario?.precisaCheckup) atualizarUsuario({ precisaCheckup: false }); // o portão abre o Radar
          else if (navigation.canGoBack()) navigation.goBack();
          else navigation.navigate('MainTabs', { screen: 'RadarTab' });
        }} />
      </Tela>
    );
  }

  // ── T04 · Completar dados ──────────────────────────────────────────────
  const incluidos = modelos.filter((m) => dadosDe(m.id).incluir);
  const usaVeiculo = incluidos.some((m) => m.categoriaId === CATEGORIA_VEICULO);

  const verResultado = async () => {
    const e: Erros = usaVeiculo ? validarVeiculo(veiculo, true) : {};
    const req: CheckupRequest = {
      respostas: Object.entries(respostas).map(([codigo, resposta]) => ({ codigo, resposta })),
      veiculos: usaVeiculo ? [{ ref: 'v1', apelido: veiculo.apelido.trim(), tipo: veiculo.tipo,
        kmAtual: inteiro(veiculo.kmAtual) ?? 0, mediaKmMes: inteiro(veiculo.mediaKmMes) ?? 0 }] : [],
      itens: incluidos.map((m) => {
        const d = dadosDe(m.id);
        const unica = m.tipoVencimento === 'DataUnica';
        const iso = validarData(d.data, e, `${m.id}.data`, { obrigatoria: 'Preencha este campo.', naoFutura: !unica });
        const kmBase = inteiro(d.kmBase);
        if (m.tipoVencimento === 'UsoOuTempo') {
          if (kmBase === undefined || Number.isNaN(kmBase) || kmBase < 0) e[`${m.id}.kmBase`] = 'A km deve ser maior ou igual a 0.';
          else if (kmBase > (inteiro(veiculo.kmAtual) ?? Infinity)) e[`${m.id}.kmBase`] = 'Não pode ser maior que a km atual do veículo.';
        }
        return {
          modeloId: m.id,
          veiculoRef: m.categoriaId === CATEGORIA_VEICULO ? 'v1' : undefined,
          dataVencimento: unica ? iso : undefined,
          dataBase: unica ? undefined : iso,
          kmBase: m.tipoVencimento === 'UsoOuTempo' ? kmBase : undefined,
        };
      }),
    };
    setErros(e);
    setErroApi(Object.keys(e).length ? 'Confira os campos destacados em vermelho acima.' : null);
    if (Object.keys(e).length) return;

    setEnviando(true);
    try {
      setResultado(await catalogoApi.fazerCheckup(req));
      setEtapa('resultado');
    } catch (err) {
      const p = problema(err);
      const porCampo: Erros = {};
      for (const [campo, msgs] of Object.entries(p?.errors ?? {})) {
        const m = /^itens\[(\d+)\]\.(\w+)$/.exec(campo);
        const modelo = m ? incluidos[Number(m[1])] : undefined;
        if (modelo) porCampo[`${modelo.id}.${m![2] === 'kmBase' ? 'kmBase' : 'data'}`] = msgs[0];
      }
      if (Object.keys(porCampo).length) setErros(porCampo);
      else setErroApi(mensagemDeErro(err));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Tela titulo="Completar dados">
      <Text style={estilos.textoApoio}>Só o necessário para calcular os prazos. Os intervalos são sugestões.</Text>
      {modelos.length === 0 && <Text style={estilos.textoApoio}>Você não marcou nenhuma área. Tudo bem: dá para adicionar itens depois.</Text>}

      {usaVeiculo && (
        <View style={estilos.cartao}>
          <Text style={estilos.subtitulo}>Seu veículo</Text>
          <Input label="Apelido" placeholder="Ex.: Gol prata" value={veiculo.apelido} error={erros.apelido}
                 onChangeText={(t) => setVeiculo((v) => ({ ...v, apelido: t }))} />
          <Escolha label="Tipo" valor={veiculo.tipo} aoEscolher={(t) => setVeiculo((v) => ({ ...v, tipo: t }))}
                   opcoes={[{ id: 'Carro', rotulo: 'Carro' }, { id: 'Moto', rotulo: 'Moto' }]} />
          <Input label="Km atual" keyboardType="number-pad" value={veiculo.kmAtual} error={erros.kmAtual}
                 onChangeText={(t) => setVeiculo((v) => ({ ...v, kmAtual: t }))} />
          <Input label="Média de km por mês" keyboardType="number-pad" value={veiculo.mediaKmMes} error={erros.mediaKmMes}
                 hint="Um palpite já ajuda: 1.000 km/mês é comum." onChangeText={(t) => setVeiculo((v) => ({ ...v, mediaKmMes: t }))} />
        </View>
      )}

      {modelos.map((m, i) => {
        const d = dadosDe(m.id);
        const novaArea = i === 0 || modelos[i - 1].categoriaId !== m.categoriaId;
        return (
          <View key={m.id} style={{ gap: spacing.sm }}>
          {novaArea && <Text style={[typography.h3, { color: colors.primary, marginTop: spacing.sm }]}>{nomesCategorias[m.categoriaId]}</Text>}
          <View style={estilos.cartao}>
            <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', minHeight: 44 }}>
              <Text style={[estilos.subtitulo, { flex: 1 }]}>{m.nome}</Text>
              <Switch value={d.incluir} onValueChange={(v) => mudarItem(m.id, { incluir: v })} accessibilityLabel={`Incluir ${m.nome}`} />
            </View>
            {d.incluir && (
              <>
                <CampoData label={rotuloData[m.tipoVencimento]} valor={d.data} aoMudar={(t) => mudarItem(m.id, { data: t })}
                           erro={erros[`${m.id}.data`]} />
                {m.tipoVencimento === 'UsoOuTempo' && (
                  <Input label="Km na última vez" keyboardType="number-pad" value={d.kmBase} error={erros[`${m.id}.kmBase`]}
                         onChangeText={(t) => mudarItem(m.id, { kmBase: t })} />
                )}
              </>
            )}
          </View>
          </View>
        );
      })}

      {erroApi && <Alerta texto={erroApi} />}
      <Button title="Ver resultado" onPress={verResultado} loading={enviando} fullWidth />
      <Button title="Rever as perguntas" variant="ghost" onPress={() => { setIndice(0); setEtapa('perguntas'); }} fullWidth />
    </Tela>
  );
}
