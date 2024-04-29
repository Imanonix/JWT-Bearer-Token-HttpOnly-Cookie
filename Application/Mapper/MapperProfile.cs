using AutoMapper;
using Application.DTOs;
using Domain.Models;


namespace Application.Mapper
{
    public class MapperProfile:Profile
    {
        public MapperProfile()
        {
            CreateMap<User, RegisterDTO>().ReverseMap();
        }
    }
}
