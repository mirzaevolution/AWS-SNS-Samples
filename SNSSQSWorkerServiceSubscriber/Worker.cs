using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using SNSModels;
using Newtonsoft.Json;

namespace SNSSQSWorkerServiceSubscriber
{
    public class Worker : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;
        public Worker(
            IAmazonSQS sqsClient,
            IConfiguration configuration,
            ILogger<Worker> logger)
        {
            _sqsClient = sqsClient;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _configuration["AWS:QueueUrl"]
                });
                if(response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.Messages.Count>0)
                {
                    _logger.LogInformation($"Received a message....");
                    foreach(Message message in response.Messages)
                    {
                        try
                        {
                            var snsBodyMessage = Amazon.SimpleNotificationService.Util.Message.ParseMessage(message.Body);
                            EventMessage eventMessage = JsonConvert.DeserializeObject<EventMessage>(snsBodyMessage.MessageText);
                            Console.WriteLine($"       Id: {eventMessage.Id}\n" +
                                              $"  Message: {eventMessage.Message}\n" +
                                              $"Timestamp: {eventMessage.TimestampUtc}");
                            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                QueueUrl = _configuration["AWS:QueueUrl"],
                                ReceiptHandle = message.ReceiptHandle
                            });
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
