﻿namespace Mango.Services.ProductAPI.Models.DTO
{
    public class ResponseDTO
    {
        public object? Result { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string Message { get; set; } = string.Empty;
    }
}
