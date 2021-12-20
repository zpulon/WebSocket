
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket
{
    public class JsonHelper : IJsonHelper, IAsyncJsonHelper
    {
        private JsonSerializerOptions _setting;
        public JsonHelper():this( new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreNullValues = false

        })
        {
            _setting.Converters.Add(new JsonStringEnumConverter());
            _setting.Converters.Add(new DateJsonConverter("yyyy-MM-dd HH:mm:ss", DateTimeZoneHandling.Local));
            _setting.Encoder = JavaScriptEncoder.Create(new UnicodeRange[]
            {
                UnicodeRanges.All
            });
        }

        public JsonHelper(JsonSerializerOptions setting)
        {
            _setting = setting;
        }
        public string ToJson(object obj)
        {
            if (obj == null)
            {
                return "";
            }
            return JsonSerializer.Serialize<object>(obj, _setting);
        }

        public Task ToJsonAsync(Stream stream, object obj)
        {
            if (stream == null || obj == null)
            {
                return Task.CompletedTask;
            }
            byte[] array = JsonSerializer.SerializeToUtf8Bytes(obj, _setting);
            new BinaryWriter(stream).Write(array);
            return Task.CompletedTask;
        }

        public object ToObject(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonSerializer.Deserialize(json, type, _setting);
        }

        public TObject ToObject<TObject>(string json)
        {
            return (TObject)this.ToObject(json, typeof(TObject));
        }

        public ValueTask<object> ToObjectAsync(Stream stream, Type type)
        {
            if (stream == null)
            {
                return new ValueTask<object>(null);
            }
            return JsonSerializer.DeserializeAsync(stream, type, _setting, default);
        }

        public ValueTask<TObject> ToObjectAsync<TObject>(Stream stream)
        {
            if (stream == null)
            {
                return new ValueTask<TObject>(null);
            }
            return JsonSerializer.DeserializeAsync<TObject>(stream, _setting, default);
        }
    }
}
