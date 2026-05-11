using System;
using System.Collections.Generic;
using System.Text;

namespace RENTALFUTSAL_KPLHEBAT.Models
{
    public sealed class OperationResult<T>
    {
        private OperationResult(bool success, string message, T? data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public bool Success { get; }
        public string Message { get; }
        public T? Data { get; }

        public static OperationResult<T> Ok(T data, string message = "Berhasil") => new(true, message, data);
        public static OperationResult<T> Fail(string message) => new(false, message, default);
    }
}
