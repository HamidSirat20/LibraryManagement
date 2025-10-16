using LibraryManagement.WebAPI.Models.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers
{
    [Route("api/v{version:ApiVersion}")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            var links = new List<LinkDto>();
            links.Add(
                new LinkDto(Url.Link("GetRoot", new { }), "self", "GET")
            );
            links.Add(
                new LinkDto(Url.Link("GetAllBooks", null), "GetBooks", "GET")
            );
            links.Add(
                new LinkDto(Url.Link("CreateBook", null), "CreateBook", "POST")
            );     
            return Ok(links);
        }
    }
}
