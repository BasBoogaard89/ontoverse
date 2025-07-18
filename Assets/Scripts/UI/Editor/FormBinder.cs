using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

public class FormBinder
{
    private Action redrawUI;
    private readonly Action onChange;
    private List<Action> unbindActions = new();

    public FormBinder(Action redrawUI = null, Action onChange = null)
    {
        this.redrawUI = redrawUI;
        this.onChange = onChange;
    }

    public void AutoBindFields(VisualElement root, object target)
    {
        var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var element = root.Q<VisualElement>(field.Name);
            if (element == null)
                continue;

            if (field.FieldType == typeof(string) && element is TextField tf)
            {
                Bind(tf,
                    () => (string)field.GetValue(target),
                    v => field.SetValue(target, v));
            } else if (field.FieldType == typeof(bool) && element is Toggle toggle)
            {
                Bind(toggle,
                    () => (bool)field.GetValue(target),
                    v => field.SetValue(target, v));
            } else if (field.FieldType == typeof(float) && element is FloatField ff)
            {
                Bind(ff,
                    () => (float)field.GetValue(target),
                    v => field.SetValue(target, v));
            } else if (field.FieldType.IsEnum && element is EnumField ef)
            {
                Bind(ef,
                    () => (Enum)field.GetValue(target),
                    v => field.SetValue(target, v));
            }
        }
    }

    public void Bind(TextField field, Func<string> getter, Action<string> setter)
    {
        field.SetValueWithoutNotify(getter());

        RegisterBinding<ChangeEvent<string>, string>(field, evt =>
        {
            setter(evt.newValue);
            onChange?.Invoke();
        });
    }

    public void Bind<TEnum>(EnumField field, Func<TEnum> getter, Action<TEnum> setter) where TEnum : Enum
    {
        field.Init(getter());
        field.SetValueWithoutNotify(getter());

        RegisterBinding<ChangeEvent<Enum>, Enum>(field, evt =>
        {
            setter((TEnum)evt.newValue);
            onChange?.Invoke();
        });
    }

    public void Bind(FloatField field, Func<float> getter, Action<float> setter)
    {
        field.SetValueWithoutNotify(getter());

        RegisterBinding<ChangeEvent<float>, float>(field, evt =>
        {
            setter(evt.newValue);
            onChange?.Invoke();
        });
    }

    public void Bind(Toggle toggle, Func<bool> getter, Action<bool> setter, VisualElement conditionalContent = null)
    {
        toggle.SetValueWithoutNotify(getter());
        conditionalContent?.SetDisplay(getter());

        RegisterBinding<ChangeEvent<bool>, bool>(toggle, evt =>
        {
            setter(evt.newValue);
            conditionalContent?.SetDisplay(evt.newValue);
            onChange?.Invoke();
            redrawUI?.Invoke();
        });
    }

    private void RegisterBinding<TEvent, TValue>(INotifyValueChanged<TValue> field, Action<ChangeEvent<TValue>> handler)
        where TEvent : ChangeEvent<TValue>, new()
    {
        EventCallback<ChangeEvent<TValue>> callback = evt => handler(evt);
        field.RegisterValueChangedCallback(callback);
        unbindActions.Add(() => field.UnregisterValueChangedCallback(callback));
    }

    public void UnbindAll()
    {
        foreach (var unbind in unbindActions)
            unbind();

        unbindActions.Clear();
    }
}