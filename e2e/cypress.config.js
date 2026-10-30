import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    // Para testar outro endereço: $env:CYPRESS_BASE_URL = "https://..."
    baseUrl: 'http://localhost:5173',
    supportFile: 'cypress/support/e2e.js',
    specPattern: 'cypress/e2e/**/*.cy.js',
    viewportWidth: 1280,
    viewportHeight: 800,
    // A API pode levar alguns segundos para responder na primeira chamada
    defaultCommandTimeout: 15000,
    video: true,
    expose: {
      // Milissegundos de pausa entre os passos, para dar tempo de acompanhar na gravação
      pausa: 500,
    },
  },
});
