// Offline service worker - caches app shell on install
const CACHE_NAME = 'breakout-cache-v1';

self.addEventListener('install', event => {
    event.waitUntil(caches.open(CACHE_NAME));
});

self.addEventListener('fetch', event => {
    if (event.request.mode === 'navigate' || event.request.destination === 'document') {
        event.respondWith(
            fetch(event.request).catch(() => caches.match(event.request))
        );
        return;
    }

    event.respondWith(
        caches.match(event.request).then(response => response || fetch(event.request))
    );
});
