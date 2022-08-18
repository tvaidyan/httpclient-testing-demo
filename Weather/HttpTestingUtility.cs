using Moq;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using System.Net;

namespace HttpUnitTesting.Weather;
public class HttpTestingUtilityParameters
{
    public string HttpClientName { get; set; } = null;
    public HttpResponseMessage ReturnMessage { get; set; } = new HttpResponseMessage()
    {
        StatusCode = HttpStatusCode.OK
    };
}

public class HttpTestingUtility
{
    public HttpTestingUtility(HttpTestingUtilityParameters parameters)
    {
        Handler = new Mock<HttpMessageHandler>();
        Handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(parameters.ReturnMessage)
            .Verifiable();
        Handler.As<IDisposable>().Setup(s => s.Dispose());

        Factory = Handler.CreateClientFactory();

        if (parameters.HttpClientName is not null)
        {
            Mock.Get(Factory).Setup(x => x.CreateClient(parameters.HttpClientName))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                client.BaseAddress = new Uri("http://www.doesntexist.com/");
                return client;
            });
        }
    }

    public Mock<HttpMessageHandler> Handler { get; }
    public IHttpClientFactory Factory { get; }
}
