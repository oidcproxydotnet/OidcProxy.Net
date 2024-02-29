$version = '2.0.0'
$repoDir = $home + '/local-nuget-repository'
$repoSourceDir = $repoDir + '/source'

# <methods>
    function Add-Package-To-Local-Repository($project, $package)
    {
        cp "src/$project/bin/Release/$package" $repoSourceDir
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

    Add-Package-To-Local-Repository "OidcProxy.Net" "OidcProxy.Net.$version.nupkg" 
    Add-Package-To-Local-Repository "OidcProxy.Net.Auth0" "OidcProxy.Net.Auth0.$version.nupkg"
    Add-Package-To-Local-Repository "OidcProxy.Net.EntraId" "OidcProxy.Net.EntraId.$version.nupkg"
    Add-Package-To-Local-Repository "OidcProxy.Net.OpenIdConnect" "OidcProxy.Net.OpenIdConnect.$version.nupkg"
# </procedure>