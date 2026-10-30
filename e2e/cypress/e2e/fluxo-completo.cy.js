// O caminho de uma pessoa no Em Dia, do cadastro à exclusão da conta. Os testes rodam em sequência no mesmo navegador
// (testIsolation: false), como uma história contínua: cada um continua de onde o anterior parou.
// Cada execução cria uma conta nova e a apaga no fim.

const email = `demo.${Date.now()}@emdia.test`;
const senha = `Demo${Date.now() % 100000}abc`;

/** Data no formato do campo de data: aaaa-mm-dd, contando dias a partir de hoje. */
const dias = (quantos) => {
  const d = new Date();
  d.setDate(d.getDate() + quantos);
  const dois = (n) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${dois(d.getMonth() + 1)}-${dois(d.getDate())}`;
};

/** Preenche a data (e a km, se houver) de um modelo na tela "Completar dados". */
const dadosDoModelo = (nome, data, km) => {
  cy.contains('.modelo', nome).within(() => {
    cy.get('input[type=date]').type(data);
    if (km) cy.get('input[type=number]').clear().type(String(km));
  });
};

const area = (nome) => {
  cy.contains('.trilha-areas button', nome).click();
  cy.pausa();
};

const pergunta = (texto, resposta) => {
  cy.contains('h1', texto).should('be.visible');
  cy.pausa();
  cy.clicar(resposta);
};

const menu = (nome) => {
  cy.contains('nav[aria-label="Menu principal"] a', nome).click();
  cy.pausa();
};

describe('Em Dia: da criação da conta à exclusão', { testIsolation: false }, () => {
  before(() => {
    cy.clearAllCookies();
  });

  // Cada teste continua de onde o anterior parou: se um falha, os seguintes não fazem sentido
  afterEach(function () {
    if (this.currentTest.state === 'failed') Cypress.stop();
  });

  it('Cadastro: a validação mostra o que falta, sem chamar a API', () => {
    cy.visit('/cadastro');
    cy.clicar('Criar conta');
    cy.contains('O nome deve ter entre 2 e 100 caracteres.').should('be.visible');

    cy.preencher('Como quer ser chamado?', 'Rodrigo');
    cy.preencher('E-mail', email);
    cy.preencher(/^Senha$/, 'abc');
    cy.preencher('Confirmar senha', 'abc');
    cy.clicar('Criar conta');
    cy.contains('Use pelo menos 8 caracteres, com letras e números.').should('be.visible');
    cy.location('pathname').should('eq', '/cadastro');
    cy.pausa();
  });

  it('Cadastro: com os dados certos, a conta é criada e o check-up abre', () => {
    cy.preencher(/^Senha$/, senha);
    cy.preencher('Confirmar senha', senha);
    cy.clicar('Criar conta');
    cy.location('pathname').should('eq', '/checkup');
  });

  it('Check-up: responde as perguntas e muda de área pela trilha', () => {
    pergunta('Você tem carro ou moto?', 'Sim');
    pergunta('Seu veículo tem seguro?', 'Não');

    area('Documentos');
    pergunta('Você tem CNH?', 'Sim');

    area('Garantias');
    pergunta('Comprou algum eletrônico', 'Não');
    pergunta('Comprou algum eletrodoméstico', 'Não');
    cy.location('pathname').should('eq', '/checkup/dados');
  });

  it('Check-up: completa os dados e vê o resultado por área', () => {
    cy.preencher('Apelido', 'Gol prata');
    cy.preencher('Km atual', 52000);
    cy.preencher('Média de km por mês', 1200);

    dadosDoModelo('IPVA', dias(-350));
    dadosDoModelo('Licenciamento', dias(-400));
    dadosDoModelo('Troca de óleo', dias(-60), 45000);
    dadosDoModelo('CNH', dias(700));
    cy.pausa();

    cy.clicar('Ver resultado');
    cy.location('pathname').should('eq', '/checkup/resultado');
    cy.contains('Resultado do check-up').should('be.visible');
    cy.pausa();
  });

  it('Radar: mostra os prazos e o modo foco deixa só os mais urgentes', () => {
    cy.clicar('Ver meu radar');
    cy.contains('h1', 'Seu radar').should('be.visible');
    cy.contains('Precisa de atenção').should('be.visible');
    cy.contains('Licenciamento').should('be.visible');
    cy.pausa();

    cy.contains('button', 'Modo foco').click();
    cy.contains('Modo foco: só os 3 itens mais urgentes').should('be.visible');
    cy.pausa();
    cy.contains('button', 'Modo foco').click();
    cy.contains('Modo foco: só os 3 itens mais urgentes').should('not.exist');
    cy.pausa();
  });

  it('Itens: cria um item novo do zero', () => {
    menu('Novo');
    cy.clicar('Criar do zero');
    cy.preencher('Nome', 'Passaporte');
    cy.campo('Categoria').select('Documentos');
    cy.campo('Tipo de vencimento').select('DataUnica');
    cy.preencher('Data de vencimento', dias(20));
    cy.pausa();
    cy.clicar('Salvar item');

    cy.contains('h1', 'Passaporte').should('be.visible');
    cy.contains('faltam 20 dias').should('be.visible');
    cy.pausa();
  });

  it('Itens: edita o nome do item', () => {
    cy.clicar('Editar');
    cy.campo('Categoria').find('option:selected').should('have.text', 'Documentos');
    cy.preencher('Nome', 'Renovar passaporte');
    cy.pausa();
    cy.clicar('Salvar alterações');
    cy.contains('h1', 'Renovar passaporte').should('be.visible');
    cy.pausa();
  });

  it('Itens: consulta a lista e filtra por situação', () => {
    menu('Itens');
    cy.contains('Renovar passaporte').should('be.visible');
    cy.contains('Troca de óleo').should('be.visible');
    cy.pausa();

    cy.contains('[aria-label="Filtrar por situação"] button', 'Vencidos').click();
    cy.contains('Licenciamento').should('be.visible');
    cy.contains('Renovar passaporte').should('not.exist');
    cy.pausa();
    cy.contains('[aria-label="Filtrar por situação"] button', 'Todos').click();
    cy.contains('Renovar passaporte').should('be.visible');
    cy.pausa();
  });

  it('Concluir: o item vencido é concluído e ganha um novo prazo', () => {
    cy.contains('a', 'Licenciamento').click();
    cy.contains('h1', 'Licenciamento').should('be.visible');
    cy.pausa();
    cy.clicar('Concluir');

    cy.get('dialog[open]').within(() => {
      cy.contains('Quando você concluiu?').should('be.visible');
      cy.pausa();
      cy.contains('button', /^\s*Concluir\s*$/).click();
      cy.contains('Concluído!').should('be.visible');
      cy.pausa();
      cy.contains('a, button', 'Voltar ao Radar').click();
    });
    cy.contains('h1', 'Seu radar').should('be.visible');
    cy.pausa();
  });

  it('Histórico: a conclusão aparece aqui', () => {
    menu('Histórico');
    cy.contains('Licenciamento').should('be.visible');
    cy.pausa();
  });

  it('Veículo: a km não pode diminuir, e pode aumentar', () => {
    menu('Perfil');
    cy.clicar('Meus veículos');
    cy.contains('Gol prata').should('be.visible');
    cy.clicar('Atualizar km');

    cy.get('dialog[open]').within(() => {
      cy.preencher('Km atual', 40000);
      cy.clicar('Atualizar');
      cy.contains('A km não pode ser menor que').should('be.visible');
      cy.pausa();
      cy.preencher('Km atual', 53000);
      cy.clicar('Atualizar');
      cy.contains('Km atualizada para').should('be.visible');
      cy.pausa();
    });
    cy.get('dialog[open] [aria-label="Fechar"]').click();
    cy.pausa();
  });

  it('Veículo: quem tem itens ligados não pode ser excluído', () => {
    cy.contains('li', 'Gol prata').contains('button', 'Excluir').click();
    cy.get('dialog[open]').within(() => {
      cy.clicar('Excluir');
      cy.get('[role=alert]').should('be.visible');
      cy.pausa();
    });
    cy.get('dialog[open] [aria-label="Fechar"]').click();
    cy.pausa();
  });

  it('Itens: exclui um item', () => {
    menu('Itens');
    cy.contains('a', 'Renovar passaporte').click();
    cy.contains('h1', 'Renovar passaporte').should('be.visible');
    cy.contains('button', 'Excluir').click();
    cy.get('dialog[open]').within(() => {
      cy.contains('Excluir este item?').should('be.visible');
      cy.pausa();
      cy.clicar('Excluir');
    });
    cy.location('pathname').should('eq', '/itens');
    cy.contains('Renovar passaporte').should('not.exist');
    cy.pausa();
  });

  it('Perfil: salva as preferências de aviso', () => {
    menu('Perfil');
    cy.contains(email).should('be.visible');
    cy.preencher('Quantos reavisos, no máximo?', 2);
    cy.clicar('Salvar preferências');
    cy.contains('Preferências salvas.').should('be.visible');
    cy.pausa();
  });

  it('Login: senha errada é recusada com mensagem genérica', () => {
    cy.clicar('Sair');
    cy.location('pathname').should('eq', '/entrar');
    cy.preencher('E-mail', email);
    cy.preencher('Senha', 'SenhaErrada1');
    cy.clicar('Entrar');
    cy.contains('E-mail ou senha inválidos.').should('be.visible');
    cy.pausa();
  });

  it('Login: com a senha certa, volta para o Radar', () => {
    cy.preencher('Senha', senha);
    cy.clicar('Entrar');
    cy.contains('h1', 'Seu radar').should('be.visible');
    cy.pausa();
  });

  it('Conta: excluir apaga tudo e volta para a tela de entrar', () => {
    menu('Perfil');
    cy.clicar('Excluir conta');
    cy.get('dialog[open]').within(() => {
      cy.preencher('Digite sua senha para confirmar', senha);
      cy.pausa();
      cy.clicar('Excluir minha conta');
    });
    cy.location('pathname').should('eq', '/entrar');
    cy.contains('h1', 'Entrar').should('be.visible');
  });
});
