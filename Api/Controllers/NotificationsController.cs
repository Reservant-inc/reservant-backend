using System.Globalization;
using System.Text;
using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notifications;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Reservant.Api.Controllers;

/// <summary>
/// Notification managing
/// </summary>
[Route("/notifications")]
[ApiController]
[Authorize]
public class NotificationsController(NotificationService service) : StrictController
{
    /// <summary>
    /// Get all notifications
    /// </summary>
    /// <remarks>
    /// <a href="/notifications/types">Possible notification types</a>
    /// </remarks>
    /// <param name="unreadOnly">Return only unread notifications</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page (max 100)</param>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<NotificationService>(nameof(NotificationService.GetNotifications))]
    public async Task<ActionResult<Pagination<NotificationVM>>> GetNotifications(
        bool unreadOnly = false, int page = 0, int perPage = 100)
    {
        var result = await service.GetNotifications(unreadOnly, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get data to show as notification bubbles
    /// </summary>
    [HttpGet("bubbles")]
    public async Task<NotificationBubblesVM> GetBubbles()
    {
        return await service.GetBubbles();
    }

    /// <summary>
    /// Mark notifications as read.
    /// </summary>
    /// <remarks>
    /// Does not check the IDs, does not update already read notifications.
    /// </remarks>
    [HttpPost("mark-read")]
    public async Task<ActionResult> MarkRead(MarkNotificationsReadDto dto)
    {
        await service.MarkRead(dto);
        return Ok();
    }

    /// <summary>
    /// Renders an HTML page documenting possible notification types
    /// </summary>
    [HttpGet("types")]
    [AllowAnonymous]
    public ActionResult GetPossibleNotificationTypes()
    {
        var types = typeof(NotificationDetails).Assembly
            .DefinedTypes
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(NotificationDetails)))
            .Select(t => new NotificationTypeVM
            {
                Name = t.Name,
                DetailsFields = t.DeclaredProperties
                    .Where(p => p.IsPubliclyReadable())
                    .ToDictionary(
                        p => Utils.PropertyPathToCamelCase(p.Name),
                        p => p.PropertyType.Name)
            })
            .ToList();

        var htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<h1>Notification Types</h1>");

        foreach (var notificationType in types)
        {
            htmlBuilder.AppendLine(CultureInfo.InvariantCulture, $"<h2>{notificationType.Name}</h2>");
            htmlBuilder.AppendLine("<ul>");

            foreach (var (name, type) in notificationType.DetailsFields)
            {
                htmlBuilder.AppendLine(CultureInfo.InvariantCulture, $"<li>{name} : {type}</li>");
            }

            htmlBuilder.AppendLine("</ul>");
        }

        return Content(htmlBuilder.ToString(), "text/html");
    }
}
