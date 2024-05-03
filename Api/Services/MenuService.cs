using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services
{
    public class MenuService(ApiDbContext context)
    {
        public async Task<Result<bool>> DeleteMenuAsync(int id, User user)
        {
            var errors = new List<ValidationResult>();
            var Menu = await context.Menus.Include(m => m.Restaurant)
                .ThenInclude(r => r.Group)
                .FirstOrDefaultAsync();

            if (Menu == null)
            {
                errors.Add(new ValidationResult("Menu not found"));
                return errors;
            }

            if (Menu.Restaurant.Group.OwnerId != user.Id)
            {
                errors.Add(new ValidationResult("Menu is not owned by user"));
                return errors;
            }

            context.Remove(Menu);
            return true;
        }
    }
}
