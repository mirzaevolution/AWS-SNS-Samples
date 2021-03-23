using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService.Util;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using SNSModels;

namespace SNSWebhookSubscriber.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SNSController : ControllerBase
    {
        private readonly string _headerSection = "x-amz-sns-message-type";
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger<SNSController> _logger;
        public SNSController(
                IAmazonSimpleNotificationService snsClient,
                ILogger<SNSController> logger
            )
        {
            _snsClient = snsClient;
            _logger = logger;
        }
        [HttpPost(nameof(Webhook))]
        public async Task<IActionResult> Webhook()
        {
            string payload = await new StreamReader(Request.Body).ReadToEndAsync();
            Message confirmMessage = Message.ParseMessage(payload.ToString());
            if(Request.Headers.TryGetValue(_headerSection, out StringValues value))
            {
                switch (value.ToString())
                {
                    case Message.MESSAGE_TYPE_SUBSCRIPTION_CONFIRMATION:
                        {
                            _logger.LogInformation("****AWS SNS Webhook Confirmation****");
                            _logger.LogInformation($"Confirming subscription with Token: {confirmMessage.Token} and TopicArn: {confirmMessage.TopicArn}");
                            var result = await _snsClient.ConfirmSubscriptionAsync(new ConfirmSubscriptionRequest
                            {
                                Token = confirmMessage.Token,
                                TopicArn = confirmMessage.TopicArn
                            });
                            _logger.LogInformation($"Confirmation result: {(int)result.HttpStatusCode}");
                            if (result.HttpStatusCode == HttpStatusCode.OK)
                                return Ok();
                            return BadRequest();
                        }
                    case Message.MESSAGE_TYPE_NOTIFICATION:
                        {
                            _logger.LogInformation($"****Receiving webhook event from AWS SNS****");
                            _logger.LogInformation("\nFull request payload:");
                            _logger.LogInformation(JsonConvert.SerializeObject(confirmMessage,Formatting.Indented));
                            EventMessage eventMessage = JsonConvert.DeserializeObject<EventMessage>(confirmMessage.MessageText);
                            if (eventMessage != null)
                            {
                                Console.WriteLine("\nCore payload:");
                                Console.WriteLine($">>>      Event Id: {eventMessage.Id}");
                                Console.WriteLine($">>>       Message: {eventMessage.Message}");
                                Console.WriteLine($">>> Timestamp Utc: {eventMessage.TimestampUtc}");
                            }
                            return Ok();
                        }
                    case Message.MESSAGE_TYPE_UNSUBSCRIPTION_CONFIRMATION:
                        {
                            //to-be implemented!
                            break;
                        }

                }
            }
            _logger.LogError($"Header `{_headerSection}` not found");
            return BadRequest(new
            {
                message = $"Header `{_headerSection}` not found"
            });
        }
    }
}
