importScripts('workbox-v5.1.2/workbox-sw.js');
workbox.routing.registerRoute(new RegExp('^https://animefeedmanager.azure-api.net/api/.*'),
  new workbox.strategies.NetworkFirst({
    cacheName: 'api-calls',
    plugins: [
      new workbox.expiration.ExpirationPlugin({
        maxEntries: 30,
      }),
    ]

  })
);

workbox.precaching.precacheAndRoute(self.__WB_MANIFEST)

