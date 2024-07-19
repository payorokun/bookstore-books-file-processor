using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileProcessor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileProcessor.Utils;

public class BookJsonConverter : JsonConverter<Book>
{
    private readonly TypoCorrectionCache _cache;

    public BookJsonConverter(TypoCorrectionCache cache)
    {
        _cache = cache;
    }

    public override Book ReadJson(JsonReader reader, Type objectType, Book existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var correctedJson = new JObject();

        foreach (var property in jsonObject.Properties())
        {
            var correctedKey = _cache.GetCorrectedKeyAsync(property.Name).GetAwaiter().GetResult();
            correctedJson[correctedKey] = property.Value;
        }

        return correctedJson.ToObject<Book>();
    }

    public override void WriteJson(JsonWriter writer, Book value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}