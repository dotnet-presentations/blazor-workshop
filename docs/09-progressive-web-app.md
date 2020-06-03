# Progressive Web App (PWA) features

The term *Progressive Web App (PWA)* refers to web applications that make use of certain modern browser APIs to create a native-like app experience that integrates with the user's desktop or mobile OS. For example:

 * Installing into the OS task bar or home screen
 * Working offline
 * Receiving push notifications

Blazor uses standard web technologies, which means you can take advantage of these browser APIs, just as you could with other modern web frameworks.

## Adding a service worker

As a prerequisite to most of the PWA-type APIs, your application will need a *service worker*. This is a JavaScript file that is usually quite small. It provides event handlers that the browser can invoke outside the context of your running application, for example when fetching resources from your domain, or when a push notification arrives. You can learn more about service workers in Google's [Web Fundamentals guide](https://developers.google.com/web/fundamentals/primers/service-workers).

Even though Blazor applications are built in .NET, your service worker will still be JavaScript because it runs outside the context of your application. Technically it would be possible to create a service worker that starts up the Mono WebAssembly runtime and then runs .NET code within the service worker context, but this is a lot of work that may be unnecessary considering that you might only need a few lines of JavaScript code.

To add a service worker, create a file called `service-worker.js` in your client app's `wwwroot` directory, containing:

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

Enable the service worker by adding the following `<script>` element into your `index.html` file beneath the other `<script>` elements:

```html
<script>navigator.serviceWorker.register('service-worker.js');</script>
```

If you run your app now, then in the browser's dev tools console, you should see it log the following message:

```
Installing service worker...
```

> Note that this only happens during the first page load after each time you modify `service-worker.js`. It doesn't reinstall on each load if that file's contents (compared byte-for-byte) haven't changed. 

Try it out: check that you can make some trivial change to the file (such as adding a comment or changing whitespace) and observe that the service worker reinstalls after those changes, but it does not reinstall if you do not make any changes.

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

![image](https://user-images.githubusercontent.com/1101362/66356174-0024f680-e962-11e9-9218-3f1ca657a7a7.png)

Users on Windows will also find it on their start menu, and can pin it to their taskbar if desired. Similar options exist on macOS.

## Sending push notifications

Another major PWA feature is the ability to receive and display *push notifications* from your backend server, even when the user is not on your site or in your installed app. You can use this to:

 * Notify users that something really important has happened, so they should return to your site/app
 * Update data stored in your app (such as a news feed) so it will be fresher when the user next returns, even if they are offline at that time
 * Send unsolicited advertising, or messages saying "*Hey we miss you, please visit us again!*" (Just kidding! If you do that, users will immediately block you.)

For Blazing Pizza, we have a very valid use case. Many users genuinely would want to receive push notifications that give order dispatch or delivery status updates.

### Getting a subscription

Before you can send push notifications to a user, you have to ask them for permission. If they agree, their browser will generate a "subscription", which is a set of tokens you can use to route notifications to this user.

You can ask for this permission any time you want, but for the best chance of success, ask users only when it's really clear why they would want to subscribe. You might want to have a *Send me updates* button, but for simplicity we'll ask users when they get to the checkout page, since at that point it's clear the user is serious about placing an order.

In `Checkout.razor`, add the following `OnInitialized` method:

```cs
protected override void OnInitialized()
{
    // In the background, ask if they want to be notified about order updates
    _ = RequestNotificationSubscriptionAsync();
}
```

You'll then need to define `RequestNotificationSubscriptionAsync`. Add this elsewhere in your `@code` block:

```cs
async Task RequestNotificationSubscriptionAsync()
{
    var subscription = await JSRuntime.InvokeAsync<NotificationSubscription>("blazorPushNotifications.requestSubscription");
    if (subscription != null)
    {
        try
        {
            await OrdersClient.SubscribeToNotifications(subscription);
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
        }
    }
}
```

Also add the `SubscribeToNotifications` method to `OrdersClient`.

```csharp
public async Task SubscribeToNotifications(NotificationSubscription subscription)
{
    var response = await httpClient.PutAsJsonAsync("notifications/subscribe", subscription);
    response.EnsureSuccessStatusCode();
}
```

You'll also need to inject the `IJSRuntime` service into the `Checkout` component.

```razor
@inject IJSRuntime JSRuntime
```

The `RequestNotificationSubscriptionAsync` code invokes a JavaScript function that you'll find in `BlazingPizza.ComponentsLibrary/wwwroot/pushNotifications.js`. The JavaScript code there calls the `pushManager.subscribe` API and returns the results to .NET.

If the user agrees to receive notifications, this code sends the data to your server where the tokens are stored in your database for later use.

To try this out, start placing an order and go to the checkout screen. You should see a request:

![image](https://user-images.githubusercontent.com/1101362/66354176-eed8eb80-e95b-11e9-9799-b4eba6410971.png)

Choose *Allow* and check in the browser dev console that it didn't cause any errors. If you want, set a breakpoint on the server in `NotificationsController`'s `Subscribe` action method, and run with debugging. You should be able to see the incoming data from the browser, which includes an endpoint URL as well as some cryptographic tokens.

Once you've either allowed or blocked notifications for a given site, your browser won't ask you again. If you need to reset things for further testing, and you're using either Chrome or Edge beta, you can click the "information" icon to the left of the address bar, and change *Notifications* back to *Ask (default)* as in this screenshot:

![image](https://user-images.githubusercontent.com/1101362/66354317-58f19080-e95c-11e9-8c24-dfa2d19b45f6.png)

### Sending a notification

Now you have subscriptions, you can send notifications. This involves performing some complex cryptographic operations on your server to protect the data in transit. Thankfully the bulk of the complexity is handled for us by a third-party NuGet package.

To get started, in your `BlazingPizza.Server` project, reference the NuGet package `WebPush`. The following instructions are based on using version `1.0.11`.

Next, open `OrdersController`. Have a look at the `TrackAndSendNotificationsAsync` method. This simulates a sequence of delivery steps once each order is placed, and calls `SendNotificationAsync` which isn't yet fully implemented.

We'll now update `SendNotificationAsync` to actually dispatch notifications using the subscription you captured earlier for the order's user. The following code makes uses of `WebPush` APIs for dispatching the notification:

```cs
private static async Task SendNotificationAsync(Order order, NotificationSubscription subscription, string message)
{
    // For a real application, generate your own
    var publicKey = "BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o";
    var privateKey = "OrubzSz3yWACscZXjFQrrtDwCKg-TGFuWhluQ2wLXDo";

    var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
    var vapidDetails = new VapidDetails("mailto:<someone@example.com>", publicKey, privateKey);
    var webPushClient = new WebPushClient();
    try
    {
        var payload = JsonSerializer.Serialize(new
        {
            message,
            url = $"myorders/{order.OrderId}",
        });
        await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Error sending push notification: " + ex.Message);
    }
}
```

You can generate the cryptographic keys either locally on your workstation, or online using a tool such as https://tools.reactpwa.com/vapid. If you change the demo keys in the code above, remember to update the public key in `pushNotifications.js` too. You would also have to update the `someone@example.com` address in the C# code to match your custom key pair.

If you try this now, although the server will send the notification, the browser won't display it. That's because you haven't told your service worker how to handle incoming notifications.

Try using the browser's dev tools to observe that a notification does arrive 10 seconds after you place an order. Use the dev tools *Application* tab and open the *Push Messaging* section, then click on the circle to *Start recording*:

![image](https://user-images.githubusercontent.com/1101362/66354962-690a6f80-e95e-11e9-9b2c-c254c36e49b4.png)

### Displaying notifications

You're nearly there! All that remains is updating `service-worker.js` to tell it what to do with incoming notifications. Add the following event handler function:

```js
self.addEventListener('push', event => {
    const payload = event.data.json();
    event.waitUntil(
        self.registration.showNotification('Blazing Pizza', {
            body: payload.message,
            icon: 'img/icon-512.png',
            vibrate: [100, 50, 100],
            data: { url: payload.url }
        })
    );
});
```

Remember that this doesn't take effect until after the next page load when the browser logs `Installing service worker...`. If you're struggling to get the service worker to update, you can use the dev tools *Application* tab, and under *Service Workers*, choose *Update* (or even *Unregister* so it re-registers on the next load).

With this in place, once you place an order, as soon as the order moves into *Out for delivery* status (after 10 seconds), you should receive a push notification:

![image](https://user-images.githubusercontent.com/1101362/66355395-0bc2ee00-e95f-11e9-898d-23be0a17829f.png)

If you're using either Chrome or the latest Edge browser, this will appear even if you're not still on the Blazing Pizza app, but only if your browser is running (or the next time you open the browser). If you're using the installed PWA, the notification should be delivered even if you're not running the app at all.

## Handling clicks on notifications

Currently if the user clicks the notification, nothing happens. It would be much better if it took them to the order status page for whichever order we're telling them about.

Your server-side code already sends a `url` data parameter with the notification for this purpose. To use it, add the following to your `service-worker.js`:

```js
self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data.url));
});
```

Now, once your service worker has updated, the next time you click on an incoming push notification it will take you to the relevant order status information. If you have the Blazing Pizza PWA installed, it will take you into the PWA, whereas if you don't it will take you to the page in your browser.

## Summary

This chapter showed how, even though Blazor applications are written in .NET, you still have full access to the benefits of modern browser/JavaScript capabilities. You can create a OS-installable app that looks and feels as native as you like, while having the always-updated benefits of a web app.

If you want to go further on the PWA journey, as a more advanced challenge you could consider adding offline support. It's relatively easy to get the basics working - just see [The Offline Cookbook](https://developers.google.com/web/fundamentals/instant-and-offline/offline-cookbook) for a variety of service worker samples representing different offline strategies, any of which can work with a Blazor app. However, since Blazing Pizza requires server APIs to do anything interesting like view or place orders, you would need to update your components to provide a sensible behavior when the network isn't reachable (for example, use cached data if that makes sense, or provide UI that appears if you're offline and try to do something that requires network access).

Next up - [Publish and deploy](10-publish-and-deploy.md)
