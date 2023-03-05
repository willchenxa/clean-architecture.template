
using Microsoft.AspNetCore.Mvc;
using MapsterMapper;
using MediatR;
using CleanArchitecture.Application.Weather.Queries;
using CleanArchitecture.Contracts.Weather;

namespace CleanArchitecture.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ApiController
{
    private readonly ISender _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(IMapper mapper, ISender mediator, ILogger<WeatherForecastController> logger)
    {
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet(Name = "Forecast")]
    public async Task<IActionResult> Get(WeatherRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<WeatherQuery>(request);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            weatherResult => Ok(_mapper.Map<WeatherResponse>(weatherResult)),
            errors => Problem(errors)
        );
    }
}