using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIdDictServer.Models;

namespace OpenIdDictServer;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        
        await EnsureTestUserCreated(scope);
        
        await EnsureClientCreated(scope, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
    private static async Task EnsureClientCreated(AsyncServiceScope scope, CancellationToken cancellationToken)
    {
        const string clientId = "bff";
        const string clientSecret = "secret";
        const string redirectUrl = "https://localhost:8443/.auth/login/callback";
        
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var client = await manager.FindByClientIdAsync(clientId, cancellationToken);
        await manager.DeleteAsync(client);
        
        if (await manager.FindByClientIdAsync(clientId, cancellationToken) == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUris =
                {
                    new Uri(redirectUrl)
                },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    
                    OpenIddictConstants.Permissions.ResponseTypes.Code
                }
            }, cancellationToken);
        }
    }

    private static async Task EnsureTestUserCreated(AsyncServiceScope scope)
    {
        using var userService = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var johnDoe = new ApplicationUser
        {
            UserName = "johndoe@oidcproxy.net",
            Email = "johndoe@oidcproxy.net"
        };

        await userService.CreateAsync(johnDoe, "~l9u9D1Xd)Wxd'y6zp_\"]ocj");
    }
}