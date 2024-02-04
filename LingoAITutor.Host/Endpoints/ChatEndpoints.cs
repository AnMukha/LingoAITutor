﻿using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LingoAITutor.Host.Endpoints
{
    public static class ChatEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/chats", GetChats).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get user chats list",
            });
            application.MapPost("api/chats", CreateChat).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Create chat",
            });
            application.MapDelete("api/chats/{chatId}", DeleteChat).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Delete chat",
            });
            application.MapPost("api/chats/{chatId}/submitMessage", SubmitMessage).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Submit message",
            });
            application.MapPut("api/chats/{chatId}/progressChat", ProgressChat).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Submit message",
            });
        }

        private static async Task<IResult> GetChats(LingoDbContext dbContext, UserIdHepler userIdHelper)
        {
            var chats = await dbContext.Chats.Where(ch => ch.UserId == userIdHelper.GetUserId()).OrderByDescending(ch => ch.Number).ToArrayAsync();
            return Results.Ok(chats.Select(ch => new ChatDto() 
            {
                ChatId = ch.ChatId,
                ChatType = ch.ChatType,
                Title = ch.Title                
            }).ToArray());
        }

        private static async Task<IResult> CreateChat(LingoDbContext dbContext, UserIdHepler userIdHelper, [FromBody] ChatDto chat)
        {
            var newChat = new Chat()
            {
                ChatId = Guid.NewGuid(),
                ChatType = chat.ChatType,
                UserId = userIdHelper.GetUserId(),
                Title = chat.Title                
            };
            await dbContext.AddAsync(newChat);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }

        private static async Task<IResult> SubmitMessage(LingoDbContext dbContext, UserIdHepler userIdHelper, GrammarChecker grammarChecker, 
                                                         Guid chatId, [FromBody] MessageDto message)
        {                      
            var chat = await dbContext.Chats.FirstOrDefaultAsync(m => m.ChatId == chatId);
            if (chat == null)
                return Results.NotFound();
            if (chat.UserId != userIdHelper.GetUserId())
                return Results.Unauthorized();
            var fixedContent = await grammarChecker.FixGrammar(chat, message.Content);
            chat.LastMessageNumber++;
            var newMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = message.Content,
                CorrectedContent = fixedContent,
                Corrections = CorrectionsComposer.ComposeCorrections(message.Content!, fixedContent),
                MessageType = Entities.Enums.MessageType.UserMessage,
                Number = chat.LastMessageNumber
            };
            chat.Messages.Add(newMessage);
            dbContext.Add(newMessage);
            await dbContext.SaveChangesAsync();
            return Results.Ok(new MessageDto()
            {
                Content = newMessage.Content,
                CorrectedContent = fixedContent,
                Corrections = newMessage.Corrections,
                MessageId = newMessage.MessageId,
                MessageType = newMessage.MessageType
            });
        }

        private static async Task<IResult> ProgressChat(LingoDbContext dbContext, UserIdHepler userIdHelper, ChatProgressor chatProgressor, Guid chatId)
        {
            var chat = await dbContext.Chats.FirstOrDefaultAsync(m => m.ChatId == chatId);
            if (chat == null)
                return Results.NotFound();
            if (chat.UserId != userIdHelper.GetUserId())
                return Results.Unauthorized();
            var gptMessage = await chatProgressor.ProgressChat(chatId);            
            if (gptMessage ==null)
                return Results.NotFound();
            return Results.Ok(new MessageDto()
            {
                Content = gptMessage.Content,
                MessageId = gptMessage.MessageId,
                MessageType = Entities.Enums.MessageType.GPTMessage
            });
        }

        private static async Task<IResult> DeleteChat(LingoDbContext dbContext, UserIdHepler userIdHelper, ChatProgressor chatProgressor, Guid chatId)
        {
            var chat = await dbContext.Chats.Include(ch => ch.Messages).FirstOrDefaultAsync(m => m.ChatId == chatId);
            if (chat == null)
                return Results.NotFound();
            if (chat.UserId != userIdHelper.GetUserId())
                return Results.Unauthorized();
            dbContext.RemoveRange(chat.Messages);
            dbContext.Remove(chat);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }

    }

}
