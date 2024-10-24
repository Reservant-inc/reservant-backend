using AutoMapper;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Reviews;

/// <summary>
/// Mapping profile for review DTOs
/// </summary>
public class ReviewMappings : Profile
{
    /// <inheritdoc />
    public ReviewMappings()
    {
        CreateMap<Review, ReviewVM>();
    }
}
