using Moq.Contrib.HttpClient;
using Xunit;

namespace HttpUnitTesting.Weather;
public class WeatherHandlerTests
{
    [Fact]
    public async Task Handle_CallsRandomNumberExternalApi()
    {
        // Arrange
        var httpTestingUtility = new HttpTestingUtility(new HttpTestingUtilityParameters
        {
            ReturnMessage = new HttpResponseMessage
            {
                Content = new StringContent("[100,99,98,97,96,95,94,93,92,91]")
            }
        });
        var sut = new GetWeatherForecastHandler(httpTestingUtility.Factory);

        // Act
        await sut.Handle(new WeatherForecastRequest(), new CancellationToken());

        // Assert
        // Did the Handle method call the Random Number API?
        httpTestingUtility.Handler.VerifyRequest(x => x.RequestUri.AbsoluteUri.Contains("randomnumberapi.com"));

        // Was it a GET request?
        httpTestingUtility.Handler.VerifyRequest(x => x.Method == HttpMethod.Get);
    }

    [Fact]
    public async Task Handle_Returns5DayForecast()
    {
        // Arrange
        var httpTestingUtility = new HttpTestingUtility(new HttpTestingUtilityParameters
        {
            ReturnMessage = new HttpResponseMessage
            {
                Content = new StringContent("[100,99,98,97,96,95,94,93,92,91]")
            }
        });
        var sut = new GetWeatherForecastHandler(httpTestingUtility.Factory);

        // Act
        var results = await sut.Handle(new WeatherForecastRequest(), new CancellationToken());

        // Assert
        Assert.Equal(5, results.WeatherForecasts.Count());
    }

    [Fact]
    public async Task Handle_ThrowsErrorWhenRandomNumberApiCallFails()
    {
        // Arrange
        var httpTestingUtility = new HttpTestingUtility(new HttpTestingUtilityParameters
        {
            ReturnMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.ServiceUnavailable
            }
        });
        var sut = new GetWeatherForecastHandler(httpTestingUtility.Factory);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await sut.Handle(new WeatherForecastRequest(), new CancellationToken());
        });
    }
}