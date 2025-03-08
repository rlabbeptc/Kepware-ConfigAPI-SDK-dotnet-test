using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{

    /// <summary>
    /// Represents a structured response from an API call, encapsulating a response code, an optional message, an optional value, and the endpoint reference.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the response.</typeparam>
    public readonly struct ApiResponse<T> : IEquatable<ApiResponse<T>>, IEquatable<T>
    {
        /// <summary>
        /// Gets the response code indicating the status of the API call.
        /// </summary>
        public ApiResponseCode ResponseCode { get; }

        /// <summary>
        /// Gets the optional message describing the result of the API call.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Gets the value returned from the API, if applicable.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets the endpoint that was called for this response.
        /// </summary>
        public string Endpoint { get; }

        /// <summary>
        /// Indicates whether the response represents a successful API call.
        /// </summary>
        public bool IsSuccess => ResponseCode is ApiResponseCode.Success or ApiResponseCode.Created or ApiResponseCode.Accepted;

        public ApiResponse(ApiResponseCode responseCode, T value, string endpoint)
        {
            ResponseCode = responseCode;
            Value = value;
            Message = null;
            Endpoint = endpoint;
        }

        public ApiResponse(ApiResponseCode responseCode, string message, string endpoint)
        {
            ResponseCode = responseCode;
            Message = message;
            Value = default;
            Endpoint = endpoint;
        }

        public void EnsureSuccess()
        {
            if (!IsSuccess)
                throw GetApiCallFailedException();
        }

        private ApiCallFailedException GetApiCallFailedException() => new(ResponseCode, Endpoint, Message ?? (IsSuccess ? "No value returned from successful API call." : "API call failed without specific message."));


        public static implicit operator T(ApiResponse<T> response)
        {
            if (!response.IsSuccess || response.Value is null)
#pragma warning disable S3877 // Exceptions should not be thrown from unexpected methods
                throw new InvalidCastException("Unable to cast api response to it's value", response.GetApiCallFailedException());
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods

            return response.Value;
        }

        public bool Equals(ApiResponse<T> other) => ResponseCode == other.ResponseCode && Message == other.Message && EqualityComparer<T?>.Default.Equals(Value, other.Value);
        public bool Equals(T? other) => EqualityComparer<T?>.Default.Equals(Value, other);
        public override bool Equals(object? obj) => obj is ApiResponse<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ResponseCode, Message, Value, Endpoint);

        public static bool operator ==(ApiResponse<T> left, ApiResponse<T> right) => left.Equals(right);
        public static bool operator !=(ApiResponse<T> left, ApiResponse<T> right) => !left.Equals(right);
    }
}
