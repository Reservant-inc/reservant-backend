using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// File uploading.
/// </summary>
[ApiController, Route("/uploads")]
public class FileUploadsController(FileUploadService fileUploadService) : Controller
{
    /// <summary>
    /// Upload a file to the server
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<string>> UploadFile([FromForm] UploadRequest request)
    {
        var path = await fileUploadService.SaveFileAsync(request);
        return Ok(path);
    }
}
