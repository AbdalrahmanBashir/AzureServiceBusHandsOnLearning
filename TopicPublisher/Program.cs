
using Azure.Messaging.ServiceBus;
using System.Text.Json;

public class Program
{
    private const string ServiceBusConnectionString = "Endpoint=sb://pub-sub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=McN7wxXv/EbavH/OHLrWQ17PILkVOD3S4+ASbKQEHlY=";

    private const string TopicName = "UserRegistered";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Initializing Service Bus client and sender for the topic...");
        await using var client = new ServiceBusClient(ServiceBusConnectionString);
        await using var sender = client.CreateSender(TopicName);

        Console.WriteLine("Creating and sending messages with application properties...");

        var users = new[]
        {
            new {UserId = Guid.NewGuid(), FullName = "Adam Brown", Email = "Adam@bashir.com"},
            new {UserId = Guid.NewGuid(), FullName = "Adam Williams", Email = "Williams@bashir.com"},
            new {UserId = Guid.NewGuid(), FullName = "Williams Brown", Email = "Brown@bashir.com"}
        };

        var messagesToSend = new List<ServiceBusMessage>();
        foreach (var user in users)
        {
            string messageBody = JsonSerializer.Serialize(user);
            var message =  new ServiceBusMessage(messageBody);
            message.ApplicationProperties.Add("EventType", "UserRegistered");

            messagesToSend.Add(message);

            Console.WriteLine($"  - Prepared message for {user.Email}");
        }

        try
        {
            Console.WriteLine("\nSending messages to the topic...");
            foreach (var message in messagesToSend)
            {
                await sender.SendMessageAsync(message);
            }
            Console.WriteLine($"Successfully sent {messagesToSend.Count} messages.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

}