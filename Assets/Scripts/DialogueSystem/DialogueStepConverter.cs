using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Ontoverse.DialogueSystem
{
    public class DialogueStepConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(BaseStep);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var stepTypeString = jo["StepType"]?.ToString();

            if (!Enum.TryParse(typeof(EStepType), stepTypeString, out var enumObj))
                throw new JsonSerializationException($"Unknown StepType: {stepTypeString}");

            var stepType = (EStepType)enumObj;

            var step = StepFactory.Step[stepType]();

            serializer.Populate(jo.CreateReader(), step);

            return step;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = JObject.FromObject(value, serializer);
            jo.WriteTo(writer);
        }
    }
}