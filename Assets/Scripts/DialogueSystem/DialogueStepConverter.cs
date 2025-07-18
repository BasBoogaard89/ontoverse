using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class DialogueStepConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(BaseStep);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var stepType = jo["StepType"]?.ToString();

        BaseStep step = stepType switch
        {
            "Wait" => new WaitStep(),
            "Type" or "Prompt" or "FakeUserInput" => new DialogueStep(EDialogueStepType.None),
            _ => throw new JsonSerializationException($"Unknown StepType: {stepType}")
        };

        serializer.Populate(jo.CreateReader(), step);
        return step;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.FromObject(value, serializer);
        jo.WriteTo(writer);
    }
}
