using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace OidcProxy.Net;

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
            var session = AuthSession.Create(x.HttpContext.Session);
            
            if (!session.HasAccessToken())
            {
                return ValueTask.CompletedTask;
            }

            var token = session.GetAccessToken();
            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return ValueTask.CompletedTask;
        });
    }
}