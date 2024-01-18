using System.Net.Http.Headers;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace GoCloudNative.Bff.Authentication;

internal class HttpHeaderTransformation : ITransformProvider
{   
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
        context.AddRequestTransform(x =>
        {   
            if (!x.HttpContext.Session.HasAccessToken())
            {
                return ValueTask.CompletedTask;
            }

            var token = x.HttpContext.Session.GetAccessToken();
            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return ValueTask.CompletedTask;
        });
    }
}