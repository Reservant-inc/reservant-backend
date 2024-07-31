using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Delivery;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Validation;

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

    public async Task<Result<DeliveryVM>> createDeliveryAsync(DeliveryVM deliveryVM, User user)
    {
        // validators? restaurants?
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