namespace ALOud.Common
{
    /// <summary>
    /// Represents the result of an operation with success/failure status and optional data
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Indicates whether the operation succeeded
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The data returned by the operation (only set if IsSuccess is true)
        /// </summary>
        public T? Data { get; }

        /// <summary>
        /// The error message if the operation failed
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Collection of validation errors
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; }

        private Result(bool isSuccess, T? data, string? error, Dictionary<string, string[]>? validationErrors = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
            ValidationErrors = validationErrors;
        }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        /// <param name="data">The data to return</param>
        /// <returns>A successful Result instance</returns>
        public static Result<T> Success(T data) => new(true, data, null);

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A failed Result instance</returns>
        public static Result<T> Failure(string error) => new(false, default, error);

        /// <summary>
        /// Creates a failed result with validation errors
        /// </summary>
        /// <param name="validationErrors">Dictionary of field names to error messages</param>
        /// <returns>A failed Result instance with validation errors</returns>
        public static Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors) 
            => new(false, default, "Validation failed", validationErrors);

        /// <summary>
        /// Creates a not found result
        /// </summary>
        /// <returns>A failed Result indicating resource not found</returns>
        public static Result<T> NotFound() => new(false, default, "Resource not found");

        /// <summary>
        /// Creates a not found result with custom message
        /// </summary>
        /// <param name="message">The not found message</param>
        /// <returns>A failed Result indicating resource not found</returns>
        public static Result<T> NotFound(string message) => new(false, default, message);
    }

    /// <summary>
    /// Represents the result of an operation without return data
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates whether the operation succeeded
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The error message if the operation failed
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Collection of validation errors
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; }

        private Result(bool isSuccess, string? error, Dictionary<string, string[]>? validationErrors = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            ValidationErrors = validationErrors;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <returns>A successful Result instance</returns>
        public static Result Success() => new(true, null);

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A failed Result instance</returns>
        public static Result Failure(string error) => new(false, error);

        /// <summary>
        /// Creates a failed result with validation errors
        /// </summary>
        /// <param name="validationErrors">Dictionary of field names to error messages</param>
        /// <returns>A failed Result instance with validation errors</returns>
        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) 
            => new(false, "Validation failed", validationErrors);
    }
}
