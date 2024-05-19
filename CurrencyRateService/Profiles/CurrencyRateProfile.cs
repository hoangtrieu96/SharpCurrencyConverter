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
    }
}