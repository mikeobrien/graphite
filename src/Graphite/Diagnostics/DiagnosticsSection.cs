namespace Graphite.Diagnostics
{
    public interface IDiagnosticsSection
    {
        string Id { get; }
        string Name { get; }
        string Render();
    }

    public abstract class DiagnosticsSectionBase : IDiagnosticsSection
    {
        protected DiagnosticsSectionBase(string name)
        {
            Id = name.Replace(" ", "-").ToLower();
            Name = name;
        }
        
        public string Id { get; }
        public string Name { get; }
        public abstract string Render();
    }
}