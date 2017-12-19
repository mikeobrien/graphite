namespace Graphite.Reflection
{
    public class ParseResult<T>
    {
        private ParseResult(string original, T result, bool success, string errorMessage)
        {
            Result = result;
            Original = original;
            Success = success;
            ErrorMessage = errorMessage;
        }

        public string Original { get; }
        public T Result { get; }

        public bool Success { get; }
        public string ErrorMessage { get; }

        public static ParseResult<T> Succeeded(string original, T result)
        {
            return new ParseResult<T>(original, result, true, null);
        }

        public static ParseResult<T> Failed(string original, string errorMessage)
        {
            return new ParseResult<T>(original, default(T), false, errorMessage);
        }
    }
}