using AutoMapper;
using Dialogs.Domain.Entities;
using Dialogs.Domain.Aggregates;
using Dialogs.Application.Dialogs.DTO;

namespace Dialogs.Application.Mapping;
public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<Dialog, AgentDTO>();
        CreateMap<DialogMessage, DialogMessageDTO>();
    }
}