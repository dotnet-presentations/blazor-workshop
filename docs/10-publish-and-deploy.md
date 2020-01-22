# Publish and deploy

In this optional session, you'll deploy the pizza store app to Azure App Service.

## Creating an Azure account

You'll need an Azure account or subscription to complete this session. If you don't have one already, you can [sign up for a free Azure account](https://azure.microsoft.com/Free).

After you've created your account, make sure to sign in to Visual Studio with this account so that you can access your Azure resources.

## Publishing to a new Azure App Service

Azure App Service allows you to easily deploy ASP.NET Core web apps to the cloud.

Right-click on the Server project in the solution and select Publish. The ASP.NET Core Server project references the client Blazor project, so publishing the Server project will include the Blazor parts and their dependencies.

![Publish from VS](https://user-images.githubusercontent.com/1874516/51885818-2501ac80-2385-11e9-8025-4d1477083a8d.png)

In the "Pick a publish target" dialog:
- Select "App Service"
- Select the "Create New" option
- Select "Create Profile" in the button drop down before clicking it

![Pick a publish target](https://user-images.githubusercontent.com/1874516/51885912-7f027200-2385-11e9-8707-0e2f82b543fd.png)

In the "Create App Service" dialog:
- Make sure that the correct account that you want to use for your new Azure App Service is selected in the account drop down in the upper right
- Pick a unique name for your app (which becomes part of the app's default URL)
- Select the Azure subscription you want to use along with the Resource Group and Hosting Plan
    - Resource groups are a convenient way to group related resources on Azure, so consider creating one specific to the pizza store app. 
    - For the hosting plan, using a free plan is fine.

![Create App Service](https://user-images.githubusercontent.com/1874516/51886115-4e6f0800-2386-11e9-9da1-82cc910aad3b.png)

At this point you could also create a production database for your app. Since the app uses SQLite and deploys its own database, creating a database isn't necessary, but for a real app it would be.

Click Create to create the App Service. This may take a couple of minutes. Once the App Service is created you should see your publish profile in the Publish page:

![Publish profile](https://user-images.githubusercontent.com/1874516/51886256-ee2c9600-2386-11e9-9da7-d80d2500b0ea.png)

Before we publish, we first we need to do some configuration for our app to run. The pizza store app requires a Twitter app consumer key and secret to handle authentication. During development, these values are stored in `appsettings.Development.json`. We need to configure these values in the App Service environment, or the app will fail to run.

To register your app service as a Twitter app, you'll need to use the [Twitter Developer Console](https://developer.twitter.com/apps) and signup up for a Twitter developer account. Or you can use dummy values for now, which will break authentication, but at least allow the app to run.

Click on the "Edit App Service Settings" link. Add two settings: `Authentication:Twitter:ConsumerKey`, and `Authentication:Twitter:ConsumerSecret`. Specify the correct values for your Twitter app, or just put in some dummy strings if you don't care about getting  authentication working. Click OK to save the app settings.

![Add app settings](https://user-images.githubusercontent.com/1874516/51886491-fc2ee680-2387-11e9-9c16-5f1fc47365fa.png)

You're ready to publish! Click Publish.

![image](https://user-images.githubusercontent.com/1874516/51886932-a52a1100-2389-11e9-8b58-6ea3ae5a4291.png)


Publishing the app may take a few minutes. Once the app has finished deploying it should automatically load in the browser.

![Published app](https://user-images.githubusercontent.com/1874516/51886593-5a5bc980-2388-11e9-9329-7e015901e45d.png)

Congrats!
