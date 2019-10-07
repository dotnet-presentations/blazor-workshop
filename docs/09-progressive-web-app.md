# Progressive Web App (PWA) features

The term *Progressive Web App (PWA)* refers to web applications that make use of certain modern browser APIs to create a native-like app experience that integrates with the user's desktop or mobile OS. For example:

 * Installing into the OS task bar or home screen
 * Working offline
 * Receiving push notifications

Blazor uses standard web technologies, which means you can take advantage of these browser APIs, just as you could with other modern web frameworks.

## Adding a service worker

As a prerequisite to most of the PWA-type APIs, your application will need a *service worker*. This is a JavaScript file, usually quite small, that provides event handlers that the browser can invoke outside the context of your running application, for example when fetching resources from your domain, or when a push notification arrives. You can learn more about service workers in Google's [Web Fundamentals guide](https://developers.google.com/web/fundamentals/primers/service-workers).

Even though Blazor applications are built in .NET, your service worker will still be JavaScript because it runs outside the context of your application. Technically it would be possible to create a service worker that starts up the Mono WebAssembly runtime and then runs .NET code within the service worker context, but this is a lot of work that may be unnecessary considering that you might only need a few lines of JavaScript code.

To add a service worker, create a file called `service-worker.js` in your client app's `wwwroot` directiry, containing:

```js
self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    self.skipWaiting();
});

self.addEventListener('fetch', event => {
    // You can add custom logic here for controlling whether to use cached data if offline, etc.
    // The following line opts out, so requests go directly to the network as usual.
    return null;
});
```

This service worker doesn't really do anything yet. It just installs itself, and then whenever any `fetch` event occurs (meaning that the browser is performing an HTTP request to your origin), it simply opts out of processing the request so that the browser handles it normally. If you want, you can come back to this file later and add some more advanced functionality like offline support, but we don't need that just yet.

Enable the service worker by adding the following `<script>` elements into your `index.html` file, for example beneath the other `<script>` elements:

```js
<script>navigator.serviceWorker.register('service-worker.js');</script>
```

If you run your app now, then in the browser's dev tools console, you should see it write the following message:

```
Installing service worker...
```

Note that this only happens during the first page load after each time you modify `service-worker.js`. It doesn't re-install on each load if that file's contents (compared byte-for-byte) haven't changed. Try it out: check that you can make some trivial change to the file (such as adding a comment or changing whitespace) and observe that it reinstalls after those changes, but not at other times.

This might not seem to achieve anything yet, but is a prerequisite for the following steps.

## Making your app installable

Next, let's make it possible to install Blazing Pizza into your OS. This uses a browser feature in Chrome/Edge-beta on Windows/Mac/Linux, or Safari/Chrome for iOS/Android. It may not yet be implemented on other browsers such as Firefox.

First add a file called `manifest.json` in your client app's `wwwroot`, containing:

```js
{
  "short_name": "Blazing Pizza",
  "name": "Blazing Pizza",
  "icons": [
    {
      "src": "img/icon-512.png",
      "type": "image/png",
      "sizes": "512x512"
    }
  ],
  "start_url": "/",
  "background_color": "#860000",
  "display": "standalone",
  "scope": "/",
  "theme_color": "#860000"
}
```

You can probably guess what this information is used for. It will determine how your app will be presented to the user once installed into the OS. Feel free to change the text or colors if you want.

Next you'll need to tell the browser where to find this file. Add the following element into the `<head>` section of your `index.html`:

```html
<link rel="manifest" href="manifest.json" />
```

... and that's it! Next time you load the site in Chrome or Edge beta, you'll see a new icon in the address bar that prompts users to install the app:

![image](https://user-images.githubusercontent.com/1101362/66352975-d1eee900-e958-11e9-9042-85ea4ac0c56b.png)

Users on mobile devices would reach the same functionality via an option called *Add to home screen* or similar.

Once installed, the app will appear as a standalone app in its own window with no other browser UI:

![image](https://user-images.githubusercontent.com/1101362/66353128-3611ad00-e959-11e9-8c03-4ef6212528ba.png)

Users on Windows will also find it on their start menu, and can pin it to their taskbar if desired. Similar options exist on macOS.

