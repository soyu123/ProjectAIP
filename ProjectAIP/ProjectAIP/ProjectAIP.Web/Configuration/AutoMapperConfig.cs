using AutoMapper;
using ProjectAIP.BLL.Entities;
using ProjectAIP.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ProjectAIP.Web.Configuration
{
    public static class AutoMapperConfig
    {
        public static IMapperConfigurationExpression Mapping(this IMapperConfigurationExpression configurationExpression, UserManager<User> userManager)
        {
            Mapper.Initialize(cfg =>
            {
                // Maps
                cfg.CreateMap<User, BaseUserVm>()
                    .ForMember(dest => dest.Roles, member => member.MapFrom(src =>
                         userManager.GetRolesAsync(src).Result
                    ));
            });
            return configurationExpression;
        }
    }
}
