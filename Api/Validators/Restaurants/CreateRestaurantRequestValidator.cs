using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Restaurants;

/// <summary>
/// Validator for CreateRestaurantRequest
/// </summary>
public class CreateRestaurantRequestValidator : AbstractValidator<CreateRestaurantRequest>
{
    /// <inheritdoc />
    public CreateRestaurantRequestValidator(FileUploadService uploadService, ApiDbContext dbContext)
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(50)
            .IsValidName();

        RuleFor(r => r.MaxReservationDurationMinutes)
            .GreaterThanOrEqualTo(Visit.MinReservationDurationMinutes);

        RuleFor(r => r.Nip)
            .NotNull()
            .Nip();

        RuleFor(r => r.Address)
            .NotEmpty()
            .MaximumLength(70)
            .IsValidAddress();

        RuleFor(r => r.PostalIndex)
            .NotNull()
            .PostalCode();

        RuleFor(r => r.City)
            .NotEmpty()
            .MaximumLength(15)
            .IsValidCity();

        RuleFor(r => r.RentalContract)
            .FileUploadName(FileClass.Document, uploadService);

        RuleFor(r => r.AlcoholLicense)
            .FileUploadName(FileClass.Document, uploadService);

        RuleFor(r => r.BusinessPermission)
            .NotNull()
            .FileUploadName(FileClass.Document, uploadService);

        RuleFor(r => r.IdCard)
            .NotNull()
            .FileUploadName(FileClass.Document, uploadService);

        RuleFor(r => r.Logo)
            .NotNull()
            .FileUploadName(FileClass.Image, uploadService);

        RuleFor(r => r.Description)
            .Length(1, 200);

        RuleForEach(r => r.Tags)
            .RestaurantTag(dbContext);

        RuleForEach(r => r.Photos)
            .FileUploadName(FileClass.Image, uploadService)
            .NotNull();

        RuleFor(r => r.ReservationDeposit)
            .GreaterThan(0)
            .When(r => r.ReservationDeposit.HasValue);
    }
}
