using Asp.Versioning;
using AutoMapper;
using BoldareBrewery.Api.Models.DTOs.Requests;
using BoldareBrewery.Api.Models.DTOs.Responses;
using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.UseCases.SearchBreweries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoldareBrewery.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class BreweriesController : ControllerBase
    {
        private readonly ISearchBreweriesUseCase _searchUseCase;
        private readonly IMapper _mapper;

        public BreweriesController(ISearchBreweriesUseCase searchUseCase, IMapper mapper)
        {
            _searchUseCase = searchUseCase;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SearchBreweriesResponseDto>> GetBreweries(
        [FromQuery] SearchBreweriesRequestDto requestDto)
        {
            var useCaseRequest = _mapper.Map<SearchBreweriesRequest>(requestDto);
            var result = await _searchUseCase.SearchAsync(useCaseRequest);

            return result.Match<ActionResult<SearchBreweriesResponseDto>>(
                onSuccess: response =>
                {
                    var responseDto = _mapper.Map<SearchBreweriesResponseDto>(response);
                    return Ok(responseDto);
                },
                onFailure: error => error.Code switch
                {
                    "ValidationFailure" => BadRequest(new { error.Code, error.Message }),
                    "NotFound" => NotFound(new { error.Code, error.Message }),
                    _ => StatusCode(500, new { error.Code, error.Message })
                }
            );
        }
    }
}
