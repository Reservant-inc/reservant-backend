﻿using ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notification;
using Reservant.Api.Services;

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
    /// <param name="unreadOnly">Return only unread notifications</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page (max 100)</param>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<NotificationService>(nameof(NotificationService.GetNotificaions))]
    public async Task<ActionResult<Pagination<NotificationVM>>> GetNotifications(
        bool unreadOnly = false, int page = 0, int perPage = 100)
    {
        var result = await service.GetNotificaions(unreadOnly, page, perPage);
        return OkOrErrors(result);
    }
}
