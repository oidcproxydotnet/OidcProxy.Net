# How to build a BFF with C#

To build a Back-end for Front-end with C#, you can choose three approaches:

## Build an ASPNETCORE Reverse Proxy with YARP. 
Use YARP to relay traffic to downstream API's. You can use Minimal-APIs to implement custom endpoints to invoke requests to multiple downstream APIs and aggregate the results.

To create a YARP based BFF-project, do the following:

```powershell
dotnet new web
Install-Package Yarp.ReverseProxy
```

Make sure to have the following `program.cs` file:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable endpoint routing, required for the reverse proxy
app.UseRouting();

// Register the reverse proxy routes
app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy();
});

app.MapGet("/custom-endpoint", () => {
    // Invoke downstream endpoints here, and aggregate the results..
});

app.Run();
```

Use the following `appsettings.json` to relay traffic to a downstream API:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "route1": {
          "ClusterId": "weatherForecastApi",
          "AuthorizationPolicy": "RequireAuthenticatedUserPolicy",
          "Match": {
           "Path": "/weatherforecast/{**catch-all}"
          },
          "AllowAnonymous": false
      }
    },
    "Clusters": {
      "weatherForecastApi": {
        "Destinations": {
          "weatherForecastApi/destination1": {
            "Address": "http://localhost:7352"
          }
        }
      }
    }
  }
}
```

## Build a GraphQL Back-end For Front-end
Another approach is to build a BFF with GraphQL, GraphQL allows end-users to specify which downstream resources they wish to query. You can use [Chilli Cream](https://chillicream.com/docs/bananacakepop/v2) to implement GraphQL in your aspnetcore project.

## Implement all endpoints manually
Another way to build a BFF with C#, is to simply create a new web-project, and use controllers to implement endpoints you need. The code in these controllers will invoke downstream APIs and aggregate the results.
