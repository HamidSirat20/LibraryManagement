﻿using Asp.Versioning;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


        [HttpGet("{bookIds}", Name = "GetBookCollection")]
        public async Task<ActionResult<BookReadDto>> GetBookCollectionAsync([ModelBinder(BinderType =typeof(ArrayModelBinder))] [FromRoute] IEnumerable<Guid> bookIds)
        {
            if (bookIds == null || !bookIds.Any())
            {
                throw new ArgumentNullException(nameof(bookIds));
            }
            var books = await _bookCollectionService.GetBookCollectionAsync(bookIds);
            if(books.Count() != bookIds.Count())
            {
                return NotFound();
            }
            return Ok(books);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> CreateBookCollection([FromBody] IEnumerable<BookCreateDto> bookCreateDtos)
        {
            if (bookCreateDtos == null || !bookCreateDtos.Any())
            {
                return BadRequest("Book collection cannot be null or empty.");
            }
          var createdBooks =  await _bookCollectionService.CreateBooksAsync(bookCreateDtos);

            var booksId = string.Join(",", createdBooks.Select(x => x.Id));

            return CreatedAtRoute("GetBookCollection", new {bookIds =booksId }, createdBooks);
        }
    }
}
