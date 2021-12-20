using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocket;

namespace WebSocket.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public class TestData
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public async Task IndexManyAsync()
        {
            var pool = new SingleNodeConnectionPool(new Uri(Url.url));
            var setting = new ConnectionSettings(pool, sourceSerializer: (builtin, settings) => new VanillaSerializer())
                .DefaultIndex("people");
                //.DisableDirectStreaming()
                //.PrettyJson();
            var client = new ElasticClient(setting);
            //最大2kb
            List<Person> person = new List<Person>();
            for (int i = 2; i < 10; i++)
            {
                Person persons = new()
                {
                    Id = i,
                    FirstName = $"Csharp{i}",
                    LastName = $"Martijn{i}"
                };
                person.Add(persons);
            }
            //var person = new Person
            //{
            //    Id = 1,
            //    FirstName = "Martijn",
            //    LastName = "Laarman"
            //};

            //var response = client.IndexDocument(peoples);
            var asyncResponse= await client.IndexDocumentAsync(person);
            //var indexResponse = client.IndexDocument(person);
        }

        /// <summary>
        /// 搜索
        /// </summary>
        public IEnumerable<Person> Search()
        {
            var setting = new ConnectionSettings(new Uri(Url.url)).DefaultIndex("people");
            var client = new ElasticClient(setting);
            var searchResponse = client.Search<Person>(s => s.From(0)
            .Size(10)
            .Query(q => q
                .Match(m =>m
                   .Field(f => f.FirstName)
                   .Query("Csharp1")
                      )
                  )       
            );
            var people = searchResponse.Documents;
            return people;
        }
    }
}
