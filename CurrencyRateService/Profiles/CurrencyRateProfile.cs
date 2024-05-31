using AutoMapper;
using CurrencyRateService.DTOs;
using CurrencyRateService.Models;

namespace CurrencyRateService.Profiles;

public class CurrencyRateProfile : Profile
{
    public CurrencyRateProfile()
    {
        CreateMap<CurrencyRateWriteDTO, CurrencyRate>();
        CreateMap<CurrencyRate, CurrencyRateReadDTO>();
        CreateMap<CurrencyRate, GrpcCurrencyRateModel>()
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.CurrencyName))
            .ForMember(dest => dest.RateToUsd, opt => opt.MapFrom(src => src.RateToUSD.ToString()))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
    }
}