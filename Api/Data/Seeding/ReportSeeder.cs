using Microsoft.EntityFrameworkCore;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Class responsible for creating example reports
/// </summary>
public class ReportSeeder(ApiDbContext context, UserSeeder users)
{
    private List<User> _customerSupportAgents = [];
    private int _nextAgentIndex;

    /// <summary>
    /// Create example reports in the database
    /// </summary>
    public async Task CreateReports()
    {
        var now = DateTime.UtcNow;

        _customerSupportAgents = await (
            from user in context.Users
            join userRole in context.UserRoles on user.Id equals userRole.UserId
            join role in context.Roles on userRole.RoleId equals role.Id
            where role.Name == Roles.CustomerSupportAgent
            select user
            ).ToListAsync();

        context.Reports.AddRange([
            CreateTechnicalReport(now.AddMonths(-2), users.KrzysztofKowalski, "Aplikacja nie działa kompletnie"),
            CreateTechnicalReport(now.AddMonths(-1), users.JohnDoe,
                """
                Podczas próby zarezerwowania stolika w restauracji występuje błąd,
                który uniemożliwia dokończenie procesu. Po wypełnieniu wszystkich
                danych (data, liczba osób, godzina) i kliknięciu przycisku "Zarezerwuj",
                pojawia się komunikat: "Wystąpił błąd. Spróbuj ponownie później".

                - Sprawdziłem stabilność mojego połączenia internetowego – działa poprawnie.
                - Problem występuje zarówno przy wyborze różnych restauracji, jak i terminów.
                - Spróbowałem wylogować się i zalogować ponownie, ale to nie pomogło.
                """),
            await CreateLostItemReport(now, 2, "Zgubiłem telefon!"),
            await CreateLostItemReport(now.AddDays(-30).AddHours(-2), 6, "Zgubiłem portfel w restauracji!"),
            await CreateCustomerReport(now.AddDays(-28).AddHours(-5), 7, "Klient był bardzo nieuprzejmy wobec obsługi."),
            await CreateEmployeeReport(now.AddDays(-25).AddHours(-1), 11, "Pracownik nie stosował się do zasad higieny."),
            await CreateLostItemReport(now.AddDays(-20).AddHours(-3), 6, "Zgubiłem klucze do samochodu w lokalu."),
            await CreateCustomerReport(now.AddDays(-18).AddHours(-6), 7, "Klient głośno krzyczał i przeszkadzał innym."),
            await CreateEmployeeReport(now.AddDays(-15).AddHours(-4), 11, "Pracownik był spóźniony o godzinę."),
            await CreateLostItemReport(now.AddDays(-10).AddHours(-8), 7, "Zostawiłem torbę i jej nie mogę znaleźć."),
            await CreateCustomerReport(now.AddDays(-5).AddHours(-9), 6, "Klient narzekał bez powodu i obrażał personel."),
            await CreateEmployeeReport(now.AddDays(-3).AddHours(-7), 7, "Pracownik był wyjątkowo niegrzeczny wobec klientów."),
            await CreateLostItemReport(now.AddHours(-12), 11, "Zgubiłem swoją kurtkę na miejscu."),
            await CreateEmployeeReport(now.AddHours(-2), 6, "Zachowywał się okropnie"),
            await CreateCustomerReport(now.AddHours(-1), 6, "Zachowywał się okropnie"),
        ]);
        await context.SaveChangesAsync();
    }

    private Report CreateTechnicalReport(DateTime reportDate, User createdBy, string description)
    {
        return new Report
        {
            Category = ReportCategory.Technical,
            Description = description,
            CreatedBy = createdBy,
            ReportDate = reportDate,
            AssignedAgents = [
                new ReportAssignment
                {
                    Agent = GetNextAgent(),
                    From = DateTime.UtcNow,
                },
            ],
        };
    }

    private async Task<Report> CreateLostItemReport(DateTime reportDate, int visitId, string description)
    {
        var visit = await FindVisitWithId(visitId);
        return new Report
        {
            Category = ReportCategory.LostItem,
            Description = description,
            CreatedById = visit.ClientId,
            ReportDate = reportDate,
            Visit = visit,
            AssignedAgents = [
                new ReportAssignment
                {
                    Agent = GetNextAgent(),
                    From = DateTime.UtcNow,
                },
            ],
        };
    }

    private async Task<Report> CreateCustomerReport(DateTime reportDate, int visitId, string description)
    {
        var visit = await FindVisitWithId(visitId);
        return new Report
        {
            Category = ReportCategory.CustomerReport,
            Description = description,
            CreatedBy = await FindEmployeeOfVisitWithId(visitId),
            ReportDate = reportDate,
            Visit = visit,
            ReportedUserId = visit.ClientId,
            AssignedAgents = [
                new ReportAssignment
                {
                    Agent = GetNextAgent(),
                    From = DateTime.UtcNow,
                },
            ],
        };
    }

    private async Task<Report> CreateEmployeeReport(DateTime reportDate, int visitId, string description)
    {
        var visit = await FindVisitWithId(visitId);
        return new Report
        {
            Category = ReportCategory.RestaurantEmployeeReport,
            Description = description,
            CreatedById = visit.ClientId,
            ReportDate = reportDate,
            Visit = visit,
            ReportedUser = await FindEmployeeOfVisitWithId(visitId),
            AssignedAgents = [
                new ReportAssignment
                {
                    Agent = GetNextAgent(),
                    From = DateTime.UtcNow,
                },
            ],
        };
    }

    private async Task<User> FindEmployeeOfVisitWithId(int visitId)
    {
        return await context.Visits
            .Where(v => v.VisitId == visitId)
            .Select(v => v.Orders.First().AssignedEmployee)
            .SingleAsync()
               ?? throw new InvalidOperationException($"No employee is assigned to visit with ID {visitId}");
    }

    private User GetNextAgent()
    {
        _nextAgentIndex = (_nextAgentIndex + 1) % _customerSupportAgents.Count;
        return _customerSupportAgents[_nextAgentIndex];
    }

    private async Task<Visit> FindVisitWithId(int visitId)
    {
        return await context.Visits.SingleAsync(v => v.VisitId == visitId);
    }
}
