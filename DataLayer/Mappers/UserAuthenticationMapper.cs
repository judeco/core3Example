using AutoMapper;
using Interfaces.Models;

namespace DataLayer.Mappers
{
    public class UserAuthenticationMapper : Profile
    {
        public UserAuthenticationMapper()
        {
            CreateMap<UserAuthentication, UserAuthenticationDto>();
            CreateMap<UserAuthenticationDto, UserAuthentication>();
        }
    }
}