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
    /// Customers
    /// </summary>
    public List<User> Customers { get; private set; } = null!;

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

        users.Customers =
        [
            await users.CreateCustomer(
                "customer", "Customer", "Przykladowski", "e08ff043-f8d2-45d2-b89c-aec4eb6a1f29", false),
            await users.CreateCustomer(
                "customer2", "Ewa", "Przykładowska", "86a24e58-cb06-4db0-a346-f75125722edd", false),
            await users.CreateCustomer(
                "customer3", "Kacper", "Testowy", "a79631a0-a3bf-43fa-8fbe-46e5ee697eeb", false),
            await users.CreateCustomer(
                "user4", "Anna", "Nowak", "3a5a6b8c-4543-4cc2-b2cf-d1fc49e6dc6c", false),
            await users.CreateCustomer(
                "randomLogin5", "Piotr", "Kowalski", "f2d3cb45-123b-4e9a-bc6c-9f73c8f6320d", false),
            await users.CreateCustomer(
                "guestUser6", "Joanna", "Zielińska", "6a73b5a9-9cf8-4f63-a0b4-b46c467b456d", false),
            await users.CreateCustomer(
                "tempUser7", "Marek", "Wiśniewski", "9245a3e7-e762-4c5d-bb7c-2f2de5f867f5", false),
            await users.CreateCustomer(
                "user8X", "Katarzyna", "Jankowska", "3c8712fa-68ef-42c4-bec6-986527f7d65a", false),
            await users.CreateCustomer(
                "newUser9", "Tomasz", "Lewandowski", "07cfa59d-bc5e-47a2-9443-2a8b3d26a962", false),
            await users.CreateCustomer(
                "login10", "Barbara", "Kaczmarek", "815a9c2d-b49e-4529-91f2-381eb5f7ca4a", false),
            await users.CreateCustomer(
                "customUser11", "Jakub", "Mazur", "a91c732f-1f5f-44c8-a742-25d5b5f0b5ec", false),
            await users.CreateCustomer(
                "testLogin12", "Magdalena", "Wójcik", "9bf81a73-07b2-4695-99ff-73c685d1b1c9", false),
            await users.CreateCustomer(
                "user13Test", "Jan", "Kamiński", "b57e5b23-9847-4f14-b67e-95e2e6c2fcb3", false),
            await users.CreateCustomer(
                "client14", "Zofia", "Kwiatkowska", "5bce34c1-02a1-4baf-bb4d-91347c3f9a98", false),
            await users.CreateCustomer(
                "account15", "Łukasz", "Kaczmarek", "95cdaf53-8ac9-4b56-ae6b-6f914faffb58", false),
            await users.CreateCustomer(
                "user16", "Alicja", "Dąbrowska", "fbf5785b-7766-47ab-b589-72076a57024d", false),
            await users.CreateCustomer(
                "guest17", "Grzegorz", "Szymański", "64a2b5d3-d727-4699-8325-17b78872aefb", false),
            await users.CreateCustomer(
                "temporary18", "Irena", "Woźniak", "e872e379-3284-4a2f-922e-b0bca60194b7", false),
            await users.CreateCustomer(
                "random19", "Andrzej", "Piotrowski", "d2d4604b-b049-40ed-b618-f55847c43c4c", false),
            await users.CreateCustomer(
                "user20a", "Jadwiga", "Pawlak", "39c6cc79-2cd9-47c2-89b7-b76c100c7976", false),
            await users.CreateCustomer(
                "account21", "Jerzy", "Chmielewski", "5d23c3f5-bf63-42fa-aada-2e48b03a1d76", false),
            await users.CreateCustomer(
                "client22", "Dorota", "Zawisza", "42a5e4e9-70d0-4edb-9a91-ea1737d7e4b0", false),
            await users.CreateCustomer(
                "newUser23", "Kamil", "Szewczyk", "0173b6bc-dba7-45a0-9f16-13a8b90d1770", false),
        ];

        (users.Customer1, users.Customer2, users.Customer3) =
            (users.Customers[0], users.Customers[1], users.Customers[2]);

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
