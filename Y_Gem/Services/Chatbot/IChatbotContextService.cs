namespace Y_GYM.Services.Chatbot
{
    public interface IChatbotContextService
    {
        Task<string> BuildContextAsync(
            CancellationToken cancellationToken = default);
    }
}