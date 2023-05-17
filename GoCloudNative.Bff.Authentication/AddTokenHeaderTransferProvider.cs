using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace GoCloudNative.Bff.Authentication;

public class AddTokenHeaderTransferProvider : ITransformProvider
{
    private readonly IIdentityProvider _identityProvider;

    public AddTokenHeaderTransferProvider(IIdentityProvider identityProvider)
    {
        _identityProvider = identityProvider;
    }
    
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // I.L.E.
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // This applies for all clusters
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async x =>
        {   
            if (!x.HttpContext.Session.HasAccessToken())
            {
                return;
            }

            var expiryDate = x.HttpContext.Session.GetExpiryDate();
            var token = x.HttpContext.Session.GetAccessToken();

            if (expiryDate.HasValue && await RenewToken(expiryDate.Value, x))
            {
                token = x.HttpContext.Session.GetAccessToken();
            }

            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }

    private async Task<bool> RenewToken(DateTime expiryDate, RequestTransformContext x)
    {
        var now = DateTimeOffset.UtcNow;
        var expiry = expiryDate.AddSeconds(-15);
        
        if (expiry > now)
        {
            return false;
        }

        var refreshToken = x.HttpContext.Session.GetRefreshToken();

        var renewer = new TokenRenewer(_identityProvider, x.HttpContext.Session);
        await renewer.Renew(refreshToken);
        
        return true;

    }
}