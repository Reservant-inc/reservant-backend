using System.Text;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Data.Seeding;

/// <summary>
/// Example users in the database
/// </summary>
public class UserSeeder
{
    /// <summary>
    /// Example password used by all users
    /// </summary>
    public const string ExamplePassword = "Pa$$w0rd";

    private readonly UserService _service;

    /// <summary>
    /// Example users in the database
    /// </summary>
    private UserSeeder(UserService service)
    {
        _service = service;
    }

    /// <summary>
    /// Example customer support agent
    /// </summary>
    public User CustomerSupportAgent { get; private set; } = null!;

    /// <summary>
    /// Example customer support manager
    /// </summary>
    public User CustomerSupportManager { get; private set; } = null!;

    /// <summary>
    /// John Doe
    /// </summary>
    public User JohnDoe { get; private set; } = null!;

    /// <summary>
    /// Anon Ymus
    /// </summary>
    public User AnonYmus { get; private set; } = null!;

    /// <summary>
    /// Walter White
    /// </summary>
    public User WalterWhite { get; private set; } = null!;

    /// <summary>
    /// Geralt Riv
    /// </summary>
    public User GeraltRiv { get; private set; } = null!;

    /// <summary>
    /// Paul Atreides
    /// </summary>
    public User PaulAtreides { get; private set; } = null!;

    /// <summary>
    /// Krzysztof Kowalski
    /// </summary>
    public User KrzysztofKowalski { get; private set; } = null!;

    /// <summary>
    /// Customer Przykladowski
    /// </summary>
    public User Customer1 { get; private set; } = null!;

    /// <summary>
    /// Ewa Przykladowska
    /// </summary>
    public User Customer2 { get; private set; } = null!;

    /// <summary>
    /// Ewa Przykladowska
    /// </summary>
    public User Customer3 { get; private set; } = null!;

    /// <summary>
    /// Create example users in the database
    /// </summary>
    public static async Task<UserSeeder> CreateUsers(UserService service)
    {
        var users = new UserSeeder(service);

        users.CustomerSupportAgent = await users.CreateSupportAgent(
            "Pracownik BOK", "Przykładowski", "fced96c1-dad9-49ff-a598-05e1c5e433aa",
            "support@mail.com", false);

        users.CustomerSupportManager = await users.CreateSupportAgent(
            "Kierownik BOK", "Przykładowski", "3f97a9d9-21b5-40ae-b178-bfe071de723c",
            "manager@mail.com", true);

        users.JohnDoe = await users.CreateCustomer(
            "JD", "John", "Doe", "e5779baf-5c9b-4638-b9e7-ec285e57b367", true);

        users.AnonYmus = await users.CreateCustomer(
            "AY", "Anon", "Ymus", "28b618d7-2f32-4f0c-823d-e63ffa56e47f", true);

        users.WalterWhite = await users.CreateCustomer(
            "WW", "Walter", "White", "e20eeb3b-563c-480a-8b8c-85b3afac7c66", true);

        users.GeraltRiv = await users.CreateCustomer(
            "GR", "Geralt", "Riv", "5ad4c90f-c52a-4b14-a8e5-e12eecfd4c8c", true);

        users.PaulAtreides = await users.CreateCustomer(
            "PA", "Paul", "Atreides", "f1e788f1-523c-4aa9-b26f-5eb43ce59573", true);

        users.KrzysztofKowalski = await users.CreateCustomer(
            "KK", "Krzysztof", "Kowalski", "558614c5-ba9f-4c1a-ba1c-07b2b67c37e9", true);

        users.Customer1 = await users.CreateCustomer(
            "customer", "Customer", "Przykladowski", "e08ff043-f8d2-45d2-b89c-aec4eb6a1f29", false);

        users.Customer2 = await users.CreateCustomer(
            "customer2", "Ewa", "Przykładowska", "86a24e58-cb06-4db0-a346-f75125722edd", false);

        users.Customer3 = await users.CreateCustomer(
            "customer3", "Kacper", "Testowy", "a79631a0-a3bf-43fa-8fbe-46e5ee697eeb", false);

        users.JohnDoe.IncomingRequests = [
            new FriendRequest
            {
                DateSent = new DateTime(2024, 8, 7, 18, 52, 2),
                DateRead = new DateTime(2024, 8, 7, 20, 30, 0),
                DateAccepted = new DateTime(2024, 8, 7, 20, 30, 19),
                Sender = users.KrzysztofKowalski,
            },
            new FriendRequest
            {
                DateSent = new DateTime(2024, 8, 10, 13, 2, 50),
                DateRead = new DateTime(2024, 8, 11, 10, 14, 8),
                Sender = users.Customer1,
            },
            new FriendRequest
            {
                DateSent = new DateTime(2024, 8, 11, 15, 8, 29),
                Sender = users.Customer3,
            },
        ];

        users.JohnDoe.OutgoingRequests = [
            new FriendRequest
            {
                DateSent = new DateTime(2024, 8, 13, 15, 43, 8),
                DateRead = new DateTime(2024, 8, 13, 16, 20, 9),
                DateAccepted = new DateTime(2024, 8, 13, 16, 20, 53),
                Receiver = users.WalterWhite,
            },
            new FriendRequest
            {
                DateSent = new DateTime(2024, 8, 13, 15, 43, 50),
                DateRead = new DateTime(2024, 8, 14, 12, 3, 2),
                Receiver = users.PaulAtreides,
            },
            new FriendRequest
            {
                DateSent = new DateTime(2024, 8, 13, 15, 44, 16),
                Receiver = users.GeraltRiv,
            },
        ];

        return users;
    }

    /// <summary>
    /// Create example customer
    /// </summary>
    private async Task<User> CreateCustomer(
        string login, string firstName, string lastName, string id, bool isRestaurantOwner)
    {
        var user = (await _service.RegisterCustomerAsync(new RegisterCustomerRequest
        {
            Login = login,
            Email = CreateEmail(firstName, lastName),
            Password = ExamplePassword,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = new PhoneNumber("+48", "123456789"),
            BirthDate =new DateOnly(1990, 2, 3),
        }, Guid.Parse(id), new DateTime(2024, 1, 1))).OrThrow();

        if (isRestaurantOwner) await _service.MakeRestaurantOwnerAsync(user.Id);
        return user;
    }

    /// <summary>
    /// Create example support agent
    /// </summary>
    private async Task<User> CreateSupportAgent(
        string firstName, string lastName, string id, string email, bool isManager)
    {
        return (await _service.RegisterCustomerSupportAgentAsync(new RegisterCustomerSupportAgentRequest
        {
            Email = email,
            Password = ExamplePassword,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = new PhoneNumber("+48", "123456789"),
            IsManager = isManager,
        }, Guid.Parse(id), new DateTime(2024, 1, 1))).OrThrow();
    }

    /// <summary>
    /// Create an email for a user with the given names
    /// </summary>
    private static string CreateEmail(string firstName, string lastName) =>
        $"{firstName}.{lastName}@mail.com";
}
