function SetVersionNumber($fileName, $versionNumber) {
    if ($versionNumber -eq $null -or $versionNumber -eq "") {
        throw 'Aborted. Package has not been published to nuget. Could not set the new version number. $versionNumber must have a value.'
    }

    $regexVersionTag = "\<Version\>[0-9\.]{1,}\<\/Version\>"
    $newVersionTag = "<Version>$versionNumber</Version>"

    $regexPackageVersionTag = "\<PackageVersion\>[0-9\.]{1,}\<\/PackageVersion\>"
    $newPackageVersionTag = "<PackageVersion>$versionNumber</PackageVersion>"

    $content = Get-Content -path $fileName

    $content = $content -Replace $regexVersionTag, $newVersionTag
    
    $content -Replace $regexPackageVersionTag, $newVersionTag | Out-File $fileName
}

# ==== test ====

dotnet test

if ($lastexitcode -eq 1) {
    write-host "Aborted. Package has not been published to nuget. Cannot deploy when tests fail. Fix the tests first, then run the script again."
    exit
}

write-host "Test succeeded."
write-host "================================="
write-host ""

# ==== set new version number ====

if ($newVersion -eq $null) {
    
    write-host "Target version number? (Type and hit enter.)" -ForegroundColor Green
    $newVersion = read-host
}

SetVersionNumber "src/GoCloudNative.Bff.Authentication/GoCloudNative.Bff.Authentication.csproj" $newVersion
SetVersionNumber "src/GoCloudNative.Bff.Authentication.Auth0/GoCloudNative.Bff.Authentication.Auth0.csproj" $newVersion
SetVersionNumber "src/GoCloudNative.Bff.Authentication.AzureAd/GoCloudNative.Bff.Authentication.AzureAd.csproj" $newVersion
SetVersionNumber "src/GoCloudNative.Bff.Authentication.OpenIdConnect/GoCloudNative.Bff.Authentication.OpenIdConnect.csproj" $newVersion

# dotnet build --configuration release

git add .
git checkout main
git commit -m "release(v$newVersion)"
git push

# # ==== publish to nuget ====

$apiKey = $env:bff_api_key 

$newVersion = "1.0.0-rc.1"
$authPackage = "src/GoCloudNative.Bff.Authentication/bin/release/GoCloudNative.Bff.Authentication.$newVersion.nupkg"
$authPackageAuth0 = "src/GoCloudNative.Bff.Authentication.Auth0/bin/release/GoCloudNative.Bff.Authentication.Auth0.$newVersion.nupkg"
$authPackageAzureAd = "src/GoCloudNative.Bff.Authentication.AzureAd/bin/release/GoCloudNative.Bff.Authentication.AzureAd.$newVersion.nupkg"
$authPackageOpenIdConnect = "src/GoCloudNative.Bff.Authentication.OpenIdConnect/bin/release/GoCloudNative.Bff.Authentication.OpenIdConnect.$newVersion.nupkg"

nuget push $authPackage $apiKey -Source https://api.nuget.org/v3/index.json
nuget push $authPackageAuth0 $apiKey -Source https://api.nuget.org/v3/index.json
nuget push $authPackageAzureAd $apiKey -Source https://api.nuget.org/v3/index.json
nuget push $authPackageOpenIdConnect $apiKey -Source https://api.nuget.org/v3/index.json