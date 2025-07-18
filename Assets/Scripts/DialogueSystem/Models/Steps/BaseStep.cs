using Newtonsoft.Json;
using System;

[JsonConverter(typeof(DialogueStepConverter))]
public abstract class BaseStep
{
}