using AutoMapper;
using System.Text.Json;
using Interfaces.Models;

namespace DataLayer.Mappers
{
    public class ProfileAutoMapping : Profile
    {
        public ProfileAutoMapping()
        {
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(dest => dest.AdditionalData, opt => opt.MapFrom(src => getSerialized(src.AdditionalData)));
            CreateMap<UserProfileDto, UserProfile>().ForMember(dest => dest.AdditionalData, opt => opt.MapFrom(src => getDeserialized(src.AdditionalData))); ;
        }

        private string? getSerialized(AdditionalData? additionalData)
        {
            if (additionalData == null)
            {
                return null;
            }
            return JsonSerializer.Serialize(additionalData);
        }

        private AdditionalData? getDeserialized(string additionalData)
        {
            return JsonSerializer.Deserialize<AdditionalData?>(additionalData);
        }
    }
}