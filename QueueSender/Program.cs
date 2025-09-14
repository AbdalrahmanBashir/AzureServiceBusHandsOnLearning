
using Azure.Messaging.ServiceBus;
using System.Data;

public class Program
{
    private const string ServiceBusConnectionString = "Endpoint=sb://azure-service-buse-learning.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=zNGNiUmiTrP4rZhs1Gf9USahI+nvU4jmh+ASbK/T9TE=";

    private const string QueueName = "email-requests";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Initializing Service Bus client and sender...");

        await using var client = new ServiceBusClient(ServiceBusConnectionString);

        await using var sender = client.CreateSender(QueueName);

        Console.WriteLine("Creating a message batch...");

        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

        var messagesToSend = new[]
        {
            new ServiceBusMessage("{\"to\": \"Bashir@Bashir.com\", \"type\": \"welcome\", \"name\": \"Bashir\"}"),
            new ServiceBusMessage("{\"to\": \"Abdalrahman@Bashir.com\", \"type\": \"password_reset\", \"link\": \"https://bashir.com/reset/123xyz\"}"),
            new ServiceBusMessage("{\"to\": \"charlie@Bashir.com\", \"type\": \"receipt\", \"orderId\": \"X47-B21\", \"amount\": \"$42.50\"}"),
            new ServiceBusMessage("{\"to\": \"dave@Bashir.com\", \"type\": \"weekly_newsletter\"}")

        };

        foreach (var message in messagesToSend) 
        {
            if (!messageBatch.TryAddMessage(message))
            {
                Console.WriteLine($"Error: The message '{message.Body}'" +
                    $" is too large to fit in the batch.");
                return;
            }
        }

        try
        {
            Console.WriteLine($"Sending a batch of {messageBatch.Count} " +
                $"messages to the queue: {QueueName}");

            await sender.SendMessagesAsync(messageBatch);

            Console.WriteLine("Message sent successfully!");

        }
        catch (Exception ex) 
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        finally
        {
            Console.WriteLine("Sender has been closed and resources have been released.");
        }

    }
}