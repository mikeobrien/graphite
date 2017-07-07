namespace TestHarness.Views
{
    public class ViewModel
    {
        public string Value { get; set; }
    }
    
    public class MultipleViewTestHandler
    {
        public ViewModel Get_Multiple()
        {
            return new ViewModel
            {
                Value = "fark"
            };
        }
    }

    public class MustacheFileViewTestHandler
    {
        public ViewModel Get_Mustache_File()
        {
            return new ViewModel
            {
                Value = "fark"
            };
        }
    }

    public class MustacheResourceViewTestHandler
    {
        public ViewModel Get_Mustache_Resource()
        {
            return new ViewModel
            {
                Value = "fark"
            };
        }
    }

    public class RazorFileViewTestHandler
    {
        public ViewModel Get_Razor_File()
        {
            return new ViewModel
            {
                Value = "fark"
            };
        }
    }

    public class RazorResourceViewTestHandler
    {
        public ViewModel Get_Razor_Resource()
        {
            return new ViewModel
            {
                Value = "fark"
            };
        }
    }
}
