namespace Graphite.Readers
{
    public enum ReadStatus
    {
        Success, Failure
    }

    public class ReadResult
    {
        public ReadResult(object value)
        {
            Status = ReadStatus.Success;
            Value = value;
        }

        public ReadResult(ReadStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public ReadStatus Status { get; }
        public string ErrorMessage { get; }
        public object Value { get; set; }

        public static ReadResult Success(object value)
        {
            return new ReadResult(value);
        }

        public static ReadResult Failure(string errorMessage)
        {
            return new ReadResult(ReadStatus.Failure, errorMessage);
        }
    }
}