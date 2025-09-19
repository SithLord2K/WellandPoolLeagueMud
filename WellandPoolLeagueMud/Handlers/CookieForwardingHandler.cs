using System.Net.Http.Headers;

namespace WellandPoolLeagueMud.Handlers;

public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookie = _httpContextAccessor.HttpContext?.Request.Headers["Cookie"];

        // The .ToArray() fix resolves the ambiguity
        if (cookie.HasValue)
        {
            request.Headers.Add("Cookie", cookie.Value.ToArray());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}