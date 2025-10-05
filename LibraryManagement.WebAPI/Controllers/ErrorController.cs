using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers;

    
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
       
    {
     
        [Route("/error")]
        public IActionResult HandleError() =>
        
            Problem(
               title: "An unexpected fault happened, please try again later!",
               statusCode: StatusCodes.Status500InternalServerError
           );
     }
    

