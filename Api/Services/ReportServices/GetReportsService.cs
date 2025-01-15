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
    /// <param name="assignedToId">Search only for reports that are assigned to the agent with the given ID</param>
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
        Guid? assignedToId,
        ReportStatus status,
        int page = 0,
        int perPage = 10)
    {
        IQueryable<Report> reports = context.Reports;

        reports = FilterReportsQuery(
            reports, dateFrom, dateUntil,
            category, reportedUserId, restaurantId,
            createdById, assignedToId, status);
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
    /// <param name="assignedToId">Search only for reports that are assigned to the agent with the given ID</param>
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
        Guid? assignedToId,
        ReportStatus status,
        int page = 0,
        int perPage = 10)
    {
        IQueryable<Report> reports = context.Reports;

        reports = FilterReportsQuery(
            reports, dateFrom, dateUntil,
            category, reportedUserId, restaurantId,
            user.Id, assignedToId, status);
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
    /// <param name="createdById">id of the user who created the report</param>
    /// <param name="assignedToId">Search only for reports that are assigned to the agent with the given ID</param>
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
        Guid? createdById,
        Guid? assignedToId,
        ReportStatus status,
        int page = 0,
        int perPage = 10)
    {
        var isOwner = await authorizationService.VerifyOwnerRole(restaurantId, user.Id);
        if (isOwner.IsError) return isOwner.Errors;

        var reports = FilterReportsQuery(
            context.Reports, dateFrom, dateUntil,
            category, reportedUserId, restaurantId,
            createdById, assignedToId, status);
        return await reports
            .ProjectTo<ReportVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, []);
    }


    /// <summary>
    /// Filters query through optional parameters
    /// </summary>
    private static IQueryable<Report> FilterReportsQuery(IQueryable<Report> reports,
        DateTime? dateFrom,
        DateTime? dateUntil,
        ReportCategory? category,
        Guid? reportedUserId,
        int? restaurantId,
        Guid? createdById,
        Guid? assignedToId,
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

        if (assignedToId is not null)
        {
            reports = reports.Where(r =>
                r.AssignedAgents.Any(
                    ra => ra.Until == null && ra.AgentId == assignedToId)
                || r.Resolution != null && r.Resolution.ResolvedBy.Id == assignedToId);
        }

        switch (status)
        {
            case ReportStatus.ResolvedPositively:
                reports = reports.Where(r => r.Resolution != null && r.Resolution.IsDecisionPositive);
                break;
            case ReportStatus.ResolvedNegatively:
                reports = reports.Where(r => r.Resolution != null && !r.Resolution.IsDecisionPositive);
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
