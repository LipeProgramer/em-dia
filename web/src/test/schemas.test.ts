import { describe, expect, it } from 'vitest';
import { cadastroSchema, concluirSchema, itemSchema, veiculoSchema } from '../features/itens/schemas.ts';

// Validações do front (spec §5.5 e §6.5).

const itemBase = {
  nome: 'Filtro', categoriaId: '3', dataBase: '', dataVencimento: '', intervaloDias: '', intervaloKm: '',
  kmBase: '', garantiaFabricanteMeses: '', veiculoId: '', antecedenciaDias: '15', observacao: '', passos: [],
};

const erros = (r: { success: boolean; error?: { issues: { path: PropertyKey[]; message: string }[] } }) =>
  Object.fromEntries((r.error?.issues ?? []).map((i) => [i.path.join('.'), i.message]));

describe('schemas', () => {
  it('cadastro: e-mail inválido e senha fraca usam as mensagens da spec', () => {
    const r = cadastroSchema.safeParse({ nome: 'Ro', email: 'x', senha: 'abc', confirmarSenha: 'abc' });
    expect(erros(r)).toMatchObject({ email: 'Digite um e-mail válido.', senha: 'Use pelo menos 8 caracteres, com letras e números.' });
  });

  it('item recorrente sem intervalo → "Informe o intervalo em dias."', () => {
    const r = itemSchema.safeParse({ ...itemBase, tipoVencimento: 'RecorrentePorTempo', dataBase: '2026-01-01' });
    expect(erros(r)).toMatchObject({ intervaloDias: 'Informe o intervalo em dias.' });
  });

  it('item uso ou tempo exige veículo, data, km e intervalos', () => {
    const r = itemSchema.safeParse({ ...itemBase, tipoVencimento: 'UsoOuTempo' });
    expect(Object.keys(erros(r))).toEqual(expect.arrayContaining(['veiculoId', 'dataBase', 'kmBase', 'intervaloKm', 'intervaloDias']));
  });

  it('antecedência fora de 0 a 365 → "Escolha entre 0 e 365 dias."', () => {
    const r = itemSchema.safeParse({ ...itemBase, tipoVencimento: 'DataUnica', dataVencimento: '2027-01-01', antecedenciaDias: '400' });
    expect(erros(r)).toMatchObject({ antecedenciaDias: 'Escolha entre 0 e 365 dias.' });
  });

  it('item válido converte os números', () => {
    const r = itemSchema.parse({ ...itemBase, tipoVencimento: 'RecorrentePorTempo', dataBase: '2026-01-01', intervaloDias: '180' });
    expect(r).toMatchObject({ categoriaId: 3, intervaloDias: 180, antecedenciaDias: 15 });
  });

  it('concluir: km menor que a anterior e data futura', () => {
    const r = concluirSchema({ exigeKm: true, kmMinima: 75000 }).safeParse({ dataConclusao: '2999-01-01', km: '70000' });
    expect(erros(r)).toMatchObject({ dataConclusao: 'A data não pode estar no futuro.', km: 'A km não pode ser menor que 75.000.' });
  });

  it('veículo: média entre 1 e 20.000', () => {
    const r = veiculoSchema.safeParse({ apelido: 'Gol', tipo: 'Carro', kmAtual: '100', mediaKmMes: '30000' });
    expect(erros(r)).toMatchObject({ mediaKmMes: 'Informe uma média entre 1 e 20.000.' });
  });
});
