using System.Collections.Generic;
using Graphite.Http;
using Graphite.Routing;

namespace TestHarness
{
    public class Handler
    {
        public class OutputModel
        {
            public string Value { get; set; }
            public string Url1 { get; set; }
            public int Url2 { get; set; }
            public string Query1 { get; set; }
            public int Query2 { get; set; }
            public string[] MultiQuery1 { get; set; }
            public int[] MultiQuery2 { get; set; }
            public int? NullableQuery { get; set; }
        }

        public class InputModel
        {
            public string Value { get; set; }
        }

        public void GetWithNoResponse() { }
        
        public OutputModel GetWithResponse()
        {
            return new OutputModel { Value = "fark" };
        }

        public void GetWithQueryParam(int query) { }

        public OutputModel GetWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = "fark",
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public OutputModel GetWithMultiQueryParams(string[] query1, int[] query2)
        {
            return new OutputModel
            {
                MultiQuery1 = query1,
                MultiQuery2 = query2
            };
        }

        public OutputModel GetWithNullableQueryParams(int? query)
        {
            return new OutputModel
            {
                NullableQuery = query
            };
        }

        public class WildcardModel
        {
            public string Single { get; set; }
            public int[] Multi { get; set; }
        }

        public WildcardModel GetWithParamsWildcard_Ids(params int[] ids)
        {
            return new WildcardModel
            {
                Multi = ids
            };
        }

        public WildcardModel GetWithAttriubteWildcard_Ids([Wildcard] int[] ids)
        {
            return new WildcardModel
            {
                Multi = ids
            };
        }

        public WildcardModel GetWithSingleValueWildcard_Ids([Wildcard] string ids)
        {
            return new WildcardModel
            {
                Single = ids
            };
        }

        public void PostWithNoResponse(InputModel request) { }

        public OutputModel PostWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public OutputModel PostWithResponse(InputModel request)
        {
            return new OutputModel
            {
                Value = request.Value
            };
        }

        public OutputModel PostWithQueryParams(string query1, int query2)
        {
            return new OutputModel
            {
                Query1 = query1,
                Query2 = query2
            };
        }

        public OutputModel PostWithMultiQueryParams(string[] query1, int[] query2)
        {
            return new OutputModel
            {
                MultiQuery1 = query1,
                MultiQuery2 = query2
            };
        }

        public void PutWithNoResponse(InputModel request) { }

        public OutputModel PutWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public void PatchWithNoResponse(InputModel request) { }

        public OutputModel PatchWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public void DeleteWithNoResponse(InputModel request) { }

        public OutputModel DeleteWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public class DelimitedResponseModel
        {
            public List<int> Ids { get; set; }
        }

        public DelimitedResponseModel GetWithDelimitedQuerystring([Delimited] List<int> ids)
        {
            return new DelimitedResponseModel
            {
                Ids = ids
            };
        }
    }
}