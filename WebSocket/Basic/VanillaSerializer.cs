using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket
{
    public class VanillaSerializer : IElasticsearchSerializer
    {
        private IJsonHelper _jsonHelper;

        public VanillaSerializer():this(new ServiceCollection().BuildServiceProvider().GetService<JsonHelper>())
        { 

        }

        public VanillaSerializer(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }
        public T Deserialize<T>(Stream stream) => throw new NotImplementedException();

        public object Deserialize(Type type, Stream stream) => throw new NotImplementedException();

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default(CancellationToken)) =>
            throw new NotImplementedException();

        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default(CancellationToken)) =>
            throw new NotImplementedException();

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.Indented) =>
            _jsonHelper.ToJson(data);

        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.Indented,
            CancellationToken cancellationToken = default(CancellationToken)) =>
            throw new NotImplementedException();
    }
}
