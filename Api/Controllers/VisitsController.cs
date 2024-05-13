using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing visits
/// </summary>
[ApiController, Route("/visits")]
public class VisitsController(VisitService visitService) : Controller
{
}
