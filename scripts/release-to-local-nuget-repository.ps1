$version = '0.0.19'
$repoDir = $home + '/local-nuget-repository'
$repoSourceDir = $repoDir + '/source'

# <methods>
    function Add-Package-To-Local-Repository($package)
    {
        cp "../src/GoCloudNative.Bff.Authentication/bin/Release/$package" $repoSourceDir
        nuget add "$repoSourceDir/$package" -Source $repoSourceDir
    }

    function Create-Directory($dir)
    {
        if ((Test-Path $dir) -ne $true)
        {
            New-Item -ItemType Directory $dir
        } 
    }
# </methods>

# <procedure>
    dotnet build ../Bff.sln --configuration release

    Create-Directory $repoSourceDir

    Add-Package-To-Local-Repository "GoCloudNative.Bff.Authentication.$version.nupkg" 
    Add-Package-To-Local-Repository "GoCloudNative.Bff.Authentication.Auth0.$version.nupkg"
    Add-Package-To-Local-Repository "GoCloudNative.Bff.Authentication.AzureAd.$version.nupkg"
    Add-Package-To-Local-Repository "GoCloudNative.Bff.Authentication.OpenIdConnect.$version.nupkg"
# </procedure>