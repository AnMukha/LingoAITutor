﻿using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Dto
{
    public class MessageDto
    {        
        public Guid MessageId { get; set; }
        public string? Content { get; set; }
        public string? CorrectedContent { get; set; }
        public string? Corrections { get; set; }        
        public MessageType MessageType { get; set; }
    }
}
