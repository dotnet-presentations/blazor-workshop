# Publish and deploy

In this optional session, you'll deploy the pizza store app to Azure App Service.

## Creating an Azure account

You'll need an Azure account or subscription to complete this session. If you don't have one already, you can [sign up for a free Azure account](https://azure.microsoft.com/Free).

After you've created your account, make sure to sign in to Visual Studio with this account so that you can access your Azure resources.

## Publishing to a new Azure App Service

Azure App Service allows you to easily deploy ASP.NET Core web apps to the cloud.

Right-click on the Server project in the solution and select Publish. The ASP.NET Core Server project references the client Blazor project, so publishing the Server project will include the Blazor parts and their dependencies.

![Publish from VS](https://user-images.githubusercontent.com/1874516/51885818-2501ac80-2385-11e9-8025-4d1477083a8d.png)

In the Publish wizard, select "Azure" and then select Next:

![Pick a publish target](https://user-images.githubusercontent.com/1874516/78459197-31118a00-766c-11ea-9d41-470ea772e34f.png)

Select "Azure App Service (Windows)" for the specific target, and then select Next:

![Publish to App Service](https://user-images.githubusercontent.com/1874516/78459246-8baae600-766c-11ea-9600-b01e168bf71a.png)

Wait for your subscriptions to load, and then select the subscription to use for the Azure App Service. After selecting your subscription, select "Create a new Azure App Service..." at the bottom of the dialog.

![Select existing or create new](https://user-images.githubusercontent.com/1874516/78459794-7041da00-7670-11ea-96ab-d103b8f21739.png)

In the "App Service: Create New" dialog:

- Make sure that the correct account that you want to use for your new Azure App Service is selected in the account drop down in the upper right
- Pick a unique name for your app (which becomes part of the app's default URL)
- Select the Azure subscription you want to use along with the Resource Group and Hosting Plan
    - Resource groups are a convenient way to group related resources on Azure, so consider creating one specific to the pizza store app. 
    - For the hosting plan, you'll need to select a Basic tier hosting plan or higher.

![Create new App Service](https://user-images.githubusercontent.com/1874516/78463095-e0ab2400-768d-11ea-8ec3-f8885368118d.png)

Click Create to create the app service. This may take a couple of minutes. 

Once the app service has been created, make sure it is selected and then click Finish in the Publish dialog

![Finish Publish](https://user-images.githubusercontent.com/1874516/78459868-0d047780-7671-11ea-87d5-0a72ca9e5d36.png)

Once the App Service is created you should see your publish profile in the Publish page:

![Publish profile](https://user-images.githubusercontent.com/1874516/78460244-0e836f00-7674-11ea-975a-f582d6af9942.png)

At this point you could create a production database for your app. Since the app uses SQLite and deploys its own database, creating a database isn't necessary, but for a real app it would be.

If we publish the app at this point, it will return a server error and fail to start. This is because we first need to configure a signing key for IdentityServer. During development, we used a development key (see *BlazingPizza.Server/appsettings.Development.json*), but in production we need to configure an actual certificate for issuing tokens. We'll do that using Azure Key Vault.

## Setup a signing certificate with Azure Key Vault

You can create a signing certificate using an existing key vault, or create a new one.

To create a new key vault:

- Sign in to the Azure portal at https://portal.azure.com.
- In the Search box, enter **Key Vault**.
- From the results list, choose **Key vaults**.
- On the Key Vault section, choose **Add**.
- On the **Create key vault** section, provide the following information:
    - **Subscription**: Choose your subscription.
    - **Resource group**: Choose the resource group for your key vault.
    - **Key vault name**: A unique name is required.
    - In the **Region** pull-down menu, choose a location.
    - Leave the other options to their defaults.
- After providing the information above, select **Review + create** to create your key vault.

Browse to your key vault in the Azure portal and select **Certificates**. Select **Generate/Import** to create a new certificate.

![Generate key vault certificate](https://user-images.githubusercontent.com/1874516/78463378-ba3ab800-7690-11ea-9744-6850c2d1a7e6.png)

Generate a self-signed certificate with a name of your choice and a matching subject name (prefixed with "CN=") and the select **Create**.

![Create certificate](https://user-images.githubusercontent.com/1874516/78463413-17cf0480-7691-11ea-91dc-343cdea5aa79.png)

Browse to your app service in the portal, select **TLS/SSL Settings**. Select the **Private Key Certificates (.pfx)** tab and then select **Import Key Vault Certificate**.

![Import key vault certificate](https://user-images.githubusercontent.com/1874516/78463445-890eb780-7691-11ea-949a-d7dd38b43550.png)

Select the certificate you previously created to import it into the app service.

![Select certificate](https://user-images.githubusercontent.com/1874516/78463454-ae9bc100-7691-11ea-9ca4-64d27582f699.png)

Select the imported certificate and copy its thumbprint.

![Copy certificate thumbprint](https://user-images.githubusercontent.com/1874516/78463487-1520df00-7692-11ea-93ae-697406bfdd86.png)

Select **Configuration** in the left nav for the app service. Add the `WEBSITE_LOAD_CERTIFICATES` application setting with its value set to the certificate thumbprint you copied previously. This setting will make the certificate available to your app using the Windows certificate store.

![Load certificates setting](https://user-images.githubusercontent.com/1874516/78463547-e8b99280-7692-11ea-9d02-394b20c653cd.png)

Now update **appsettings.json** in the server project configure the app to use the certificate in production.

```json
"IdentityServer": {
  "Key": {
    "Type": "Store",
    "StoreName": "My",
    "StoreLocation": "CurrentUser",
    "Name": "CN=BlazingPizzaCertificate"
  },
  "Clients": {
    "BlazingPizza.Client": {
      "Profile": "IdentityServerSPA"
    }
  }
}
```

You're ready to publish! Click Publish.

Publishing the app may take a few minutes. Once the app has finished deploying it should automatically load in the browser.

![Published app](https://user-images.githubusercontent.com/1874516/78463636-09ceb300-7694-11ea-9d3c-57b52b982186.png)

Congrats!

Once you're done showing off your completed Blazor app to your friends, be sure to clean up any Azure resources that you no longer wish to maintain.
