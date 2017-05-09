namespace Graphite.Binding
{
    public class MapResult
    {
        public MapResult(bool mapped, object value)
        {
            Mapped = mapped;
            Value = value;
        }

        public static MapResult NotMapped()
        {
            return new MapResult(false, null);
        }

        public static MapResult WasMapped(object value)
        {
            return new MapResult(true, value);
        }

        public bool Mapped { get; }
        public object Value { get; }
    }
}