# Anime Feed Manager client

Simple web client developed using stencil, ionic, and akita (state management)

## Ionic PWA Toolkit

The PWA Toolkit is a starting point for building Progressive Web Apps using Ionic and Stencil.
This combination of tools gives you the ability to build a fast, efficient PWA out of the box.

For more info check out our [homepage](https://ionicframework.com/pwa/toolkit)!

## Features

* `@ionic/core` for the UI.
* Stencil for the application logic and routing
* Push Notifications setup
* Unit Tests
* Pre-rendering
* Lazy-loading and code splitting
* Intelligent Polyfills
* Modern mode: ES6/ESM for new browser, ES5 for older
* Service Worker, App manifest, iOS meta tags
* Theming using CSS variables

## Getting Started

To start building, clone this repo to a new directory:

```bash
npm init stencil ionic-pwa
```

## Production

To build for production, run:

```bash
npm run build
```

A production build includes:

* Minified code bundles
* Generated Service workers
* App manifest

## Service Workers

Service workers are generated via the Stencil build tool. For more information on how they can be configured, see the [Service Worker docs](https://stenciljs.com/docs/service-workers).

## Developing with a Service Worker

For most cases, you'll want to develop your app without generating a Service Worker. But if you'd like to test out Web Push Notifications or Background Sync, you'll need to have one generated. To generate a Service Worker during dev builds, we've added the npm script:

```
npm run start.sw
```

This will start a dev build and generate a Service Worker as well.

## Unit Tests

To run the unit tests once, run:

```bash
npm test
```

To run the unit tests and watch for file changes during development, run:

```bash
npm run test.watch
```

## Testing your PWA's performance

We recommend using https://www.webpagetest.org/easy with the `Run Lighthouse Audit` option turned on.
This will give you an in depth look into your app's load performance on the average device connected to the average network.
For more info on how to use webpagetest check out [this article](https://zoompf.com/blog/2015/07/the-seo-experts-guide-to-web-performance-using-webpagetest-2)
