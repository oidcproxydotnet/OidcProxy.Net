namespace Host;

public static class TestEndpoints
{
    public static WebApplication AddTestingEndpoints(this WebApplication app)
    {
        app.Map("/", () => HtmlResult.WithHtml("<h1>Welcome</h1>")
            .AddHtml("<ul>")    
            .AddHtml("<li><a href=\"/auth0/login\">/auth0/login</a></li>")
            .AddHtml("<li><a href=\"/oidc/login\">/oidc/login</a></li>")
            .AddHtml("<li><a href=\"/aad/login\">/aad/login</a></li>")
            .AddHtml("</ul>"));

        app.Map("account/oops", () => Results.Text("Oops..."));

        app.Map("account/welcome", () => HtmlResult
            .WithHtml("<h1>Welcome</h1>")
            .AddHtml("<p><a href=\"/echo\">Echo</a></p>")
            .AddHtml("<h2>Auth0</h2>")
            .AddHtml("<ul>")
            .AddHtml("<li><a href=\"/auth0/me\">/auth0/me</a></li>")
            .AddHtml("<li><a href=\"/auth0/end-session\">/auth0/end-session</a></li>")
            .AddHtml("</ul>")
            .AddHtml("<h2>IdentityServer4</h2>")
            .AddHtml("<ul>")
            .AddHtml("<li><a href=\"/oidc/me\">/oidc/me</a></li>")
            .AddHtml("<li><a href=\"/oidc/end-session\">/oidc/end-session</a></li>")
            .AddHtml("</ul>")
            .AddHtml("<h2>Azure Ad</h2>")
            .AddHtml("<ul>")
            .AddHtml("<li><a href=\"/aad/me\">/aad/me</a></li>")
            .AddHtml("<li><a href=\"/aad/end-session\">/aad/end-session</a></li>")
            .AddHtml("</ul>"));

        app.MapGet("echo/echo", async (context) =>
        {
            var htmlResult = HtmlResult
                .WithHtml("<h1>Echo</h1>")
                .AddHtml("<ul>");

            foreach (var header in context.Request.Headers)
            {
                htmlResult.AddHtml($"<li><strong>{header.Key}</strong></li>");
                htmlResult.AddHtml($"<li>{header.Value.ToString()}</li>");
                htmlResult.AddHtml($"<li>&nbsp;</li>");
            }

            htmlResult.AddHtml("</ul>");

            await htmlResult.ExecuteAsync(context);
        });

        return app;
    }
}