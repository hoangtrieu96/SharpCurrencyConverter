namespace CurrencyRateService.Services.AsyncDataServices;

public interface IMessageQueueProducer
{
    void PublishRateUpdateEvent();
}