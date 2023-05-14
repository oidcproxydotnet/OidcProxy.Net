using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace GoCloudNative.Bff.Authentication;

public class AddTokenHeaderTransferProvider : ITransformProvider
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
            const string sessionKey = "token_key";
            if (!x.HttpContext.Session.Keys.Contains(sessionKey))
            {
                return ValueTask.CompletedTask;
            }

            var token = x.HttpContext.Session.GetString(sessionKey);
            x.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return ValueTask.CompletedTask;
        });
    }
}