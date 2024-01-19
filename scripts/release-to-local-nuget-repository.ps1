$version = '0.0.19'
$repoDir = $home + '/local-nuget-repository'
$repoSourceDir = $repoDir + '/source'

# <methods>
    function Add-Package-To-Local-Repository($package)
    {
        cp "../src/$package/bin/Release/$package" $repoSourceDir
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

    Add-Package-To-Local-Repository "OidcProxy.Net.$version.nupkg" 
    Add-Package-To-Local-Repository "OidcProxy.Net.$version.nupkg"
    Add-Package-To-Local-Repository "OidcProxy.Net.$version.nupkg"
    Add-Package-To-Local-Repository "OidcProxy.Net.$version.nupkg"
# </procedure>