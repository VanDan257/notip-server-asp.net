using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace notip_server.ChatBot
{
    public class ChatBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text.ToLower();

            if (text.Contains("bạn là ai?"))
            {
                // Xác định người gửi của tin nhắn
                var sender = turnContext.Activity.From.Name;

                // Tùy thuộc vào người gửi, gửi một câu trả lời phù hợp
                if (sender == "A")
                {
                    await turnContext.SendActivityAsync($"Tên tôi là A", cancellationToken: cancellationToken);
                }
                else if (sender == "B")
                {
                    await turnContext.SendActivityAsync($"Tôi không biết", cancellationToken: cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync($"Xin lỗi, tôi không hiểu bạn đang nói gì", cancellationToken: cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"Xin lỗi, tôi không hiểu câu hỏi của bạn", cancellationToken: cancellationToken);
            }
        }
    }
}
