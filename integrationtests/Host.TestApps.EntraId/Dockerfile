FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Host/Host.csproj", "Host/"]
COPY ["OpenIdConnect/OidcProxy.Net.OpenIdConnect/OidcProxy.Net.OpenIdConnect.csproj", "OidcProxy.Net.OpenIdConnect/"]
COPY ["OidcProxy.Net/OidcProxy.Net.csproj", "OidcProxy.Net/"]
COPY ["Auth0/OidcProxy.Net.Auth0/OidcProxy.Net.Auth0.csproj", "OidcProxy.Net.Auth0/"]
COPY ["EntraId/OidcProxy.Net.EntraId/OidcProxy.Net.EntraId.csproj", "OidcProxy.Net.EntraId/"]
RUN dotnet restore "Host/Host.csproj"
COPY . .
WORKDIR "/src/Host"
RUN dotnet build "Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Host.dll"]
