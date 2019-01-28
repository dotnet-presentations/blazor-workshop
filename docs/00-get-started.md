# Get started

In this session, you'll setup your machine for Blazor development and build your first Blazor app.

## Setup

Install the following:

1. [.NET Core 2.1 SDK](https://go.microsoft.com/fwlink/?linkid=873092) (2.1.500 or later).
1. [Visual Studio 2017](https://go.microsoft.com/fwlink/?linkid=873093) (15.9 or later) with the *ASP.NET and web development* workload selected.
1. The latest [Blazor Language Services extension](https://go.microsoft.com/fwlink/?linkid=870389) from the Visual Studio Marketplace.
1. The Blazor templates on the command-line:

   ```console
   dotnet new -i Microsoft.AspNetCore.Blazor.Templates
   ```

## Build and run your first Blazor app

To create a Blazor project in Visual Studio:

1. Select **File** > **New** > **Project**. Select **Web** > **ASP.NET Core Web Application**. Name the project "BlazorApp1" in the **Name** field. Select **OK**.

    ![New ASP.NET Core project](https://raw.githubusercontent.com/aspnet/Blazor.Docs/gh-pages/docs/tutorials/build-your-first-blazor-app/_static/new-aspnet-core-project.png)

1. The **New ASP.NET Core Web Application** dialog appears. Make sure **.NET Core** is selected at the top. Also select **ASP.NET Core 2.1**. Choose the **Blazor** template and select **OK**.

    ![New Blazor app dialog](https://raw.githubusercontent.com/aspnet/Blazor.Docs/gh-pages/docs/tutorials/build-your-first-blazor-app/_static/new-blazor-app-dialog.png)

1. Once the project is created, press **Ctrl-F5** to run the app *without the debugger*. Running with the debugger (**F5**) isn't supported at this time.

> [!NOTE]
> If not using Visual Studio, create the Blazor app at a command prompt on Windows, macOS, or Linux:
>
> ```console
> dotnet new blazor -o BlazorApp1
> cd BlazorApp1
> dotnet run
> ```
>
> Navigate to the app using the localhost address and port provided in the console window output after `dotnet run` is executed. Use **Ctrl-C** in the console window to shutdown the app.

The Blazor app runs in the browser:

![Blazor app Home page](https://user-images.githubusercontent.com/1874516/39509497-5515c3ea-4d9b-11e8-887f-019ea4fdb3ee.png)

Congrats! You just built and ran your first Blazor app!

If you have more time, try out the rest of the [introductory Blazor tutorial](https://blazor.net/docs/tutorials/build-your-first-blazor-app.html#build-components)

Next up - [Components and layout](01-components-and-layout.md)
