namespace Y_GYM.Services.Chatbot
{
    public interface IChatbotService
    {
        Task<string> SendMessageAsync(
            string userMessage,
            CancellationToken cancellationToken = default);
    }
}