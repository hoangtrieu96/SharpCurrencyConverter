using AutoMapper;
using ConverterService.DTOs;
using CurrencyRateService;
using CurrencyRateService.Models;

namespace ConverterService.Profiles;

public class ConverterProfile : Profile
{
    public ConverterProfile()
    {
        // Source -> Target
        CreateMap<CurrencyRate, CurrencyRateReadDTO>();
        CreateMap<GrpcCurrencyRateModel, CurrencyRate>()
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.CurrencyName))
            .ForMember(dest => dest.RateToUSD, opt => opt.MapFrom(src => src.RateToUsd))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
    }
}