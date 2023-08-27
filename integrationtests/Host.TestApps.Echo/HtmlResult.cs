using System.Net.Mime;
using System.Text;

namespace Host.TestApps.TestApi;

internal class HtmlResult : IResult
{
    private string _html = string.Empty;

    public static HtmlResult WithHtml(string html)
    {
        return new HtmlResult { _html = html };
    }

    public HtmlResult AddHtml(string html)
    {
        _html = $"{_html}{html}";
        return this;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var html = $"<html><body>{_html}</body></html>";

        httpContext.Response.ContentType = MediaTypeNames.Text.Html;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(html);
        await httpContext.Response.WriteAsync(html);
    }
}