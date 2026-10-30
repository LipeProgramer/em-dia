// Comandos usados pelos testes. Os campos são achados pelo texto do rótulo, como uma pessoa os acha.

/** Pausa curta para quem assiste acompanhar (Cypress.expose('pausa'); 0 desliga). */
Cypress.Commands.add('pausa', () => cy.wait(Number(Cypress.expose('pausa') ?? 0), { log: false }));

/** Campo pelo rótulo. Ex.: cy.campo('E-mail') ou cy.campo(/^Senha$/). */
Cypress.Commands.add('campo', (rotulo) =>
  cy.contains('label', rotulo).invoke('attr', 'for').then((id) => cy.get(`[id="${id}"]`)));

/** Digita devagar o bastante para aparecer na gravação. */
Cypress.Commands.add('preencher', (rotulo, valor) => {
  cy.campo(rotulo).clear();
  cy.campo(rotulo).type(String(valor), { delay: 25 });
});

/** Clica no botão ou link com este texto exato. */
Cypress.Commands.add('clicar', (texto) => {
  cy.get('button, a')
    .filter(':visible')
    .filter((_, el) => el.textContent.trim() === texto)
    .first()
    .click();
  cy.pausa();
});
