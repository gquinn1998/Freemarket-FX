using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("/error")]
public class ErrorController : ControllerBase
{
    [HttpGet]
    public IActionResult HandleError() => Problem();
}
