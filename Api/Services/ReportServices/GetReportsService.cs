using AutoMapper;
using AutoMapper.QueryableExtensions;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// service for managing user reports
/// </summary>
public class GetReportsService(
    IMapper mapper,
    ApiDbContext context,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Function for getting the reports as Customer Support
    /// </summary>
    /// <param name="dateFrom">Starting date to look for reports</param>
    /// <param name="dateUntil">Ending date to look for reports</param>
    /// <param name="category">category of the reports to look for</param>
    /// <param name="reportedUserId">id of the user that was reported in the reports</param>
    /// <param name="restaurantId">id of the restaurant that the reported visit took place in</param>
    /// <param name="createdById">id of the user who created the report</param>
    /// <param name="status">status of the report considered in the search</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>list of reports that match given parameters</returns>
    public async Task<Result<Pagination<ReportVM>>> GetReportsAsync(
        DateTime? dateFrom,
        DateTime? dateUntil,
        ReportCategory? category,
        Guid? reportedUserId,
        int? restaurantId,
        Guid? createdById,
        ReportStatus status,
        int page = 0,
        int perPage = 10)
    {
        IQueryable<Report> reports = context.Reports;

        reports = FilterReportsQuery(reports, dateFrom, dateUntil, category, reportedUserId, restaurantId, createdById, status);
        return await reports
            .ProjectTo<ReportVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, []);
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
    /// <param name="status">status of the report considered in the search</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>list of reports that match given parameters</returns>
    public async Task<Result<Pagination<ReportVM>>> GetMyReportsAsync(
        User user,
        DateTime? dateFrom,
        DateTime? dateUntil,
        ReportCategory? category,
        Guid? reportedUserId,
        int? restaurantId,
        ReportStatus status,
        int page = 0,
        int perPage = 10)
    {
        IQueryable<Report> reports = context.Reports;

        reports = FilterReportsQuery(reports, dateFrom, dateUntil, category, reportedUserId, restaurantId, user.Id, status);
        return await reports
            .ProjectTo<ReportVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, []);
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
    /// <param name="status">status of the report considered in the search</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>list of reports that match given parameters</returns>
    public async Task<Result<Pagination<ReportVM>>> GetMyRestaurantsReportsAsync(
        User user,
        DateTime? dateFrom,
        DateTime? dateUntil,
        ReportCategory? category,
        Guid? reportedUserId,
        int restaurantId,
        ReportStatus status,
        int page = 0,
        int perPage = 10)
    {
        var isOwner = await authorizationService.VerifyOwnerRole(restaurantId, user.Id);
        if (isOwner.IsError) return isOwner.Errors;

        var reports = FilterReportsQuery(context.Reports, dateFrom, dateUntil, category, reportedUserId, restaurantId, user.Id, status);
        return await reports
            .ProjectTo<ReportVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, []);
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
    /// <param name="createdById"></param>
    /// <param name="status">status of the report considered in the search</param>
    /// <returns></returns>
    private static IQueryable<Report> FilterReportsQuery(IQueryable<Report> reports,
        DateTime? dateFrom,
        DateTime? dateUntil,
        ReportCategory? category,
        Guid? reportedUserId,
        int? restaurantId,
        Guid? createdById,
        ReportStatus status)
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
            reports = reports.Where(r => r.Category == category);
        }
        if (reportedUserId is not null)
        {
            reports = reports.Where(r => r.ReportedUserId == reportedUserId);
        }
        if (restaurantId is not null)
        {
            reports = reports.Where(r => r.Visit!.RestaurantId == restaurantId);
        }
        if (createdById is not null)
        {
            reports = reports.Where(r => r.CreatedById == createdById);
        }
        switch (status)
        {
            case ReportStatus.Resolved:
                reports = reports.Where(r => r.Resolution != null);
                break;
            case ReportStatus.NotResolved:
                reports = reports.Where(r => r.Resolution == null);
                break;
            case ReportStatus.All:
                break;
        }

        return reports.OrderByDescending(report => report.ReportDate);
    }

}
