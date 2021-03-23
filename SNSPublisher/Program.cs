using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using SNSModels;
using Newtonsoft.Json;

namespace SNSPublisher
{
    class Program
    {
        private readonly static IServiceCollection _serviceCollection;
        private readonly static IServiceProvider _serviceProvider;
        private readonly static IConfiguration _configuration;
        static Program()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();
            var options =  _configuration.GetAWSOptions();
            _serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddAWSService<IAmazonSimpleNotificationService>(options);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }
        static async void Publish()
        {
            EventMessage eventMessage = new EventMessage();
            eventMessage.Message = $"Message: {eventMessage.Id}";

            var snsClient = _serviceProvider.GetService<IAmazonSimpleNotificationService>();
            var publishRequest = new PublishRequest
            {
                Subject = $"My Topic {eventMessage.Id}",
                TopicArn = _configuration["AWS:TopicArn"],
                Message = JsonConvert.SerializeObject(eventMessage)
            };
            publishRequest.MessageAttributes.Add("Author", new MessageAttributeValue
            {
                DataType = "String",
                StringValue = "Mirza Ghulam Rasyid"
            });
            var publishResponse = await snsClient.PublishAsync(publishRequest);
            
            if(publishResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Message published");
            }
            else
            {
                Console.WriteLine($"Message failed to publish: {publishResponse.HttpStatusCode}");
            }
        }
        static void Main(string[] args)
        {
            Publish();
            Console.ReadLine();
        }
    }
}
