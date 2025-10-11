using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class BookCollectionsController : ControllerBase
    {
        private readonly IBookCollectionService _bookCollectionService;

        public BookCollectionsController(IBookCollectionService bookCollectionService)
        {
            _bookCollectionService = bookCollectionService;
        }
        [HttpPost]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> CreateBookCollection([FromBody] IEnumerable<BookCreateDto> bookCreateDtos)
        {
            if (bookCreateDtos == null || !bookCreateDtos.Any())
            {
                return BadRequest("Book collection cannot be null or empty.");
            }
          var createdBooks =  await _bookCollectionService.CreateBooksAsync(bookCreateDtos);
            return CreatedAtAction(nameof(CreateBookCollection), createdBooks);
        }
    }
}
