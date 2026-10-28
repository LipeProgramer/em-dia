import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
  Modal,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ArrowLeft, Car, Plus, Gauge, Trash2 } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { veiculosApi } from '../../api/services';
import type { VeiculoResponse } from '../../api/types';
import { TipoVeiculo } from '../../api/types';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { EmptyState } from '../../components/EmptyState';

type Props = NativeStackScreenProps<RootStackParamList, 'Veiculos'>;

export function VeiculosScreen({ navigation }: Props) {
  const [veiculos, setVeiculos] = useState<VeiculoResponse[]>([]);
  const [loading, setLoading] = useState(true);

  // Modal de Adição
  const [modalNovoAberto, setModalNovoAberto] = useState(false);
  const [nomeVeiculo, setNomeVeiculo] = useState('');
  const [placa, setPlaca] = useState('');
  const [kmAtual, setKmAtual] = useState('');
  const [salvando, setSalvando] = useState(false);

  // Modal de Atualizar Km
  const [veiculoKmSelecionado, setVeiculoKmSelecionado] = useState<VeiculoResponse | null>(null);
  const [novoKm, setNovoKm] = useState('');

  const carregarVeiculos = async () => {
    try {
      setLoading(true);
      const res = await veiculosApi.listar();
      setVeiculos(res);
    } catch {
      // Ignora erro
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarVeiculos();
  }, []);

  const handleSalvarVeiculo = async () => {
    if (!nomeVeiculo.trim()) {
      Alert.alert('Atenção', 'Informe o nome do veículo (ex: Honda Civic).');
      return;
    }

    try {
      setSalvando(true);
      await veiculosApi.criar({
        nome: nomeVeiculo.trim(),
        tipo: TipoVeiculo.Carro,
        placa: placa.trim() || null,
        kmAtual: kmAtual ? parseInt(kmAtual, 10) : 0,
        mediaKmPorMes: 1000,
      });

      setNomeVeiculo('');
      setPlaca('');
      setKmAtual('');
      setModalNovoAberto(false);
      await carregarVeiculos();
      Alert.alert('Sucesso!', 'Veículo cadastrado.');
    } catch {
      Alert.alert('Erro', 'Não foi possível cadastrar o veículo.');
    } finally {
      setSalvando(false);
    }
  };

  const handleSalvarKm = async () => {
    if (!veiculoKmSelecionado || !novoKm) return;
    try {
      await veiculosApi.atualizarKm(veiculoKmSelecionado.id, {
        km: parseInt(novoKm, 10),
      });
      setVeiculoKmSelecionado(null);
      setNovoKm('');
      await carregarVeiculos();
      Alert.alert('Sucesso!', 'Quilometragem atualizada.');
    } catch {
      Alert.alert('Erro', 'Não foi possível atualizar a quilometragem.');
    }
  };

  const handleExcluirVeiculo = (id: string) => {
    Alert.alert('Excluir Veículo', 'Tem certeza de que deseja remover este veículo?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Excluir',
        style: 'destructive',
        onPress: async () => {
          await veiculosApi.excluir(id);
          await carregarVeiculos();
        },
      },
    ]);
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.header}>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.goBack()}
          style={styles.backButton}
        >
          <ArrowLeft size={22} color={colors.textPrimary} strokeWidth={2} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Meus Veículos</Text>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => setModalNovoAberto(true)}
          style={styles.addButton}
        >
          <Plus size={22} color={colors.primary} strokeWidth={2.5} />
        </TouchableOpacity>
      </View>

      <ScrollView contentContainerStyle={styles.container}>
        {veiculos.length === 0 ? (
          <EmptyState
            icon={<Car size={36} color={colors.primary} strokeWidth={2} />}
            title="Nenhum veículo cadastrado"
            description="Cadastre seu carro ou moto para receber lembretes automáticos de IPVA, revisão e troca de óleo."
            actionTitle="Cadastrar Veículo"
            onAction={() => setModalNovoAberto(true)}
          />
        ) : (
          veiculos.map((v) => (
            <View key={v.id} style={styles.card}>
              <View style={styles.cardHeader}>
                <View style={styles.cardIconBox}>
                  <Car size={22} color={colors.primary} strokeWidth={2} />
                </View>
                <View style={styles.cardInfo}>
                  <Text style={styles.cardTitle}>{v.nome}</Text>
                  {v.placa && <Text style={styles.cardPlaca}>{v.placa}</Text>}
                </View>
                <TouchableOpacity
                  onPress={() => handleExcluirVeiculo(v.id)}
                  style={styles.deleteBtn}
                >
                  <Trash2 size={18} color={colors.error} strokeWidth={2} />
                </TouchableOpacity>
              </View>

              <View style={styles.odometerRow}>
                <View style={styles.odometerLeft}>
                  <Gauge size={18} color={colors.textMuted} strokeWidth={2} />
                  <Text style={styles.odometerLabel}>Quilometragem atual:</Text>
                </View>
                <Text style={styles.odometerValue}>{v.kmAtual.toLocaleString('pt-BR')} km</Text>
              </View>

              <Button
                title="Atualizar Quilometragem"
                onPress={() => {
                  setVeiculoKmSelecionado(v);
                  setNovoKm(String(v.kmAtual));
                }}
                variant="secondary"
                size="sm"
                style={styles.updateKmBtn}
              />
            </View>
          ))
        )}
      </ScrollView>

      {/* Modal Novo Veículo */}
      <Modal visible={modalNovoAberto} animationType="slide" transparent>
        <View style={styles.modalBackdrop}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Cadastrar Novo Veículo</Text>
            <Input
              label="Nome / Modelo *"
              placeholder="Ex: Honda Civic 2022"
              value={nomeVeiculo}
              onChangeText={setNomeVeiculo}
            />
            <Input
              label="Placa (opcional)"
              placeholder="Ex: BRA2E19"
              autoCapitalize="characters"
              value={placa}
              onChangeText={setPlaca}
            />
            <Input
              label="Quilometragem atual"
              placeholder="Ex: 45000"
              keyboardType="numeric"
              value={kmAtual}
              onChangeText={setKmAtual}
            />
            <View style={styles.modalActions}>
              <Button
                title="Cancelar"
                onPress={() => setModalNovoAberto(false)}
                variant="secondary"
                fullWidth={false}
                style={{ flex: 1 }}
              />
              <Button
                title="Salvar"
                onPress={handleSalvarVeiculo}
                variant="primary"
                loading={salvando}
                fullWidth={false}
                style={{ flex: 1 }}
              />
            </View>
          </View>
        </View>
      </Modal>

      {/* Modal Atualizar Km */}
      <Modal visible={!!veiculoKmSelecionado} animationType="slide" transparent>
        <View style={styles.modalBackdrop}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Atualizar Odômetro</Text>
            <Text style={styles.modalSubtitle}>
              Veículo: {veiculoKmSelecionado?.nome}
            </Text>
            <Input
              label="Nova quilometragem (km)"
              placeholder="Ex: 50200"
              keyboardType="numeric"
              value={novoKm}
              onChangeText={setNovoKm}
            />
            <View style={styles.modalActions}>
              <Button
                title="Cancelar"
                onPress={() => setVeiculoKmSelecionado(null)}
                variant="secondary"
                fullWidth={false}
                style={{ flex: 1 }}
              />
              <Button
                title="Confirmar"
                onPress={handleSalvarKm}
                variant="primary"
                fullWidth={false}
                style={{ flex: 1 }}
              />
            </View>
          </View>
        </View>
      </Modal>
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
  addButton: {
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
    paddingTop: spacing.md,
    gap: spacing.md,
  },
  card: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.md + 2,
  },
  cardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: spacing.md,
  },
  cardIconBox: {
    width: 42,
    height: 42,
    borderRadius: radius.md,
    backgroundColor: 'rgba(0, 200, 150, 0.15)',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  cardInfo: {
    flex: 1,
  },
  cardTitle: {
    ...typography.bodyBold,
    color: colors.textPrimary,
    fontSize: 16,
  },
  cardPlaca: {
    ...typography.caption,
    color: colors.textMuted,
  },
  deleteBtn: {
    padding: spacing.xs,
  },
  odometerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: colors.surfaceElevated,
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  odometerLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  odometerLabel: {
    ...typography.caption,
    color: colors.textSecondary,
  },
  odometerValue: {
    ...typography.bodyBold,
    color: colors.primary,
  },
  updateKmBtn: {
    marginTop: spacing.xs,
  },
  modalBackdrop: {
    flex: 1,
    backgroundColor: colors.overlay,
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.lg,
  },
  modalContent: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.xl,
    padding: spacing.xl,
    width: '100%',
    maxWidth: 400,
  },
  modalTitle: {
    ...typography.h2,
    color: colors.textPrimary,
    fontWeight: '800',
    marginBottom: spacing.xs,
  },
  modalSubtitle: {
    ...typography.body,
    color: colors.textMuted,
    marginBottom: spacing.lg,
  },
  modalActions: {
    flexDirection: 'row',
    gap: spacing.md,
    marginTop: spacing.md,
  },
});
