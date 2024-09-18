using AutoMapper;
using Dialogs.Domain.Entities;
using Dialogs.Infrastructure.Snapshots;

namespace Dialogs.Infrastructure.Mapping;

public class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        CreateMap<UserSnapshot, User>();
    }
}