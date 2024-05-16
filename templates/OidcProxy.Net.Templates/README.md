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
dotnet new OidcProxy.Net --backend "https://api.myapp.com" --idp "https://idp.myapp.com" --clientId xyz --clientSecret abc

dotnet run
```

# Scaffolding an Angular App with a BFF

Scaffold a new project by executing the following commands:

```bash
dotnet new OidcProxy.Net.Angular --backend "https://api.myapp.com" --idp "https://idp.myapp.com" --clientId xyz --clientSecret abc

dotnet run
```

Open a browser and navigate to https://localhost:8444

Read more about OidcProxy.Net here:
- https://github.com/oidcproxydotnet/oidcproxy.net