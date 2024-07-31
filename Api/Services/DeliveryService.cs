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
    ValidationService validationService,
    FileUploadService uploadService
    )
{


    public async Task<Result<DeliveryVM>> GetDeliveryAsync(int id)
    {

        var delivery = await context.Deliveries
            .Include(e => e.Positions)
            .ThenInclude(deliveryPosition => deliveryPosition.MenuItem)
            .ThenInclude(menuItem => menuItem.Photo)
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
        
        var deliveryVM = new DeliveryVM
        {
            Id = delivery.Id,
            Positions = delivery.Positions.Select(p => new DeliveryPositionVM
            {
                Id = p.Id,
                MenuItem = new MenuItemVM
                {
                    MenuItemId = p.MenuItem.Id,
                    AlcoholPercentage = p.MenuItem.AlcoholPercentage,
                    AlternateName = p.MenuItem.AlternateName,
                    Name = p.MenuItem.Name,
                    Photo = uploadService.GetPathForFileName(p.MenuItem.PhotoFileName),
                    Price = p.MenuItem.Price
                },
                Quantity = p.Quantity
            }).ToList()
        };
        
        return deliveryVM;
    }




    public async Task<Result<DeliveryVM>> CreateDeliveryAsync(CreateDeliveryRequest deliveryVM, User user)
    {
        

        var menuItemIds = deliveryVM.Positions.Select(p => p.MenuItem.MenuItemId).ToList();

        var menuItems = await context.MenuItems
            .Where(mi => menuItemIds.Contains(mi.Id))
            .ToDictionaryAsync(mi => mi.Id);

        if (menuItems.Count != menuItemIds.Count)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound
            };
        }
        
        var delivery = new Delivery
        {
            Positions = deliveryVM.Positions.Select(p => new DeliveryPosition
            {
                MenuItem = menuItems[p.MenuItem.MenuItemId],
                Quantity = p.Quantity
            }).ToList()
        };

        
        var validationResult = await validationService.ValidateAsync(delivery, user.Id);
        
        if (!validationResult.IsValid)
        {
            return validationResult;
        }


        await context.Deliveries.AddAsync(delivery);
        await context.SaveChangesAsync();


        var createdDeliveryVM = new DeliveryVM
        {
            Id = delivery.Id,
            Positions = delivery.Positions.Select(p => new DeliveryPositionVM
            {
                Id = p.Id,
                MenuItem = new MenuItemVM
                {
                    MenuItemId = p.MenuItem.Id,
                    AlcoholPercentage = p.MenuItem.AlcoholPercentage,
                    AlternateName = p.MenuItem.AlternateName,
                    Name = p.MenuItem.Name,
                    Photo = uploadService.GetPathForFileName(p.MenuItem.PhotoFileName),
                    Price = p.MenuItem.Price
                },
                Quantity = p.Quantity
            }).ToList()
        };

        return createdDeliveryVM;
    }
    
    
    
    
}