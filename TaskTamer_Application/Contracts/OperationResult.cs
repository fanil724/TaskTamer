namespace TaskTamer_Application.Contracts
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }


        public OperationResult() { }
        public OperationResult(bool isSuccess, string error) {
            IsSuccess = isSuccess;
            Message = error;
        }

        public static OperationResult Success(string message="") =>
            new OperationResult { IsSuccess = true, Message = message };

        public static OperationResult Failure(string message) =>
            new OperationResult { IsSuccess = false, Message = message };
    }


    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; }

        protected OperationResult(T? data, bool isSuccess, string error)
            : base(isSuccess, error)
        {
            Data = data;
        }

        public static OperationResult<T> Success(T data, string message="") => new(data, true, message);
        public new static OperationResult<T> Failure(string error) => new(default, false, error);
    }
}
