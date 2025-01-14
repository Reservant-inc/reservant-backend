using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Class responsible for creating example visits
/// </summary>
public class VisitSeeder(ApiDbContext context, UserSeeder users)
{
    private const int RandomSeed = 1;

    private Random _random = new(RandomSeed);
    private User[] _availableClients =
    [
        users.Customer1,
        users.Customer2,
        users.Customer3,
        users.JohnDoe,
        users.AnonYmus,
        users.GeraltRiv,
        users.KrzysztofKowalski,
        users.PaulAtreides,
        users.WalterWhite,
    ];

    private User[] GetRandomParticipants()
    {
        return _random.GetItems(_availableClients, _random.Next(3));
    }

    private static User? GetHallEmployee(Restaurant restaurant)
    {
        return restaurant.Employments
            .Where(e => e.IsHallEmployee)
            .Select(e => e.Employee)
            .FirstOrDefault();
    }

    private Order CreateRandomOrder(Visit visit)
    {
        var orderedItems = _random.GetItems(
            visit.Restaurant.MenuItems.ToArray(), _random.Next(1, 5))
            .Distinct();
        var employee = GetHallEmployee(visit.Restaurant);

        return new Order
        {
            VisitId = visit.VisitId,
            Visit = visit,
            OrderItems = orderedItems.Select(item =>
                new OrderItem
                {
                    Amount = _random.Next(1, 3),
                    MenuItem = item,
                    OneItemPrice = item.Price,
                    Status = OrderStatus.Taken,
                })
                .ToList(),
            AssignedEmployee = employee,
            PaymentTime = visit.Reservation?.ReservationDate,
        };
    }

    private async Task<Visit> CreateRandomVisit(User client, Restaurant restaurant, DateTime reservationDate, bool isPast)
    {
        var alignedReservationDate = new DateTime(
            DateOnly.FromDateTime(reservationDate),
            new TimeOnly(reservationDate.Hour, reservationDate.Minute % 30));
        var visitDate = alignedReservationDate.AddDays(_random.Next(3));

        var visit = new Visit
        {
            Restaurant = restaurant,
            NumberOfGuests = _random.Next(2),
            Creator = client,
            Participants = GetRandomParticipants(),
            Reservation = new Reservation
            {
                StartTime = visitDate,
                EndTime = visitDate.AddMinutes(30 * _random.Next(1, 4)),
                Deposit = restaurant.ReservationDeposit,
                ReservationDate = alignedReservationDate,
            },
            Takeaway = false,
            Tip = null,
        };

        var numberOfPeople = visit.NumberOfGuests + visit.Participants.Count + 1;
        var table = await context.Entry(restaurant)
            .Collection(r => r.Tables)
            .Query()
            .AsNoTracking()
            .Where(t => t.Capacity >= numberOfPeople)
            .Where(t => !context.Visits.Any(v =>
                v.TableId == t.Number &&
                v.Reservation!.StartTime < visit.Reservation.EndTime &&
                v.Reservation!.EndTime > visit.Reservation.StartTime))
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException("Unable to find a table big enough");
        visit.TableId = table.Number;

        if (isPast)
        {
            visit.StartTime = visitDate.AddMinutes(_random.Next(15));
            visit.EndTime = visit.Reservation.EndTime.AddMinutes(_random.Next(15));

            visit.Orders = new List<Order>();
            for (var i = 0; i < _random.Next(1, 3); i++)
            {
                visit.Orders.Add(CreateRandomOrder(visit));
            }

            visit.Reservation.Decision = new RestaurantDecision
            {
                AnsweredBy = GetHallEmployee(restaurant) ??
                             throw new InvalidOperationException("No restaurant employee"),
                IsAccepted = true,
            };
        }

        return visit;
    }

    /// <summary>
    ///
    /// </summary>
    public async Task CreateVisits()
    {
        var johnDoes = await context.Restaurants
            .Include(r => r.MenuItems)
            .Include(r => r.Employments)
            .ThenInclude(r => r.Employee)
            .SingleAsync(r => r.Name == "John Doe's");
        var kowalskis = await context.Restaurants
            .Include(r => r.MenuItems)
            .Include(r => r.Employments)
            .ThenInclude(r => r.Employee)
            .SingleAsync(r => r.Name == "Kowalski's");

        var now = DateTime.UtcNow;
        var visits = new []
        {
            await CreateRandomVisit(users.KrzysztofKowalski, johnDoes, now.AddDays(-30), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-28), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-25), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-25).AddHours(2), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-22), true),
            await CreateRandomVisit(users.KrzysztofKowalski, johnDoes, now.AddDays(-22), true),
            await CreateRandomVisit(users.KrzysztofKowalski, johnDoes, now.AddDays(-6), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-4), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-2).AddHours(-8), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-2).AddHours(-5), true),
            await CreateRandomVisit(users.KrzysztofKowalski, johnDoes, now.AddDays(-2).AddHours(-3), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddDays(-2).AddHours(-1), true),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddHours(-23), false),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddHours(-8), false),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddHours(-5), false),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now.AddHours(-1), false),
            await CreateRandomVisit(users.JohnDoe, kowalskis, now, false),
        };

        context.Visits.AddRange(visits);
        context.Events.AddRange(await CreateEvents(visits));

        await context.SaveChangesAsync();
    }

    private async Task<FileUpload> RequireFileUpload(string fileName, User owner)
    {
        var upload = await context.FileUploads.FirstOrDefaultAsync(x => x.FileName == fileName) ??
                     throw new InvalidDataException($"Upload {fileName} not found");
        if (upload.UserId != owner.Id)
        {
            throw new InvalidDataException($"Upload {fileName} is not owned by {owner.UserName}");
        }

        return upload;
    }

    private async Task<List<Event>> CreateEvents(Visit[] visits)
    {
        var visit0Date = visits[0].Reservation!.ReservationDate!.Value;
        var visit1Date = visits[1].Reservation!.ReservationDate!.Value;

        return
        [
            new Event
            {
                Name = "Posiadówa w John Doe's",
                CreatedAt = visit0Date.AddDays(-1),
                Description = "Event 1 Description",
                Time = visit0Date,
                MustJoinUntil = visit0Date.AddHours(-3),
                MaxPeople = 13,
                Creator = users.JohnDoe,
                RestaurantId = 1,
                Visit = visits[0],
                ParticipationRequests =
                [
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = visit0Date.AddHours(-5),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = visit0Date.AddHours(-5),
                        DateAccepted = visit0Date.AddHours(-4),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer3,
                        DateSent = visit0Date.AddHours(-5),
                        DateDeleted = visit0Date.AddHours(-4),
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe),
            },
            new Event
            {
                Name = "Posiadówa w John Doe's vol. 2",
                CreatedAt = visit1Date.AddDays(-5),
                Description = "Event 2 Description",
                Time = visit1Date,
                MustJoinUntil = visit1Date.AddDays(-1),
                MaxPeople = 10,
                Creator = users.JohnDoe,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests =
                [
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = visit1Date.AddDays(-3),
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside5.jpg", users.JohnDoe),
            },
            new Event
            {
                Name = "Przyszłe Wydarzenie",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Description = "Event 3 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(10),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(10).AddHours(-1),
                MaxPeople = 5,
                Creator = users.AnonYmus,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests =
                [
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = DateTime.UtcNow,
                    },
                    new ParticipationRequest
                    {
                        User = users.JohnDoe,
                        DateSent = DateTime.UtcNow,
                        DateAccepted = DateTime.UtcNow,
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside1.jpg", users.AnonYmus),
            },
            new Event
            {
                Name = "Event 4",
                CreatedAt = DateTime.UtcNow,
                Description = "Event 4 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(15),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(15).AddHours(-1),
                MaxPeople = 20,
                Creator = users.AnonYmus,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests = [],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside2.jpg", users.AnonYmus),
            },
            new Event
            {
                Name = "Wydarzenie 5",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Description = "Event 5 Description",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(20),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(20).AddHours(-1),
                MaxPeople = 20,
                Creator = users.GeraltRiv,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests =
                [
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = DateTime.UtcNow.AddHours(-1),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = DateTime.UtcNow,
                    },
                    new ParticipationRequest
                    {
                        User = users.JohnDoe,
                        DateSent = DateTime.UtcNow,
                    },
                ],
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside4.jpg", users.GeraltRiv),
            },
            new Event
            {
                Name = "Impreza w przyszłości 1",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Description = "Opis wydarzenia 1",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(10),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(10).AddHours(-3),
                MaxPeople = 15,
                Creator = users.GeraltRiv,
                RestaurantId = 2,
                VisitId = null,
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside3.jpg", users.GeraltRiv),
            },
            new Event
            {
                Name = "Impreza w przyszłości 2",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Description = "Opis wydarzenia 2",
                Time = DateTime.UtcNow.AddMonths(2).AddDays(5),
                MustJoinUntil = DateTime.UtcNow.AddMonths(2).AddDays(5).AddHours(-6),
                MaxPeople = 8,
                Creator = users.KrzysztofKowalski,
                RestaurantId = 3,
                VisitId = null,
                ParticipationRequests = new List<ParticipationRequest>
                {
                    new ParticipationRequest
                    {
                        User = users.Customer3,
                        DateSent = DateTime.UtcNow.AddDays(-1),
                    },
                    new ParticipationRequest
                    {
                        User = users.JohnDoe,
                        DateSent = DateTime.UtcNow.AddDays(-1),
                        DateAccepted = DateTime.UtcNow.AddHours(-3),
                    }
                },
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside6.jpg", users.KrzysztofKowalski),
            },
            new Event
            {
                Name = "Spotkanie przy kawie",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                Description = "Opis wydarzenia 3",
                Time = DateTime.UtcNow.AddMonths(3),
                MustJoinUntil = DateTime.UtcNow.AddMonths(3).AddDays(-2),
                MaxPeople = 5,
                Creator = users.KrzysztofKowalski,
                RestaurantId = 4,
                VisitId = null,
                ParticipationRequests = new List<ParticipationRequest>
                {
                    new ParticipationRequest
                    {
                        User = users.Customer1,
                        DateSent = DateTime.UtcNow.AddDays(-22),
                    }
                },
                PhotoFileName = null!,
                Photo = await RequireFileUpload("menu.png", users.KrzysztofKowalski),
            },
            new Event
            {
                Name = "Wieczór Włoskich Przysmaków",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Description = "Przyjdź i poczuj atmosferę małej włoskiej trattorii! W menu: świeże makarony, chrupiąca pizza i domowe tiramisu. Wieczór urozmaici muzyka na żywo.",
                Time = DateTime.UtcNow.AddMonths(1).AddDays(15),
                MustJoinUntil = DateTime.UtcNow.AddMonths(1).AddDays(14),
                MaxPeople = 30,
                Creator = users.PaulAtreides,
                RestaurantId = 5,
                VisitId = null,
                ParticipationRequests = new List<ParticipationRequest>(),
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside7.jpg", users.PaulAtreides),
            },
            new Event
            {
                Name = "Piknik rodzinny",
                CreatedAt = DateTime.UtcNow.AddDays(-35),
                Description = "Opis wydarzenia 5",
                Time = DateTime.UtcNow.AddMonths(3).AddDays(10),
                MustJoinUntil = DateTime.UtcNow.AddMonths(3).AddDays(9),
                MaxPeople = 25,
                Creator = users.PaulAtreides,
                RestaurantId = 1,
                VisitId = null,
                ParticipationRequests = new List<ParticipationRequest>
                {
                    new ParticipationRequest
                    {
                        User = users.Customer2,
                        DateSent = DateTime.UtcNow.AddDays(-33),
                    },
                    new ParticipationRequest
                    {
                        User = users.Customer3,
                        DateSent = DateTime.UtcNow.AddDays(-34),
                    }
                },
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResLogo5.png", users.PaulAtreides),
            },
            new Event
            {
                Name = "Impreza sylwestrowa",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                Description = "Spotkajmy się na leniwy brunch z rodziną i przyjaciółmi. Bufet pełen świeżych potraw, kącik dla dzieci oraz muzyka w tle sprawią, że to będzie wyjątkowy dzień.",
                Time = new DateTime(DateTime.UtcNow.Year, 12, 31, 20, 0, 0),
                MustJoinUntil = new DateTime(DateTime.UtcNow.Year, 12, 31, 18, 0, 0),
                MaxPeople = 50,
                Creator = users.GeraltRiv,
                RestaurantId = 2,
                VisitId = null,
                ParticipationRequests = new List<ParticipationRequest>
                {
                    new ParticipationRequest
                    {
                        User = users.Customer3,
                        DateSent = DateTime.UtcNow.AddDays(-55),
                    }
                },
                PhotoFileName = null!,
                Photo = await RequireFileUpload("ResInside3.jpg", users.GeraltRiv),
            },
        ];
    }
}
