using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Endpoints
{
    public static class MessagesEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/chats/{chatId}/messages", GetMessages).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get all chat messages",
            });
        }

        private static async Task<IResult> GetMessages(LingoDbContext dbContext, UserIdHepler userIdHelper, Guid chatId)
        {
            var chat = await dbContext.Chats.Where(ch => ch.ChatId == chatId).Include(ch => ch.Messages).FirstOrDefaultAsync();
            if (chat is null || chat.UserId != userIdHelper.GetUserId())
                return Results.NotFound("Chat not found");
            return Results.Ok(chat.Messages.Select(m => new MessageDto()
            {
                MessageId = m.MessageId,
                Content = m.Content,
                CorrectedContent = m.CorrectedContent,
                Corrections = m.Corrections,
                MessageType = m.MessageType
            }).ToArray());
        }

    }
}
