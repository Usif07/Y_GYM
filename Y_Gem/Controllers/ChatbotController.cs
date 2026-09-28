using Microsoft.AspNetCore.Mvc;
using Y_GYM.Services.Chatbot;

namespace Y_GYM.Controllers
{
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("SendMessage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(
            [FromBody] ChatbotMessageRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "من فضلك اكتب رسالة أولاً."
                });
            }

            if (request.Message.Length > 1000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "الرسالة طويلة جدًا. الحد الأقصى 1000 حرف."
                });
            }

            try
            {
                var reply =
                    await _chatbotService.SendMessageAsync(
                        request.Message.Trim(),
                        cancellationToken);

                return Json(new
                {
                    success = true,
                    reply
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء معالجة الرسالة. حاول مرة أخرى."
                });
            }
        }
    }

    public class ChatbotMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }

}
