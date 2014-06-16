using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DoRequest().Wait();
        }

        public static async Task DoRequest()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8080/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync("api/nodes?includeData=true");
                if (response.IsSuccessStatusCode)
                {
                    var nodes = await response.Content.ReadAsAsync<NSysmon.Collector.Api.NodeListViewModel>();
                }
            }
        }
    }
}
