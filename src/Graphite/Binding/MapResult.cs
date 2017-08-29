namespace Graphite.Binding
{
    public enum MappingStatus
    {
        Success, Failure, NoMapper
    }

    public class MapResult
    {
        public MapResult(object value)
        {
            Status = MappingStatus.Success;
            Value = value;
        }

        public MapResult(MappingStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public MappingStatus Status { get; }
        public string ErrorMessage { get; }
        public object Value { get; set; }

        public static MapResult Success(object value)
        {
            return new MapResult(value);
        }

        public static MapResult Failure(string errorMessage)
        {
            return new MapResult(MappingStatus.Failure, errorMessage);
        }

        public static MapResult NoMapper()
        {
            return new MapResult(MappingStatus.NoMapper);
        }
    }
}