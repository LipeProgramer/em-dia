import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import {
  ArrowLeft,
  FileText,
  Car,
  HeartPulse,
  Home,
  Package,
  Layers,
  Search,
  Calendar,
  DollarSign,
  ChevronRight,
  Check,
} from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { Input } from '../../components/Input';
import { Button } from '../../components/Button';
import { itensApi } from '../../api/services';
import { useRadar } from '../../context/RadarContext';
import { TipoVencimento } from '../../api/types';

type Props = NativeStackScreenProps<RootStackParamList, 'AdicionarItem'>;

interface CategoriaOpcao {
  id: number;
  nome: string;
  descricao: string;
  icone: any;
  cor: string;
  sugestoes: string[];
}

const CATEGORIAS: CategoriaOpcao[] = [
  {
    id: 1,
    nome: 'Documento',
    descricao: 'CNH, RG, passaporte, vistos, etc.',
    icone: FileText,
    cor: '#38BDF8',
    sugestoes: ['Renovar CNH', 'Passaporte', 'Certificado Digital', 'Identidade'],
  },
  {
    id: 2,
    nome: 'Veículo',
    descricao: 'IPVA, licenciamento, revisão, seguro, etc.',
    icone: Car,
    cor: colors.primary,
    sugestoes: ['IPVA', 'Licenciamento Anual', 'Revisão de 10.000 km', 'Troca de óleo'],
  },
  {
    id: 3,
    nome: 'Saúde',
    descricao: 'Vacinas, exames, consultas, receitas, etc.',
    icone: HeartPulse,
    cor: '#F472B6',
    sugestoes: ['Vacina da Gripe', 'Check-up Anual', 'Exames de Sangue', 'Dentista'],
  },
  {
    id: 4,
    nome: 'Casa',
    descricao: 'Aluguel, condomínio, IPTU, gás, etc.',
    icone: Home,
    cor: '#FBBF24',
    sugestoes: ['Aluguel', 'Condomínio', 'IPTU', 'Conta de Energia'],
  },
  {
    id: 5,
    nome: 'Produto',
    descricao: 'Garantia, troca, manutenção, etc.',
    icone: Package,
    cor: '#A78BFA',
    sugestoes: ['Garantia do Celular', 'Garantia Eletrodoméstico', 'Revisão Filtro'],
  },
  {
    id: 6,
    nome: 'Outro',
    descricao: 'Contratos, assinaturas, seguros em geral',
    icone: Layers,
    cor: colors.textMuted,
    sugestoes: ['Seguro de Vida', 'Renovação de Contrato', 'Assinatura Anual'],
  },
];

export function AdicionarItemScreen({ route, navigation }: Props) {
  const { carregarDados } = useRadar();
  const categoriaInicial = route.params?.categoriaId || 1;

  const [busca, setBusca] = useState('');
  const [categoriaSelecionada, setCategoriaSelecionada] = useState<number>(categoriaInicial);
  const [nome, setNome] = useState('');
  const [dataVencimento, setDataVencimento] = useState(
    new Date(Date.now() + 15 * 86400000).toISOString().split('T')[0]
  );
  const [valorEstimado, setValorEstimado] = useState('');
  const [observacoes, setObservacoes] = useState('');
  const [salvando, setSalvando] = useState(false);

  const categoriaAtual = CATEGORIAS.find((c) => c.id === categoriaSelecionada) || CATEGORIAS[0];

  // Filtra categorias pela busca caso digitado
  const categoriasFiltradas = CATEGORIAS.filter(
    (c) =>
      c.nome.toLowerCase().includes(busca.toLowerCase()) ||
      c.descricao.toLowerCase().includes(busca.toLowerCase()) ||
      c.sugestoes.some((s) => s.toLowerCase().includes(busca.toLowerCase()))
  );

  const handleSalvar = async () => {
    if (!nome.trim()) {
      Alert.alert('Atenção', 'Informe o nome do vencimento.');
      return;
    }

    try {
      setSalvando(true);
      await itensApi.criar({
        nome: nome.trim(),
        categoriaId: categoriaSelecionada,
        tipoVencimento: TipoVencimento.DataUnica,
        dataVencimento,
        valorEstimado: valorEstimado ? parseFloat(valorEstimado.replace(',', '.')) : null,
        observacoes: observacoes.trim() || null,
        passos: [
          'Verificar valor e comprovante',
          'Acessar plataforma de pagamento',
          'Realizar pagamento e arquivar recibo',
        ],
      });

      await carregarDados();
      Alert.alert('Sucesso!', 'Vencimento adicionado com sucesso ao radar.');
      navigation.goBack();
    } catch {
      Alert.alert('Erro', 'Não foi possível salvar o vencimento.');
    } finally {
      setSalvando(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={{ flex: 1 }}
      >
        {/* Header */}
        <View style={styles.header}>
          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() => navigation.goBack()}
            style={styles.backButton}
          >
            <ArrowLeft size={22} color={colors.textPrimary} strokeWidth={2} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Adicionar tarefa</Text>
          <View style={{ width: 40 }} />
        </View>

        <ScrollView
          contentContainerStyle={styles.container}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {/* Campo de Busca de Tipo */}
          <Input
            placeholder="Qual tipo de vencimento? Buscar..."
            value={busca}
            onChangeText={setBusca}
            leftIcon={<Search size={18} color={colors.textMuted} strokeWidth={2} />}
          />

          {/* Seleção de Categoria */}
          <Text style={styles.sectionLabel}>Categoria</Text>
          <View style={styles.categoriesList}>
            {categoriasFiltradas.map((cat) => {
              const IconComp = cat.icone;
              const isSelected = cat.id === categoriaSelecionada;

              return (
                <TouchableOpacity
                  key={cat.id}
                  activeOpacity={0.7}
                  onPress={() => {
                    setCategoriaSelecionada(cat.id);
                    if (!nome) setNome(cat.sugestoes[0]);
                  }}
                  style={[
                    styles.catCard,
                    isSelected && styles.catCardSelected,
                  ]}
                >
                  <View style={[styles.catIconCircle, { backgroundColor: `${cat.cor}18` }]}>
                    <IconComp size={20} color={cat.cor} strokeWidth={2} />
                  </View>
                  <View style={styles.catInfo}>
                    <Text style={styles.catName}>{cat.nome}</Text>
                    <Text style={styles.catDesc} numberOfLines={1}>
                      {cat.descricao}
                    </Text>
                  </View>
                  {isSelected ? (
                    <Check size={18} color={colors.primary} strokeWidth={2.5} />
                  ) : (
                    <ChevronRight size={18} color={colors.borderLight} />
                  )}
                </TouchableOpacity>
              );
            })}
          </View>

          {/* Sugestões rápidas para facilitar TDAH */}
          <Text style={styles.sectionLabel}>Sugestões rápidas</Text>
          <View style={styles.sugestoesRow}>
            {categoriaAtual.sugestoes.map((sugestao) => (
              <TouchableOpacity
                key={sugestao}
                activeOpacity={0.7}
                onPress={() => setNome(sugestao)}
                style={[
                  styles.sugestaoChip,
                  nome === sugestao && styles.sugestaoChipSelected,
                ]}
              >
                <Text
                  style={[
                    styles.sugestaoText,
                    nome === sugestao && styles.sugestaoTextSelected,
                  ]}
                >
                  {sugestao}
                </Text>
              </TouchableOpacity>
            ))}
          </View>

          {/* Campos do Formulário */}
          <Text style={styles.sectionLabel}>Detalhes do vencimento</Text>

          <Input
            label="Nome do item *"
            placeholder="Ex: IPVA 2026"
            value={nome}
            onChangeText={setNome}
          />

          <Input
            label="Data de vencimento (AAAA-MM-DD) *"
            placeholder="Ex: 2026-11-15"
            value={dataVencimento}
            onChangeText={setDataVencimento}
            leftIcon={<Calendar size={18} color={colors.textMuted} strokeWidth={2} />}
          />

          <Input
            label="Valor estimado (R$) (opcional)"
            placeholder="Ex: 1250,00"
            keyboardType="decimal-pad"
            value={valorEstimado}
            onChangeText={setValorEstimado}
            leftIcon={<DollarSign size={18} color={colors.textMuted} strokeWidth={2} />}
          />

          <Input
            label="Onde pagar / Observações (opcional)"
            placeholder="Ex: Pelo aplicativo do Banco ou site da Sefaz"
            value={observacoes}
            onChangeText={setObservacoes}
          />

          {/* Botão Salvar */}
          <Button
            title="Salvar Vencimento no Radar"
            onPress={handleSalvar}
            variant="primary"
            size="lg"
            loading={salvando}
            style={styles.submitBtn}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.sm,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: radius.md,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerTitle: {
    ...typography.h3,
    color: colors.textPrimary,
    fontWeight: '700',
  },
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  sectionLabel: {
    ...typography.caption,
    color: colors.textSecondary,
    fontWeight: '700',
    marginTop: spacing.md,
    marginBottom: spacing.sm,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  categoriesList: {
    gap: spacing.sm,
    marginBottom: spacing.md,
  },
  catCard: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
  },
  catCardSelected: {
    borderColor: colors.primary,
    backgroundColor: '#162334',
  },
  catIconCircle: {
    width: 38,
    height: 38,
    borderRadius: radius.md,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  catInfo: {
    flex: 1,
  },
  catName: {
    ...typography.bodyBold,
    color: colors.textPrimary,
  },
  catDesc: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 1,
  },
  sugestoesRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
    marginBottom: spacing.md,
  },
  sugestaoChip: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.full,
    paddingVertical: spacing.xs + 2,
    paddingHorizontal: spacing.md,
  },
  sugestaoChipSelected: {
    borderColor: colors.primary,
    backgroundColor: 'rgba(0, 200, 150, 0.15)',
  },
  sugestaoText: {
    ...typography.caption,
    color: colors.textSecondary,
  },
  sugestaoTextSelected: {
    color: colors.primary,
    fontWeight: '700',
  },
  submitBtn: {
    marginTop: spacing.xl,
  },
});
