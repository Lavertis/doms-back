using AutoMapper;
using DoctorsOffice.Application.AutoMapper;

namespace DoctorsOffice.UnitTests;

public class UnitTest
{
    protected readonly IMapper Mapper;

    protected UnitTest()
    {
        Mapper = AutoMapperModule.CreateAutoMapper();
    }
}