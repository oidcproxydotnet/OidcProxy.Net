---
author: Albert Starreveld
title: What is a Back-end For Front-end?
---
# Implementing the BFF Security Pattern with Azure Container Apps

In many cases, it is desirable to use a BFF (Backend For Frontend) as an Authentication Gateway when you have a microservices architecture. Microservices are often hosted on a Kubernetes-based platform, like Azure Container.

However, since Microsoft Azure Container Apps manages is in essence a managed Kubernetes Cluster, it does not provide the same level of flexibility as Kubernetes itself. Therefore, if you are using Azure Container Apps, you need to approach things slightly differently.

In this article, you will learn how to set up a microservices landscape using Azure Container Apps and the GoCloudNative.BFF.

# Architecture

Assuming that the application will scale horizontally, it is likely that the following architecture will emerge:

![GoCloudNative.Bff authentication flow](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/azure-container-apps-demo.png)

In this diagram, you see the following:

* You see the BFF (Backend for Frontend). It is used as a gateway to other services within the cluster. All traffic goes through this component. It is crucial that this component scales well. It is highly likely that multiple instances of this component are running. The ingress of this component is enabled for external traffic coming from outside the cluster. The URL associated with this container app is the URL that end-users will enter in their browser to access the web app.
* You also see a Redis Cache. It is used to store the BFF’s state (user sessions). Without this component, the BFF cannot scale horizontally. This component must only be accessible to the BFF.
* You also see a SPA (Single Page App). That’s an NginX container which serves the "dist" folder of the Single Page App. In this diagram, all requests from the SPA "pass through" the BFF. This is inefficient and unnecessary. However, for the sake of simplicity in this example, we have chosen to do it this way. The only file that needs to pass through the BFF is index.html.
* And last, but not least, there is an API as well. This API may only be accessible to authenticated users, and the BFF ensures that. The ingress for this API is only enabled for traffic within the cluster. After all, we want to prevent direct access to the API by anyone outside the cluster.

# Deploying the dependencies

In order to run this application, you will require the following components:

* A resource group
* An Azure Container Environment
* Three Container Apps
* An Azure Container Registry
* An Azure Cache for Redis
* For this example, we are utilizing Auth0 as the Identity Provider.

This section focuses on the dependencies of the Azure Container Apps. The *subsequent section will delve into the Azure Container App Environment itself.

We assume that you possess experience in deploying this infrastructure within Azure. The script for deploying these resources can be located in the provided file. This article includes excerpts from the script. In the following paragraphs, we will elucidate the key aspects of this infrastructure and the reasoning behind its configuration.

To initiate the creation of the resource group, container registry, and KeyVault, please execute the following commands:

```powershell
$yourAppName = "yourappname"

$rgName = "rg-$yourAppName"
$acrName = "acr$yourAppName"
$cacheName = "redis-$yourAppName";

$location = "westeurope"

az login

# Create the resource group
az group create --location $location --name $rgName

# Create the container app
az acr create --resource-group $rgName --location $location --name $acrName --sku Basic --admin-enabled true

# Create the cache. This may take up to 15 minutes...
az redis create --location $location --name $cacheName --resource-group $rgName --sku Basic --vm-size c0
```
## Building the Docker images

To witness the complete application in action within an Azure Container App, you need to build the containers, push them to the deployed container registry, and deploy the container apps to run these containers.

These containers require access to both the Azure Container Registry and the Azure KeyVault.

Execute the following commands to build and push the Docker images:

```powershell 
# Clone the repostiry
git clone https://github.com/thecloudnativewebapp/GoCloudNative.Bff.git

# Navigate to the correct folder
cd GoCloudNative.Bff/docs/demos/Auth0/src

# Build the containers
docker build -f Bff/Dockerfile -t gocloudnative-demo-bff:1.0 .
docker build -f Spa/Dockerfile  -t gocloudnative-demo-spa:1.0 .
docker build -f Api/Dockerfile -t gocloudnative-demo-api:1.0 .

# Log in to the Azure Portal to find the login server value of the 
# Azure Container Registry you've created in the previous step
$acrLoginServer = "{loginServer}" 

# Tag the images
docker tag gocloudnative-demo-bff:1.0 "$acrLoginServer/bffdemo-bff:1.0"
docker tag gocloudnative-demo-spa:1.0 "$acrLoginServer/bffdemo-spa:1.0"
docker tag gocloudnative-demo-api:1.0 "$acrLoginServer/bffdemo-api:1.0"

# Push the images
az acr login --name $acrLoginServer
docker push "$acrLoginServer/bffdemo-bff:1.0"
docker push "$acrLoginServer/bffdemo-spa:1.0"
docker push "$acrLoginServer/bffdemo-api:1.0"
```

## Deploying the Azure Container Apps Environment

To deploy the images we have recently uploaded to the container registry in a Container App, an Azure Container Apps Environment is required. This environment relies on a log analytics workspace. To deploy both the environment and the workspace, run the following script:

In this guide, we are utilizing the Auth0 sample [available in our GitHub repository](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/Auth0/src). This sample includes a functional BFF (built with GoCloudNative.Bff), an API, and an Angular test app. Each component is accompanied by its respective Dockerfile for container building. Additionally, the demo includes a script for container building.


```powershell
$yourAppName = "yourappname"

$rgName = "rg-$yourAppName"
$logWorkspaceName = "la-$yourAppName"
$envName = "env-$yourAppName"

# Create the workspace and obtain the key required by the container
# app environment to access it
$monitorJson = az monitor log-analytics workspace create 
    -g $rgName 
    -n $logWorkspaceName

$sharedKeysJson = az monitor log-analytics workspace get-shared-keys 
    --resource-group $rgName 
    --workspace-name $logWorkspaceName

$monitor = $monitorJson | ConvertFrom-Json
$sharedKeys = $sharedKeysJson | ConvertFrom-Json

# Create the container app environment
az containerapp env create 
    -n $envName 
    -g $rgName 
    --location $location 
    --logs-workspace-id $monitor.customerId 
    --logs-workspace-key $sharedKeys.primarySharedKey

```

## Configuring Auth0 and deploying the API and the Single Page Application

In order for the BFF application we are about to deploy to authenticate using Auth0, you need to configure Auth0 following the instructions provided in the [Auth0 quickstart](/integration-manuals/quickstarts/auth0/quickstart/).

Within the quickstart, you will find the necessary details such as the Auth0 Domain, Audience, ClientId, and Secret. These values are required for the successful configuration of the BFF and the API.

After you have configured Auth0, first, you must deploy the Single-Page Application and the API. To create the Azure Container Apps which will run these applications, please execute the provided script:

```powershell
$yourAppName = "yourappname"
$acrLoginServer = "???"

$auth0Domain = "???" # Values from the Auth0 quickstart
$auth0Audience = "???"

$envName = "env-$yourAppName"
$spaAppName = "app-$yourAppName-spa"
$apiAppName = "app-$yourAppName-api"

$spaApp = az containerapp create -n $spaAppName `
    -g $rgName `
    --image "$acrLoginServer/bffdemo-spa:1.0" `
    --environment $envName `
    --cpu 0.5 `
    --memory 1.0Gi `
    --min-replicas 2 `
    --max-replicas 2 `
    --ingress internal --target-port 80 `
    --registry-server $acrLoginServer | ConvertFrom-Json

$apiApp = az containerapp create -n $apiAppName `
    -g $rgName `
    --image "$acrLoginServer/bffdemo-api:1.0" `
    --environment $envName `
    --cpu 0.5 `
    --memory 1.0Gi `
    --min-replicas 2 `
    --max-replicas 2 `
    --ingress internal --target-port 80 `
    --registry-server $acrLoginServer `
    --secrets auth0domain=$auth0Domain auth0audience=$auth0Audience `
    --env-vars Auth0__Domain=secretref:auth0domain Auth0__Audience=secretref:auth0audience | ConvertFrom-Json
```

Please note that both the Single-Page Application and the API have the ingress enabled and set to internal. This configuration is essential to ensure that these resources can only be accessed through the BFF.

Furthermore, it is important to mention that, for the purpose of this demo, the secrets are not stored in a KeyVault. However, we highly recommend utilizing a KeyVault to securely store such sensitive values when you deploy your app to production.

## Deploying the BFF

A significant advantage of a container platform is its ability to scale horizontally. In this demonstration, each Container App has been set up to maintain a minimum of two instances consistently.

However, as indicated in the diagram provided earlier, this poses a challenge for the BFF. Since traffic is distributed randomly across nodes, there is a possibility that users may encounter errors due to the lack of shared session state. To address this issue, we need to configure the connection string to Redis. By doing so, we can configure the BFF Container App to store its session state in Redis.

To create Azure Cache for Redis, and to create the connection string, execute the following commands:

```powershell
$yourAppName = "yourappname"

$cacheName = "redis-$yourAppName";

$location = "westeurope"

$redis = az redis create --location $location --name $cacheName --resource-group $rgName --sku Basic --vm-size c0 | ConvertFrom-Json

$redisAccessKey = $redis.accessKeys.primaryKey
$redisPort = $redis.sslPort
$redisHost = $redis.hostName

$redisConnectionString = "$redisHost`:$redisPort,password=$redisAccessKey,ssl=True,abortConnect=False"
```

Once you have successfully created the Redis connection string, deployed the Single Page Application (SPA) and the API, you can proceed with deploying the BFF (Backend For Frontend). Before deploying the BFF, you must configure additional Auth0-related variables. Ensure that these values match the ones you set up during the initial setup using the provided quickstart guide.

As shown in the diagram at the beginning of this article, the BFF acts as a proxy for both the APIs and the Single Page Application. Therefore, the BFF needs to know the location of these services. To achieve this, you will need the output from the previously executed commands, which are stored in the `$spaApp` and `$apiApp` variables. These variables contain the URLs to which the BFF will forward requests.

You can use the following commands to accomplish this:

```powershell
$yourAppName = "yourappname"

$rgName = "rg-$yourAppName"
$acrLoginServer = "???" # look this up in the Azure portal
$envName = "env-$yourAppName"

$bffAppName = "app-$yourAppName-bff"

$location = "westeurope"

$apiUrlFqdn = $apiApp.properties.configuration.ingress.fqdn
$apiUrl = "https://$apiUrlFqdn"

$spaUrlFqdn = $spaApp.properties.configuration.ingress.fqdn
$spaUrl = "https://$spaUrlFqdn"

az containerapp create -n $bffAppName 
    -g $rgName 
    --image "$acrLoginServer/bffdemo-bff:1.0" 
    --environment $envName 
    --cpu 0.5 
    --memory 1.0Gi 
    --min-replicas 2 
    --max-replicas 2 
    --ingress external --target-port 80 
    --registry-server $acrLoginServer 
    --secrets auth0domain=$auth0Domain auth0audience=$auth0Audience auth0clientid=$auth0ClientId auth0clientsecret=$auth0ClientSecret redisconnectionstring=$redisConnectionString `
    --env-vars Auth0__Domain=secretref:auth0domain Auth0__Audience=secretref:auth0audience Auth0__ClientId=secretref:auth0ClientId Auth0__ClientSecret=secretref:auth0clientsecret REVERSEPROXY__CLUSTERS__SPA__DESTINATIONS__SPA__ADDRESS=$spaUrl REVERSEPROXY__CLUSTERS__API__DESTINATIONS__API__ADDRESS=$apiUrl ConnectionStrings__Redis=secretref:redisConnectionString
```

After creating the BFF Container App, Azure Container Apps automatically generates a hostname for you. This hostname is crucial for configuring Auth0, specifically the redirect URL. Failure to configure the redirect URL will prevent successful login. In my case, the redirect URI is:

`https://app-yourappname-bff.bluepond-ea035e9a.westeurope.azurecontainerapps.io/account/login/callback`

It's important to note that unlike the other Container Apps, the BFF has its `ingress` configured to be externally accessible. This serves as the entry point for the application. If you open your browser and navigate to the URL of the BFF (e.g., `https://app-yourappname-bff.bluepond-ea035e9a.westeurope.azurecontainerapps.io/`), you should now see a functional website that looks like this:

![Demo app](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/demo-app.png)

## Summary
To deploy an application utilizing the BFF Security Pattern on Azure Container Apps, follow the steps outlined below:

* Configure Auth0 by referring to the [provided quickstart guide](/integration-manuals/quickstarts/auth0/quickstart/).
* Deploy an Azure Container Registry.
* Upload the container images to the Azure Container Registry.
* Deploy an Azure Cache for Redis instance.
* Deploy the Azure Container Apps Environment.
* Begin by deploying the Single Page Application Container App, followed by the API Azure Container App.
* Finally, deploy the BFF Azure Container App and configure it as follows:
    * Utilize the Azure Cache for Redis instance to store the session state.
    *  Configure the necessary Auth0 environment variables.
    *  Configure the URLs to the downstream services.

For detailed instructions on executing these steps, refer to [this script](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/blob/main/docs/demos/Auth0/src/deploy-to-azure.ps1).

## Feedback

Help us build better software. Your feedback is valuable to us. We would like to inquire about your success in setting up this demo.

- Were you able to successfully deploy the application with Azure Container Apps? Please share your thoughts on the overall experience by answering the questions in our [feedback form](/feedback/).
- Did you face any difficulties or encounter missing features? Kindly inform us at: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues
