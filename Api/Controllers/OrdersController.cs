using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing orders
/// </summary>
[ApiController, Route("/orders")]
public class OrdersController(OrderService orderService) : Controller
{
}
