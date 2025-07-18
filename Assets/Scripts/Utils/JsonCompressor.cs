using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class JsonCompressor
{
    static JsonSerializerSettings JsonSerializerSettings(bool pretty)
    {
        return new JsonSerializerSettings
        {
            Converters = { new DialogueStepConverter() },
            Formatting = pretty ? Formatting.Indented : Formatting.None
        };
    }

    public static string Serialize(object obj, bool pretty = true)
    {
        return JsonConvert.SerializeObject(obj, JsonSerializerSettings(pretty));
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings(false));
    }

    public static byte[] CompressString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    public static string DecompressBytes(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        return reader.ReadToEnd();
    }
}
