$yourAppName = "yourappname"

$rgName = "rg-$yourAppName"
$acrName = "acr$yourAppName"
$logWorkspaceName = "ws-$yourAppName"
$envName = "env-$yourAppName"
$cacheName = "redis-$yourAppName";

$bffAppName = "app-$yourAppName-bff"
$spaAppName = "app-$yourAppName-spa"
$apiAppName = "app-$yourAppName-api"

$location = "westeurope"

# Collect parameters from user
Write-Host "What's the Auth0 domain?"
$auth0Domain = Read-Host

Write-Host "What's the Auth0 Audience?"
$auth0Audience = Read-Host

Write-Host "What's the Auth0 ClientId?"
$auth0ClientId = Read-Host

Write-Host "What's the Auth0 ClientSecret?"
$auth0ClientSecret = Read-Host

# Create the resources

Write-Host "======= Creating resource group ======"
az group create --location $location --name $rgName

Write-Host "======= Creating Azure Container Registry ======"
$createAcrResultJson = az acr create --resource-group $rgName --location $location --name $acrName --sku Basic --admin-enabled true
$createAcrResult = $createAcrResultJson | ConvertFrom-Json
Write-Host $createAcrResultJson

Write-Host "======= Creating Azure Container Registry :: Initializing the repositories ======"
$acrLoginServer = $createAcrResult.loginServer; 

az acr login --name $createAcrResult.loginServer

docker build -f Bff/Dockerfile -t gocloudnative-demo-bff:1.0 .
docker tag gocloudnative-demo-bff:1.0 "$acrLoginServer/bffdemo-bff:1.0"
docker push "$acrLoginServer/bffdemo-bff:1.0"

docker build -f Spa/Dockerfile  -t gocloudnative-demo-spa:1.0 .
docker tag gocloudnative-demo-spa:1.0 "$acrLoginServer/bffdemo-spa:1.0"
docker push "$acrLoginServer/bffdemo-spa:1.0"

docker build -f Api/Dockerfile -t gocloudnative-demo-api:1.0 .
docker tag gocloudnative-demo-api:1.0 "$acrLoginServer/bffdemo-api:1.0"
docker push "$acrLoginServer/bffdemo-api:1.0"

Write-Host "======= Creating Redis Cache ======"
$redisJson = az redis create --location $location --name $cacheName --resource-group $rgName --sku Basic --vm-size c0
Write-Host $redisJson

$redis = $redisJson | ConvertFrom-Json
$redisAccessKey = $redis.accessKeys.primaryKey
$redisPort = $redis.sslPort
$redisHost = $redis.hostName

Write-Host "======= Creating Container Apps Environment :: Creating logging workspace ======"
$monitorJson = az monitor log-analytics workspace create -g $rgName -n $logWorkspaceName
Write-Host $monitorJson

$sharedKeysJson = az monitor log-analytics workspace get-shared-keys --resource-group $rgName --workspace-name $logWorkspaceName
Write-Host $sharedKeysJson

$monitor = $monitorJson | ConvertFrom-Json
$sharedKeys = $sharedKeysJson | ConvertFrom-Json

Write-Host "======= Creating Container Apps Environment ======"
az containerapp env create -n $envName -g $rgName --location $location --logs-workspace-id $monitor.customerId --logs-workspace-key $sharedKeys.primarySharedKey

$spaAppJson = az containerapp create -n $spaAppName `
    -g $rgName `
    --image "$acrLoginServer/bffdemo-spa:1.0" `
    --environment $envName `
    --cpu 0.5 `
    --memory 1.0Gi `
    --min-replicas 2 `
    --max-replicas 2 `
    --ingress internal --target-port 80 `
    --registry-server $acrLoginServer
Write-Host $spaAppJson
$spaApp = $spaAppJson | ConvertFrom-Json

$apiAppJson = az containerapp create -n $apiAppName `
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
    --env-vars Auth0__Domain=secretref:auth0domain Auth0__Audience=secretref:auth0audience
Write-Host $apiAppJson
$apiApp = $apiAppJson | ConvertFrom-Json

# Determine the URLs to the API and the SPA
$apiUrlFqdn = $apiApp.properties.configuration.ingress.fqdn
$apiUrl = "https://$apiUrlFqdn"

$spaUrlFqdn = $spaApp.properties.configuration.ingress.fqdn
$spaUrl = "https://$spaUrlFqdn"

# Create the Redis connection string
$redisConnectionString = "$redisHost`:$redisPort,password=$redisAccessKey,ssl=True,abortConnect=False"

# Deploy the BFF
az containerapp create -n $bffAppName `
    -g $rgName `
    --image "$acrLoginServer/bffdemo-bff:1.0" `
    --environment $envName `
    --cpu 0.5 `
    --memory 1.0Gi `
    --min-replicas 2 `
    --max-replicas 2 `
    --ingress external --target-port 80 `
    --registry-server $acrLoginServer `
    --secrets auth0domain=$auth0Domain auth0audience=$auth0Audience auth0clientid=$auth0ClientId auth0clientsecret=$auth0ClientSecret redisconnectionstring=$redisConnectionString `
    --env-vars Auth0__Domain=secretref:auth0domain Auth0__Audience=secretref:auth0audience Auth0__ClientId=secretref:auth0ClientId Auth0__ClientSecret=secretref:auth0clientsecret REVERSEPROXY__CLUSTERS__SPA__DESTINATIONS__SPA__ADDRESS=$spaUrl REVERSEPROXY__CLUSTERS__API__DESTINATIONS__API__ADDRESS=$apiUrl ConnectionStrings__Redis=secretref:redisConnectionString

Write-Host $redisConnectionString