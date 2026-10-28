import { describe, expect, it } from 'vitest';
import { brParaIso, dataBr, mascaraData } from '../utils/formato';
import { cadastroSchema, errosDe, validarItem, validarVeiculo, type ItemTexto } from '../utils/validacao';

// Validações do app (spec §5.5 e §6.5) — mesmas mensagens do front web.

const item: ItemTexto = {
  nome: 'Filtro', categoriaId: 3, tipoVencimento: 'RecorrentePorTempo', dataBase: '01/05/2026', dataVencimento: '',
  intervaloDias: '180', intervaloKm: '', kmBase: '', garantiaFabricanteMeses: '', veiculoId: null, antecedenciaDias: '15', passos: [],
};

describe('datas', () => {
  it('máscara e conversão dd/mm/aaaa ↔ ISO', () => {
    expect(mascaraData('15102026')).toBe('15/10/2026');
    expect(brParaIso('15/10/2026')).toBe('2026-10-15');
    expect(brParaIso('31/02/2026')).toBeUndefined();
    expect(dataBr('2026-10-15')).toBe('15/10/2026');
  });
});

describe('validações', () => {
  it('CT47 · senha sem número mostra a mensagem da spec', () => {
    const e = errosDe(cadastroSchema.safeParse({ nome: 'Rodrigo', email: 'r@e.com', senha: 'semnumero', confirmarSenha: 'semnumero' }));
    expect(e.senha).toBe('Use pelo menos 8 caracteres, com letras e números.');
  });

  it('item recorrente válido não tem erros', () => {
    expect(validarItem(item)).toEqual({});
  });

  it('recorrente sem intervalo → "Informe o intervalo em dias."', () => {
    expect(validarItem({ ...item, intervaloDias: '' }).intervaloDias).toBe('Informe o intervalo em dias.');
  });

  it('data da última vez no futuro → "A data não pode estar no futuro."', () => {
    expect(validarItem({ ...item, dataBase: '01/01/2999' }).dataBase).toBe('A data não pode estar no futuro.');
  });

  it('uso ou tempo exige veículo, km e intervalos', () => {
    const e = validarItem({ ...item, tipoVencimento: 'UsoOuTempo', intervaloDias: '' });
    expect(Object.keys(e)).toEqual(expect.arrayContaining(['veiculoId', 'kmBase', 'intervaloKm', 'intervaloDias']));
  });

  it('antecedência fora de 0 a 365', () => {
    expect(validarItem({ ...item, antecedenciaDias: '400' }).antecedenciaDias).toBe('Escolha entre 0 e 365 dias.');
  });

  it('veículo: média entre 1 e 20.000 e km ≥ 0', () => {
    const e = validarVeiculo({ apelido: 'Gol', kmAtual: '-1', mediaKmMes: '30000' }, true);
    expect(e).toMatchObject({ kmAtual: 'A km deve ser maior ou igual a 0.', mediaKmMes: 'Informe uma média entre 1 e 20.000.' });
  });
});
