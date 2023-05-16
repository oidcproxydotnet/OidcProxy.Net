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
            if (!x.HttpContext.Session.Keys.Contains(LoginEndpoints.TokenKey))
            {
                return;
            }

            var token = x.HttpContext.Session.GetString(LoginEndpoints.TokenKey);
            
            var tokenHeader = token.ParseJwtPayload();
            if (tokenHeader.Exp.HasValue)
            {
                if (await RenewToken(tokenHeader, x))
                {
                    token = x.HttpContext.Session.GetString(LoginEndpoints.TokenKey);
                }
            }

            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }

    private async Task<bool> RenewToken(JwtPayload tokenHeader, RequestTransformContext x)
    {
        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(tokenHeader.Exp.Value - 60);
        var now = DateTimeOffset.UtcNow;

        if (expiryDate > now)
        {
            return false;
        }

        var refreshToken = x.HttpContext.Session.GetString(LoginEndpoints.RefreshTokenKey);

        var renewer = new TokenRenewer(_identityProvider, x.HttpContext.Session);
        await renewer.Renew(refreshToken);
        
        return true;

    }
}