using System;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Extensions;

namespace TestHarness
{
    public class PerformanceHandler
    {
        public PerfOutputModel Post_PerformanceTests_Graphite_Url1_Url2_Url3(PerfInputModel request, 
            string url1, Guid url2, int url3, string query1, Guid query2, int query3)
        {
            return PerfOutputModel.FromInput(request, url1, url2, url3, query1, query2, query3);
        }

        public Task<PerfOutputModel> Post_PerformanceTests_Graphite_Async_Url1_Url2_Url3(
            PerfInputModel request, string url1,Guid url2, int url3, string query1, Guid query2, int query3)
        {
            return PerfOutputModel.FromInput(request, url1, url2, 
                url3, query1, query2, query3).ToTaskResult();
        }
    }

    public class PerformanceController : ApiController
    {
        [Route("performancetests/webapi/{url1}/{url2}/{url3}")]
        public PerfOutputModel Post(PerfInputModel request, string url1, Guid url2,
            int url3, string query1, Guid query2, int query3)
        {
            return PerfOutputModel.FromInput(request, url1, url2, url3, query1, query2, query3);
        }

        [Route("performancetests/webapi/async/{url1}/{url2}/{url3}")]
        public Task<PerfOutputModel> PostAsync(PerfInputModel request, string url1, 
            Guid url2,int url3, string query1, Guid query2, int query3)
        {
            return PerfOutputModel.FromInput(request, url1, url2, 
                url3, query1, query2, query3).ToTaskResult();
        }
    }

    public class PerfInputModel
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
    }

    public class PerfOutputModel
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Url1 { get; set; }
        public Guid Url2 { get; set; }
        public int Url3 { get; set; }
        public string Query1 { get; set; }
        public Guid Query2 { get; set; }
        public int Query3 { get; set; }

        public static PerfOutputModel FromInput(PerfInputModel request, string url1, 
            Guid url2, int url3, string query1, Guid query2, int query3)
        {
            return new PerfOutputModel
            {
                Value1 = request.Value1,
                Value2 = request.Value2,
                Value3 = request.Value3,
                Url1 = url1,
                Url2 = url2,
                Url3 = url3,
                Query1 = query1,
                Query2 = query2,
                Query3 = query3
            };
        }
    }
}