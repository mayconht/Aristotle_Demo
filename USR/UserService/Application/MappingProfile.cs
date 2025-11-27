using Aristotle.Application.DTOs;
using Aristotle.Domain.Entities;
using AutoMapper;

namespace Aristotle.Application;

/// <summary>
///     AutoMapper profile for mapping between domain entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the MappingProfile class.
    /// </summary>
    public MappingProfile()
    {
        // User mappings (simplified for Keycloak integration)
        CreateMap<User, UserResponseDto>();
        CreateMap<UserUpdateDto, User>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalUserId, opt => opt.Ignore());
    }
}