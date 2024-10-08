using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.FileUpload;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// File uploading.
/// </summary>
[ApiController, Route("/uploads")]
public class FileUploadsController(FileUploadService fileUploadService)
    : StrictController
{
    /// <summary>
    /// Upload a file to the server
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<FileUploadService>(nameof(FileUploadService.SaveFileAsync))]
    public async Task<ActionResult<UploadVM>> UploadFile([FromForm] UploadRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await fileUploadService.SaveFileAsync(request, userId.Value);
        return OkOrErrors(result);
    }
}
