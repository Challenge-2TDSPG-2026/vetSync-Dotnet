using System.Net;

namespace VetApi.Tests.Unit.Services;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;
    public HttpRequestMessage? UltimaRequisicao { get; private set; }
    public string? UltimoCorpoRequisicao { get; private set; }

    public FakeHttpMessageHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UltimaRequisicao = request;
        if (request.Content is not null)
            UltimoCorpoRequisicao = await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody)
        };
    }
}
