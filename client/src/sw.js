importScripts('workbox-v5.1.3/workbox-sw.js');
workbox.routing.registerRoute(new RegExp('^https://animefeedmanager.azure-api.net/api/(library|seasons).*'),
  new workbox.strategies.CacheFirst({
    cacheName: 'api-calls',
    plugins: [
      new workbox.expiration.ExpirationPlugin({
        maxEntries: 30,
        maxAgeSeconds: 60 * 60 * 24 * 5
      }),
    ]

  })
);

workbox.routing.registerRoute(new RegExp('^https://animefeedmanager.azure-api.net/api/subscriptions/.*'),
  new workbox.strategies.NetworkFirst({
    cacheName: 'api-subscriptions',
    plugins: [
      new workbox.expiration.ExpirationPlugin({
        maxEntries: 30,
        maxAgeSeconds: 60 * 60 * 24 * 2
      }),
    ]

  })
);

workbox.precaching.precacheAndRoute(self.__WB_MANIFEST)

