namespace Reservant.Api.Services;
using Reservant.Api.Models.Dtos.Visit;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;


/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(ApiDbContext dbContext)
{
    /// <summary>
    /// Gets the visist oof provided id
    /// </summary>
    /// <param name="visitId"></param>
    /// <returns></returns>
    public async Task<VisitSummaryVM?> GetVisitByIdAsync(int visitId)
    {
        var visit = await dbContext.Visits
            .Where(x => x.Id == visitId)
            .FirstOrDefaultAsync();

        if (visit == null)
        {
            return null;
        }

        var result = new VisitSummaryVM
        {
            Id = visit.Id,
            Date = visit.Date,
            NumberOfPeople = visit.NumberOfGuests + (visit.Participants?.Count ?? 0) + 1,
            Takeaway = visit.Takeaway,
            ClientId = visit.ClientId,
            RestaurantId = visit.RestaurantId
        };

        return result;
    }
}
