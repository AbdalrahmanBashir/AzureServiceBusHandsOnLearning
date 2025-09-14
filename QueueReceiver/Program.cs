using Azure.Messaging.ServiceBus;
using System.Data;

public class Program
{
    private const string ServiceBusConnectionString = "Endpoint=sb://azure-service-buse-learning.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=zNGNiUmiTrP4rZhs1Gf9USahI+nvU4jmh+ASbK/T9TE=";

    private const string QueueName = "email-requests";

    private static ServiceBusProcessor? processor;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Initializing Service Bus client and processor...");

        await using var client = new ServiceBusClient(ServiceBusConnectionString);

        processor = client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

        try
        {
            processor.ProcessMessageAsync += MessageHandler;

            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();

            Console.WriteLine("Waiting for messages. Press any key to stop processing.");
            Console.ReadKey();

            Console.WriteLine("\nStopping the receiver...");

            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages.");


        }
        catch (Exception ex)
        {

            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
        finally {

            if (processor != null) { 
              await processor.StopProcessingAsync();
            
            }
        
        }
    }

    static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"Recived message:  { body}");

        await args.CompleteMessageAsync(args.Message);
    }

}