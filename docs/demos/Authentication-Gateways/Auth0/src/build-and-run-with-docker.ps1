
Write-Host "=========="
Write-Host "Building OidcProxy.Net Sample - Auth0"
Write-Host "=========="
Write-Host

$steps = 4

Write-Host "[Step 1/$steps] - Building the API-container"
Write-Host
docker build -f Api/Dockerfile -t oidcproxy-demo-api:1.0 .

Write-Host
Write-Host "[Step 2/$steps] - Building the SPA-container"
Write-Host
docker build -f Spa/Dockerfile  -t oidcproxy-demo-spa:1.0 .

Write-Host
Write-Host "[Step 3/$steps] - Build the BFF"
Write-Host
docker build -f Bff/Dockerfile -t oidcproxy-demo-bff:1.0 .

Write-Host
Write-Host "[Step 4/$steps] - Running the containers"
Write-Host
docker compose up -d

Write-Host "=========="
Write-Host "Done - Browse to http://localhost:8443 to check out the demo-app"
Write-Host "Please make sure everything runs as expected."
Write-Warning "If it doesn't, make sure you have configured the environment variables "
Write-Warning "correctly in the docker-compose.yml."
Write-Host "=========="