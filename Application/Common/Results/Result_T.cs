namespace SyncVerse.Application.Common.Results
{
    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        private Result(bool isSuccess, T? data, string message, List<string>? errors = null)
            : base(isSuccess, message, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data, string message = "") => new(true, data, message);
        public static new Result<T> Failure(string message, List<string>? errors = null)
            => new(false, default, message, errors);
    }
}