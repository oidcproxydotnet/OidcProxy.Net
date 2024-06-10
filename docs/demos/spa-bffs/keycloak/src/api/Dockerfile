FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=base /App/out .
ENTRYPOINT ["dotnet", "Api.dll"]
