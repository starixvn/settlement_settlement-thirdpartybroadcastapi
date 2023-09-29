using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using MassTransit;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface ManageQueueService
{
    Task<Result> PublishToWaiting(string productType, string data, int index);

    Task<Result> PublishToProcessing(string productType, string data, int index);
}

public class ManageQueueServiceImp : ManageQueueService
{
    private readonly AppSettingsKey appSettingsKey;

    public ManageQueueServiceImp(IOptions<AppSettingsKey> appSettingsKey)
    {
        this.appSettingsKey = appSettingsKey.Value;
    }

    private IBusControl CreateRabbitMq
    {
        get
        {
            var rabbitMqSetting = appSettingsKey.RabbitMq;

            return Bus.Factory.CreateUsingRabbitMq(configure =>
            {
                configure.Host(rabbitMqSetting.Hosting, rabbitMqSetting.VirtualHost, h =>
                {
                    h.Username(rabbitMqSetting.Username);
                    h.Password(rabbitMqSetting.Password);
                });
            });
        }
    }

    public async Task<Result> PublishToWaiting(string productType, string data, int index)
    {
        var result = new Result();
        var busControl = CreateRabbitMq;

        try
        {
            await busControl.StartAsync();
            await busControl.Publish<IBroadcastApiWaiting>(new
            {
                productType,
                data
            });
        }
        catch (Exception exception)
        {
            result.ErrorCode = 1000;
            result.ErrorMessage = $"Error from broad cast api: Match info {data}\n {exception.Message}";

            await WriteLog(exception, result.ErrorMessage, index);

            return result;
        }
        finally
        {
            await busControl.StopAsync();
        }

        return result;
    }

    public async Task<Result> PublishToProcessing(string productType, string data, int index)
    {
        var result = new Result();
        var busControl = CreateRabbitMq;

        try
        {
            await busControl.StartAsync();
            await busControl.Publish<IBroadcastApiProcessing>(new
            {
                productType,
                data
            });
        }
        catch (Exception exception)
        {
            result.ErrorCode = 1000;
            result.ErrorMessage = $"Error from broad cast api: Match info {data}\n {exception.Message}";
            await WriteLog(exception, result.ErrorMessage, index);

            return result;
        }
        finally
        {
            await busControl.StopAsync();
        }

        return result;
    }

    private async Task WriteLog(Exception exception, string errorMessage, int index)
    {
        if (index == appSettingsKey.RetryCount)
        {
            var logMessage = new LogMessage(errorMessage)
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                .ToString();
            await LogService.ErrorAsync(logMessage);
        }
    }
}