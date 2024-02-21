using OpenIddict.Abstractions;

namespace OpenIdDictServer;

public class ClientConfiguration : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ClientConfiguration(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        
        await EnsureClientCreated(scope, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
    private static async Task EnsureClientCreated(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        const string clientId = "bff";
        const string clientSecret = "secret";
        
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var client = await manager.FindByClientIdAsync(clientId, cancellationToken);
        if (client != null)
        {
            await manager.DeleteAsync(client, cancellationToken);
        }

        if (await manager.FindByClientIdAsync(clientId, cancellationToken) == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                RedirectUris =
                {
                    new Uri("https://localhost:8443/.auth/login/callback"),
                    new Uri("https://localhost:8444/.auth/login/callback"),
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:8443/"),
                    new Uri("https://localhost:8444/")
                },
                
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                },
                
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
                }
            }, cancellationToken);
        }
    }
}