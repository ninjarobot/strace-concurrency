using System;
using System.Net.Http;

namespace strace_concurrency_cs
{
    class Program
    {
        static readonly Uri googleUri = new Uri("https://www.cnn.com");
        static readonly HttpClient http = new HttpClient();
        static async System.Threading.Tasks.Task getTpl() {
            var httpResponse = await http.GetAsync(googleUri);
            var content = await httpResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Read page length: {content.Length}");
        }
        static void Main(string[] args)
        {
            System.Threading.Tasks.Task.Run(async () => 
                {
                    var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
                    for (int i = 0; i < 100; i++) {
                        tasks.Add(getTpl ());
                    }
                    await System.Threading.Tasks.Task.WhenAll(tasks);
                }
            ).GetAwaiter().GetResult();
            
        }
    }
}
