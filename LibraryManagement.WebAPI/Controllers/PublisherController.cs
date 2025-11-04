using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LibraryManagement.WebAPI.Controllers;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class PublisherController : ControllerBase
{
    private readonly IPublisherService _publisherService;
    private readonly IPublisherMapper _publisherMapper;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IPropertyCheckerService _propertyCheckerService;
    private readonly ILogger<PublisherService> _logger;



    public PublisherController(IPublisherService publisherService, IPublisherMapper publisherMapper, ProblemDetailsFactory problemDetailsFactory, IPropertyCheckerService propertyCheckerService, ILogger<PublisherService> logger)
    {
        _publisherService = publisherService ?? throw new ArgumentNullException(nameof(publisherService));
        _publisherMapper = publisherMapper ?? throw new ArgumentNullException(nameof(publisherMapper));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    [HttpGet]
    public async Task<IActionResult> GetAllPublisher([FromQuery] QueryOptions queryOptions )
    {
        var readPublisherDtos = await _publisherService.ListAllPublisherAsync(queryOptions);

        
        return Ok(readPublisherDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPublisherById(Guid id)
    {
        var publisher = await _publisherService.GetPublisherByIdAsync(id);
        if (publisher == null)
        {
            return NotFound();
        }
        var readPublisherDto = _publisherMapper.ToPublisherReadDto(publisher);
        return Ok(readPublisherDto);
    }
    [HttpGet("{email}")]
    public async Task<IActionResult> GetPublisherById([FromRoute] string email)
    {
        var publisher = await _publisherService.GetPublisherByEmailAsync(email);
        if (publisher == null)
        {
            return NotFound();
        }
        var readPublisherDto = _publisherMapper.ToPublisherReadDto(publisher);
        return Ok(readPublisherDto);
    }
    [HttpPost]
    public async Task<IActionResult> CreatePublisher([FromBody] PublisherCreateDto publisherCreateDto)
    {
        if (publisherCreateDto == null)
        {
            return BadRequest();
        }
        if(!ModelState.IsValid)
        {
           _logger.LogDebug("Invalid model state for the PublisherCreateDto object");
            return UnprocessableEntity(ModelState);
        }
        var createdPublisher = await _publisherService.CreatePublisherAsync(publisherCreateDto);
        var readPublisherDto = _publisherMapper.ToPublisherReadDto(createdPublisher);
        return CreatedAtAction(nameof(GetPublisherById), new { id = createdPublisher!.Id }, readPublisherDto);
    }
    [HttpDelete("{id}")] 
    public async Task<IActionResult> DeletePublisherById([FromRoute] Guid id)
    {
        var publisher = await _publisherService.DeletePublisherAsync(id);
        if(publisher == null)
        {
            return NotFound();
        }

        var publisherReadDto = _publisherMapper.ToPublisherReadDto(publisher);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePublisher([FromRoute] Guid id, [FromBody] PublisherUpdateDto publisherUpdateDto)
    {
        if (publisherUpdateDto == null)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            _logger.LogDebug("Invalid model state for the PublisherUpdateDto object");
            return UnprocessableEntity(ModelState);
        }
        var updatedPublisher = await _publisherService.UpdatePublisherAsync(id,publisherUpdateDto);
        if (updatedPublisher == null)
        {
            return NotFound();
        }
        var readPublisherDto = _publisherMapper.ToPublisherReadDto(updatedPublisher);
        return Ok(readPublisherDto);
    }



}
