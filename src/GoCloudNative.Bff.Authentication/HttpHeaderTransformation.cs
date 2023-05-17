using System.Net.Http.Headers;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace GoCloudNative.Bff.Authentication;

public class HttpHeaderTransformation : ITransformProvider
{
    private readonly IIdentityProvider _identityProvider;

    public HttpHeaderTransformation(IIdentityProvider identityProvider)
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

            var token = x.HttpContext.Session.GetAccessToken();
            
            var factory = new TokenFactory(_identityProvider, x.HttpContext.Session);
            if (await factory.RenewAccessTokenIfExpired())
            {
                token = x.HttpContext.Session.GetAccessToken();
            }

            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });
    }
}