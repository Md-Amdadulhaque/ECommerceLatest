using E_commerce.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_commerce.Models;

namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {


        private IEmailConsumerService _emailConsumerService;

        public EmailController(IEmailConsumerService emailConsumerService)
        {
            _emailConsumerService = emailConsumerService;
        }


        [HttpPost("")]
        public async Task<IActionResult> SendMailAsync([FromBody] MailRequest mailRequest)
        {
         var send = _emailConsumerService.SendMailAsync(mailRequest);
            return (send ? "Email Send Successfully" : "Email Didn't Send");
        }
    }
}
