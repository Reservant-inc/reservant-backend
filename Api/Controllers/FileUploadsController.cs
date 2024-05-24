using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.FileUpload;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// File uploading.
/// </summary>
[ApiController, Route("/uploads")]
public class FileUploadsController(FileUploadService fileUploadService, UserManager<User> userManager)
    : StrictController
{
    /// <summary>
    /// Upload a file to the server
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<UploadVM>> UploadFile([FromForm] UploadRequest request)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await fileUploadService.SaveFileAsync(request, userId);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }
}
