using Ontoverse.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Ontoverse.DialogueSystem.Editor
{
    public class FormBinder
    {
        private Action redrawUI;
        private readonly Action OnChange;
        private readonly List<Action> unbindActions = new();

        // TODO: onchange bepalen gebruiken ja of nee
        public FormBinder(Action redrawUI = null, Action onChange = null)
        {
            this.redrawUI = redrawUI;
            this.OnChange = onChange;
        }

        public void AutoBindFields(VisualElement root, object target, string prefix = "")
        {
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                string fullName = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}.{field.Name}";
                var element = root.Q<VisualElement>(fullName);

                if (element != null)
                {
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
                } else if (!field.FieldType.IsPrimitive && !field.FieldType.IsEnum && !field.FieldType.IsArray && !field.FieldType.IsGenericType)
                {
                    var subTarget = field.GetValue(target);
                    if (subTarget != null)
                    {
                        AutoBindFields(root, subTarget, fullName);
                    }
                }
            }
        }

        public void Bind(TextField field, Func<string> getter, Action<string> setter)
        {
            field.SetValueWithoutNotify(getter());

            RegisterBinding<ChangeEvent<string>, string>(field, evt =>
            {
                setter(evt.newValue);
                OnChange?.Invoke();
            });
        }

        public void Bind<TEnum>(EnumField field, Func<TEnum> getter, Action<TEnum> setter) where TEnum : Enum
        {
            field.Init(getter());
            field.SetValueWithoutNotify(getter());

            RegisterBinding<ChangeEvent<Enum>, Enum>(field, evt =>
            {
                setter((TEnum)evt.newValue);
                OnChange?.Invoke();
            });
        }

        public void Bind(FloatField field, Func<float> getter, Action<float> setter)
        {
            field.SetValueWithoutNotify(getter());

            RegisterBinding<ChangeEvent<float>, float>(field, evt =>
            {
                setter(evt.newValue);
                OnChange?.Invoke();
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
                OnChange?.Invoke();
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
}