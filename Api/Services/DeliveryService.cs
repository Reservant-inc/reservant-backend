using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Delivery;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;
/// <summary>
/// Service for managing deliveries
/// </summary>
/// <param name="context"></param>
public class DeliveryService(
    ApiDbContext context, 
    ValidationService validationService
    )
{


    public async Task<Result<DeliveryVM>> GetDeliveryAsync(int id)
    {

        var delivery = await context.Deliveries
            .Include(e => e.positions.Item1)
            .FirstOrDefaultAsync(delivery => delivery.Id == id);
        
        if (delivery is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(id),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound
            };
        }
        
        return new DeliveryVM
        {
            Id = delivery.Id,
            positions = delivery.positions
        };
    }




    public async Task<Result<DeliveryVM>> CreateDeliveryAsync(DeliveryVM deliveryVM, User user)
    {
        var delivery = new Delivery()
        {
            positions = deliveryVM.positions
        };


        await validationService.ValidateAsync(delivery, user.Id);
        
        
        await context.Deliveries.AddAsync(delivery);
        await context.SaveChangesAsync();


        return new DeliveryVM()
        {
            Id = delivery.Id,
            positions = delivery.positions
        };
    }
    
    
    
    
}