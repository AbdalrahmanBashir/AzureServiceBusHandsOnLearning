using Azure.Messaging.ServiceBus;
using System.Text.Json;

public record User(Guid UserId, string FullName, string Email);
public class Program
{
    private const string ServiceBusConnectionString = "Endpoint=sb://pub-sub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=McN7wxXv/EbavH/OHLrWQ17PILkVOD3S4+ASbKQEHlY=";

    private const string TopicName = "UserRegistered";

    private const string SubscriptionName = "EmailServiceSubscription";

    static async Task Main(string[] args)
    {
        Console.WriteLine($"Initializing Email Service Consumer for subscription '{SubscriptionName}'...");
        await using var client = new ServiceBusClient(ServiceBusConnectionString);

        ServiceBusProcessor processor = client.CreateProcessor(TopicName, SubscriptionName, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += MessageHandler;

        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync();

        Console.WriteLine("Processor started. Listening for user registration events. Press any key to stop.");
        Console.ReadKey();

        Console.WriteLine("\nStopping the message processor...");
        await processor.StopProcessingAsync();
        Console.WriteLine("Processor stopped.");
    }

    static  Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {args.Exception.Message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }

    private static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"[Received raw message]: {body}");

        try
        {

            var user = JsonSerializer.Deserialize<User>(body);

            if (user != null)
            {

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Email Service] | Sending welcome email to {user.FullName} at {user.Email}...");
                Console.ResetColor();
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException jsonEx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error deserializing message: {jsonEx.Message}");
            Console.ResetColor();
        }
    }
}