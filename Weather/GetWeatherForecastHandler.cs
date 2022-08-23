using MediatR;

namespace HttpUnitTesting.Weather;

public class WeatherForecastRequest :
    IRequest<WeatherForecastResponse>
{
    // your request params will go here
}

public class GetWeatherForecastHandler : IRequestHandler<WeatherForecastRequest, WeatherForecastResponse>
{
    private readonly HttpClient httpClient;
    public GetWeatherForecastHandler(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient();
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public async Task<WeatherForecastResponse> Handle(WeatherForecastRequest request, CancellationToken cancellationToken)
    {
        var httpResponse = await httpClient.GetAsync("http://www.randomnumberapi.com/api/v1.0/random?min=26&max=38&count=10");

        if (httpResponse.IsSuccessStatusCode)
        {
            var forecast = await httpResponse.Content.ReadFromJsonAsync<int[]>();
            var weatherForecastResponse = new WeatherForecastResponse
            {
                WeatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = forecast![index],
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
            };

            return await Task.FromResult(weatherForecastResponse);
        }

        throw new HttpRequestException("Uh-oh!  Bad stuff happened.");
    }
}

public class WeatherForecastResponse
{
    public IEnumerable<WeatherForecast> WeatherForecasts { get; set; } = new List<WeatherForecast>();
}
