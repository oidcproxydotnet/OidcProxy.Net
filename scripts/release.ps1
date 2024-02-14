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

# dotnet test

# if ($lastexitcode -eq 1) {
#     write-host "Aborted. Package has not been published to nuget. Cannot deploy when tests fail. Fix the tests first, then run the script again."
#     exit
# }

# write-host "Test succeeded."
# write-host "================================="
# write-host ""

# ==== set new version number ====

if ($newVersion -eq $null) {
    
    write-host "Target version number? (Type and hit enter.)" -ForegroundColor Green
    $newVersion = read-host
}

# SetVersionNumber "src/OidcProxy.Net/OidcProxy.Net.csproj" $newVersion
# SetVersionNumber "src/OidcProxy.Net.Auth0/OidcProxy.Net.Auth0.csproj" $newVersion
# SetVersionNumber "src/OidcProxy.Net.AzureAd/OidcProxy.Net.AzureAd.csproj" $newVersion
# SetVersionNumber "src/OidcProxy.Net.OpenIdConnect/OidcProxy.Net.OpenIdConnect.csproj" $newVersion

# dotnet build --configuration release

# git add .
# git checkout main
# git commit -m "release(v$newVersion)"
# git push

# # ==== publish to nuget ====

$apiKey = $env:bff_api_key 

$authPackage                = "src/OidcProxy.Net/bin/release/OidcProxy.Net.$newVersion.nupkg"
$authPackageAuth0           = "src/OidcProxy.Net.Auth0/bin/release/OidcProxy.Net.Auth0.$newVersion.nupkg"
$authPackageAzureAd         = "src/OidcProxy.Net.EntraId/bin/release/OidcProxy.Net.EntraId.$newVersion.nupkg"
$authPackageOpenIdConnect   = "src/OidcProxy.Net.OpenIdConnect/bin/release/OidcProxy.Net.OpenIdConnect.$newVersion.nupkg"

nuget push $authPackage $apiKey -Source https://api.nuget.org/v3/index.json
nuget push $authPackageAuth0 $apiKey -Source https://api.nuget.org/v3/index.json
nuget push $authPackageAzureAd $apiKey -Source https://api.nuget.org/v3/index.json
nuget push $authPackageOpenIdConnect $apiKey -Source https://api.nuget.org/v3/index.json