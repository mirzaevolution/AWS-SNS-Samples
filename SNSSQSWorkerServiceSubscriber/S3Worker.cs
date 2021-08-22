using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;

namespace SNSSQSWorkerServiceSubscriber
{
    public class S3Worker : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<S3Worker> _logger;
        public S3Worker(
            IAmazonSQS sqsClient,
            IConfiguration configuration,
            ILogger<S3Worker> logger)
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
                    QueueUrl = _configuration["AWS:S3SNSQueueUrl"]
                });
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.Messages.Count > 0)
                {
                    foreach (Message message in response.Messages)
                    {
                        try
                        {
                            var snsBodyMessage = Amazon.SimpleNotificationService.Util.Message.ParseMessage(message.Body);
                            Console.WriteLine("\n----------------------------------------------------");
                            Console.WriteLine("****From S3 Core Events****");
                            Console.WriteLine(snsBodyMessage.MessageText);
                            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                QueueUrl = _configuration["AWS:S3SNSQueueUrl"],
                                ReceiptHandle = message.ReceiptHandle
                            });
                        }
                        catch (Exception ex)
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
