// Offline service worker - caches app shell on install
const CACHE_NAME = 'breakout-cache-v1';

self.addEventListener('install', event => {
    event.waitUntil(caches.open(CACHE_NAME));
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => response || fetch(event.request))
    );
});
