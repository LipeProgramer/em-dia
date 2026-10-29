// Service worker do Em Dia: só mostra as notificações de vencimento (Web Push) e abre a tela certa ao tocar.
// Não guarda páginas em cache: o site sempre carrega a versão publicada.

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()));

self.addEventListener('push', (event) => {
  let dados = {};
  try {
    dados = event.data ? event.data.json() : {};
  } catch {
    dados = { corpo: event.data ? event.data.text() : '' };
  }

  event.waitUntil(
    self.registration.showNotification(dados.titulo || 'Em Dia', {
      body: dados.corpo || 'Você tem prazos chegando.',
      icon: '/icons/icon-192.png',
      badge: '/icons/icon-192.png',
      data: { url: dados.url || '/' },
    }),
  );
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const url = new URL((event.notification.data && event.notification.data.url) || '/', self.location.origin).href;

  event.waitUntil((async () => {
    const janelas = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
    const aberta = janelas.find((j) => new URL(j.url).origin === self.location.origin);
    if (aberta) {
      await aberta.focus();
      return aberta.navigate(url);
    }
    return self.clients.openWindow(url);
  })());
});
