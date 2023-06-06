# Auth0 demo

To run this demo, configure Auth0 as described in [this article](https://bff.gocloudnative.org/integration-manuals/quickstarts/auth0/quickstart/).

Now, clone or download this repository. Navigate to this folder on your local machine and open the `docker-compose.yml`. It has a section called `bff` and a section called `api`. Configure the following environment variables with the values you have configured in Auth0:

```yaml
version: '3.4'
services:
  bff:
...
    environment:
      - Auth0__ClientSecret={yourSecret}
      - Auth0__ClientId={yourClientId}
      - Auth0__Domain={yourDomain}
      - Auth0__Audience={yourAudience}
...

  api:
...
    environment:
      - Auth0__Domain={yourDomain}
      - Auth0__Audience={yourAudience}
```

Now, open powershell or terminal if you have a Mac and navigate to the folder where the `docker-compose.yml` is located and type the following command:

Powershell on Windows:
```powershell
.\build-and-run-with-docker.ps1
```

Terminal on Mac:
```powershell
brew install --cask powershell
pwsh build-and-run-with-docker.ps1
```

This scripts builds the API, the BFF, and the SPA and then executes `docker compose up -d`. After the script has completed, open a browser and navigate to `http://localhost:8443`.