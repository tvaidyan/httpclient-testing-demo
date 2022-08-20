using System.Net;
using Moq;
using Moq.Contrib.HttpClient;
using Xunit;

namespace HttpUnitTesting.Weather;
public class WeatherHandlerTests
{
    [Fact]
    public async Task Handle_CallsRandomNumberExternalApi()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var factory = handler.CreateClientFactory();

        var numbers = new int[] { 100, 99, 98, 97, 96, 95, 94, 93, 92, 91 };

        // See "MatchesQueryParameters" here for tips on matching dynamic query strings:
        // https://github.com/maxkagamine/Moq.Contrib.HttpClient/blob/master/Moq.Contrib.HttpClient.Test/RequestExtensionsTests.cs

        handler.SetupRequest(HttpMethod.Get, "http://www.randomnumberapi.com/api/v1.0/random?min=26&max=38&count=10")
            .ReturnsJsonResponse(numbers);

        var sut = new GetWeatherForecastHandler(factory);

        // Act
        await sut.Handle(new WeatherForecastRequest(), new CancellationToken());

        // Assert
        handler.VerifyAll();
    }

    [Fact]
    public async Task Handle_Returns5DayForecast()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var factory = handler.CreateClientFactory();

        var numbers = new int[] { 100, 99, 98, 97, 96, 95, 94, 93, 92, 91 };

        handler.SetupRequest(HttpMethod.Get, "http://www.randomnumberapi.com/api/v1.0/random?min=26&max=38&count=10")
            .ReturnsJsonResponse(numbers);

        var sut = new GetWeatherForecastHandler(factory);

        // Act
        var results = await sut.Handle(new WeatherForecastRequest(), new CancellationToken());

        // Assert
        Assert.Equal(5, results.WeatherForecasts.Count());
        Assert.Equal(numbers.Skip(1).Take(5), results.WeatherForecasts.Select(x => x.TemperatureC));
    }

    [Fact]
    public async Task Handle_ThrowsErrorWhenRandomNumberApiCallFails()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var factory = handler.CreateClientFactory();

        handler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.ServiceUnavailable);

        var sut = new GetWeatherForecastHandler(factory);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await sut.Handle(new WeatherForecastRequest(), new CancellationToken());
        });
    }
}