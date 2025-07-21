using System.Linq;
using UnityEngine.UIElements;

namespace Ontoverse.DialogueSystem.Editor
{
    public class ActionStepForm
    {
        private readonly VisualElement paramsList;
        private readonly Button addButton;
        private readonly VisualTreeAsset paramRowTemplate;
        private int maxParamsCount = 5;

        public ActionStepForm(VisualElement root)
        {
            paramsList = root.Q<VisualElement>("params-list");
            addButton = root.Q<Button>("add-param-button");
            paramRowTemplate = AssetHelper.LoadEditorAsset<VisualTreeAsset>("/Views/ParamRowView.uxml");
        }

        public void Setup(ActionStep step)
        {
            paramsList.Clear();

            foreach (var kvp in step.Parameters.ToList())
            {
                AddParamRow(step, kvp.Key, (string)kvp.Value);
            }

            addButton.clicked += () =>
            {
                if (step.Parameters.Count >= maxParamsCount)
                    return;

                string newKey = "param" + (step.Parameters.Count + 1);
                step.Parameters[newKey] = "";
                AddParamRow(step, newKey, "");

                addButton.SetEnabled(step.Parameters.Count < maxParamsCount);
            };
        }

        private void AddParamRow(ActionStep step, string key, string value)
        {
            var row = paramRowTemplate.CloneTree();
            var keyField = row.Q<TextField>("key-field");
            var valueField = row.Q<TextField>("value-field");
            var removeButton = row.Q<Button>("remove-button");

            keyField.value = key;
            valueField.value = value.ToString();

            keyField.RegisterValueChangedCallback(evt =>
            {
                string oldKey = key;
                string newKey = evt.newValue;

                if (string.IsNullOrWhiteSpace(newKey))
                {
                    keyField.SetValueWithoutNotify(oldKey);
                    return;
                }

                if (oldKey == newKey)
                    return;

                if (!step.Parameters.ContainsKey(oldKey))
                    return;

                if (step.Parameters.ContainsKey(newKey))
                {
                    keyField.SetValueWithoutNotify(oldKey);
                    return;
                }

                var oldValue = step.Parameters[oldKey];
                step.Parameters.Remove(oldKey);
                step.Parameters[newKey] = oldValue;
            });


            valueField.RegisterValueChangedCallback(evt =>
            {
                step.Parameters[keyField.value] = evt.newValue;
            });

            removeButton.clicked += () =>
            {
                step.Parameters.Remove(keyField.value);
                paramsList.Remove(row);
            };

            paramsList.Add(row);
        }
    }
}