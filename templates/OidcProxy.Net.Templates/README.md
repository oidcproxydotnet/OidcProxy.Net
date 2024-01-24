<!-- Provide an overview of what your template package does and how to get started.
Consider previewing the README before uploading (https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#preview-your-readme). -->
# The OidcProxy.Net Template Pack

This library contain boilerplate projects for Backend For Frontend projects or Identity-Aware proxies with ASPNET Core. 

This package now contains the following templates:

* OidcProxy.Net.Web (A vanilla OidcProxy)
* OidcProxy.Net.Angular (Angular App + ASP.NET Core BFF)

# Installing the template pack

To install the templates, execute the following command:

```bash
dotnet new install OidcProxy.Net.Templates
```

# Scaffolding a vanilla OidcProxy

Scaffold a new project by executing the following commands:

```bash
mkdir MyProxy
cd MyProxy

dotnet new OidcProxy.Net.Web
```

Open the appsettings.json to configure the `client_id`, `client_secret`, and the `authority` and run your new BFF:

```bash
dotnet run
```

# Scaffolding an Angular App with a BFF

Scaffold a new project by executing the following commands:

```bash
mkdir MyBff
cd MyBff

dotnet new OidcProxy.Net.Angular
```

Open the appsettings.json to configure the `client_id`, `client_secret`, and the `authority` and run your new BFF:

```bash
dotnet run
```

Open a browser and navigate to https://localhost:8444

Read more about OidcProxy.Net here:
- https://github.com/thecloudnativewebapp/oidcproxy.net
- https://bff.gocloudnative.org/