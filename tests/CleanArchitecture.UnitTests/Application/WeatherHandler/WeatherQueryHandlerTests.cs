using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Queries.Weather;
using CleanArchitecture.Application.Weather.Queries;
using CleanArchitecture.Domain.Weather;
using CleanArchitecture.Application.Queries.Weather.Common;
using CleanArchitecture.UnitTests.Data;
using FluentAssertions.Execution;

namespace CleanArchitecture.UnitTests.Application.WeatherQueryHandlerTests;

public class LoginQueryHandlerTests
{
    private readonly IWeatherProxy _weatherProxy;
    private readonly IDirectGeocodingProxy _directGeocodingProxy;
    private readonly WeatherQueryHandler _weatherQueryHandler;

    public LoginQueryHandlerTests()
    {
        _weatherProxy = Substitute.For<IWeatherProxy>();
        _directGeocodingProxy = Substitute.For<IDirectGeocodingProxy>();

        _weatherQueryHandler = new WeatherQueryHandler(_weatherProxy, _directGeocodingProxy);
    }

    [Fact]
    public async Task ReturnWeather_WhenRequest_IsValid()
    {
        // Arrange
        var query = new WeatherQuery("Brisbane", "QLD", "4000", "AU");
        _directGeocodingProxy.GetLocation(Arg.Any<WeatherQuery>(), new CancellationToken()).Returns(
            new Location
            {
                Latitude = 1.1,
                Longitude = 1.1,
                Country = "Australia",
                State = "QLD",
                City = "Brisbane"
            });
        _weatherProxy.GetWeatherResult(Arg.Any<double>(), Arg.Any<double>(), new CancellationToken())
            .Returns(MockData.ReturnWeatherResult());

        // Act
        var result = await _weatherQueryHandler.Handle(query, new CancellationToken());

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Value.Location.City.Should().BeEquivalentTo("Brisbane");
            result.Value.Temperature.CurrentTemp.Should().Be(23);
            result.Value.Temperature.FeelsLike.Should().Be(27.3);
            result.Value.Temperature.MaxTemperature.Should().Be(29);
            result.Value.Temperature.Humidity.Should().Be(0.14);
            result.Value.Temperature.SeeLevel.Should().Be(1);
        }
    }

    [Fact]
    public async Task ReturnErros_WhenLocationIsNotValidate_ReturnError()
    {
        // Arrange
        var query = new WeatherQuery("Brisbane", "QLD", "4000", "AU");
        _directGeocodingProxy.GetLocation(Arg.Any<WeatherQuery>(), new CancellationToken()).ReturnsNull();

        // Act
        var result = await _weatherQueryHandler.Handle(query, new CancellationToken());

        // Assert
        result.Should().NotBeNull();
        result.Errors.Count.Should().Be(1);
        result.Errors.First().Code.Should().BeEquivalentTo("Location.NotFound");
        result.Errors.First().Description.Should().BeEquivalentTo("Can not find location.");
    }
}