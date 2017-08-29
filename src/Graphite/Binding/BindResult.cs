namespace Graphite.Binding
{
    public enum BindingStatus
    {
        Success, Failure, NoReader
    }

    public class BindResult
    {
        public BindResult(BindingStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public BindingStatus Status { get; }
        public string ErrorMessage { get; }

        public static BindResult Success()
        {
            return new BindResult(BindingStatus.Success);
        }

        public static BindResult Failure(string errorMessage)
        {
            return new BindResult(BindingStatus.Failure, errorMessage);
        }

        public static BindResult NoReader()
        {
            return new BindResult(BindingStatus.NoReader);
        }
    }
}