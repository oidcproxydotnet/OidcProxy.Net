using System.Net.Http.Headers;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
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
                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(tokenHeader.Exp.Value);
                var oneMinuteAgo = DateTimeOffset.UtcNow.AddMinutes(-1);
                
                if (expiryDate < oneMinuteAgo)
                {
                    // todo: Implement a mechanism to prevent refreshing the token extremely often
                    //       case: A page invokes multiple endpoints, in that case, this code will get multiple tokens at the same time.
                    var refreshToken = x.HttpContext.Session.GetString(LoginEndpoints.RefreshTokenKey);
                    var tokenResponse = await _identityProvider.RefreshTokenAsync(refreshToken);

                    // todo: revoke the old refresh token
                    
                    x.HttpContext.Session.Save(LoginEndpoints.TokenKey, tokenResponse.access_token);
                    x.HttpContext.Session.Save(LoginEndpoints.RefreshTokenKey, tokenResponse.refresh_token);
                }
            }

            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }
}