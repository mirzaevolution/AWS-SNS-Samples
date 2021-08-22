using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService.Util;
using Microsoft.Extensions.Logging;
using System.IO;

namespace SNSWebhookHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SNSHandlerController : ControllerBase
    {
        private readonly IAmazonSimpleNotificationService _snsService;
        private readonly ILogger<SNSHandlerController> _logger;
        public SNSHandlerController(
                IAmazonSimpleNotificationService snsService,
                ILogger<SNSHandlerController> logger
            )
        {
            _snsService = snsService;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {

                //this is still error!!


                string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
                Message messagePayload = Message.ParseMessage(requestBody);
                switch (messagePayload.Type)
                {
                    case Message.MESSAGE_TYPE_SUBSCRIPTION_CONFIRMATION:
                        {
                            var response = await _snsService.ConfirmSubscriptionAsync(new ConfirmSubscriptionRequest
                            {
                                Token = messagePayload.Token,
                                TopicArn = messagePayload.TopicArn
                            });
                            return StatusCode((int)response.HttpStatusCode);
                        }
                    case Message.MESSAGE_TYPE_NOTIFICATION:
                        {
                            _logger.LogInformation($"[Subject]: {messagePayload.Subject}, [Message]: {messagePayload.MessageText}");
                            return Ok();
                        }
                    default:
                        {

                            return BadRequest("Invalid payload");
                        }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500);
            }
        }
    }
}
