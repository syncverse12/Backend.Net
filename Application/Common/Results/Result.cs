namespace SyncVerse.Application.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        protected Result(bool isSuccess, string message, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public static Result Success(string message = "") => new(true, message);
        public static Result Failure(string message, List<string>? errors = null) => new(false, message, errors);
    }
}