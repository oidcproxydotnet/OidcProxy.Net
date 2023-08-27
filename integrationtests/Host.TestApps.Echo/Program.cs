namespace Host.TestApps.TestApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        MapEchoEndpoint(app);

        app.Run();
    }

    public static void MapEchoEndpoint(WebApplication app)
    {
        app.MapGet("api/echo", async (context) =>
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
    }
}
