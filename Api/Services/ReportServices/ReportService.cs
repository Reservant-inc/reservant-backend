using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Validation;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// service for managing user reports
/// </summary>
/// <param name="mapper"></param>
/// <param name="context"></param>
public class ReportService(
    IMapper mapper,
    ApiDbContext context)
{
    /// <summary>
    /// Function for getting the reports as Customer Support
    /// </summary>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <returns>list of reports that match given parameters</returns>
    public async Task<Result<List<ReportVM>>> GetReportsAsync(
        DateTime? dateFrom,
        DateTime? dateUntil,
        string? category,
        string? reportedUserId,
        int? restaurantId)
    {
        IQueryable<Report> reports = context.Reports;

        reports = specifyQuery(reports, dateFrom, dateUntil, category, reportedUserId, restaurantId);
        var res = await reports.ToListAsync();
        return mapper.Map<List<ReportVM>>(res);
    }

    /// <summary>
    /// Function for getting the reports as a normal user
    /// </summary>
    /// <param name="user">user that calls the function</param>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <returns>list of reports that match given parameters</returns>
    public async Task<Result<List<ReportVM>>> GetMyReportsAsync(
    User user,
    DateTime? dateFrom,
    DateTime? dateUntil,
    string? category,
    string? reportedUserId,
    int? restaurantId)
    {
        IQueryable<Report> reports = context.Reports.Where(r => r.CreatedById == user.Id);

        reports = specifyQuery(reports, dateFrom, dateUntil, category, reportedUserId, restaurantId);
        var res = await reports.ToListAsync();
        return mapper.Map<List<ReportVM>>(res);
    }
    /// <summary>
    /// Function for getting the reports as a restaurant owner
    /// </summary>
    /// <param name="user">user that calls the function</param>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <returns>list of reports that match given parameters</returns>
    public async Task<Result<List<ReportVM>>> GetMyRestaurantsReportsAsync(
    User user,
    DateTime? dateFrom,
    DateTime? dateUntil,
    string? category,
    string? reportedUserId,
    int? restaurantId)
    {
        IQueryable<Report> reports = context.Reports.Where(r => r.Visit!.Restaurant.Group.OwnerId == user.Id);

        reports = specifyQuery(reports, dateFrom, dateUntil, category, reportedUserId, restaurantId);
        var res = await reports.ToListAsync();
        return mapper.Map<List<ReportVM>>(res);
    }

    /// <summary>
    /// Filters query through optional parameters
    /// </summary>
    /// <param name="reports"></param>
    /// <param name="dateFrom"></param>
    /// <param name="dateUntil"></param>
    /// <param name="category"></param>
    /// <param name="reportedUserId"></param>
    /// <param name="restaurantId"></param>
    /// <returns></returns>
    private static IQueryable<Report> specifyQuery(IQueryable<Report> reports,
        DateTime? dateFrom,
        DateTime? dateUntil,
        string? category,
        string? reportedUserId,
        int? restaurantId)
    {
        if (dateFrom is not null)
        {
            reports = reports.Where(r => r.ReportDate >= dateFrom);
        }
        if (dateUntil is not null)
        {
            reports = reports.Where(r => r.ReportDate <= dateUntil);
        }
        if (category is not null)
        {
            reports = reports.Where(r => r.ReportedUserId!.Value.ToString() == reportedUserId);
        }
        if (restaurantId is not null)
        {
            reports = reports.Where(r => r.Visit!.RestaurantId == restaurantId);
        }
        return reports;
    }
}
