using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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

        BaseStep step = stepType switch
        {
            EStepType.Wait => new WaitStep(),
            EStepType.Type => new TypeStep(EDisplayType.Type),
            EStepType.Button => new ButtonStep(),
            EStepType.Command => new CommandStep(),
            EStepType.Action => new ActionStep(),
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
