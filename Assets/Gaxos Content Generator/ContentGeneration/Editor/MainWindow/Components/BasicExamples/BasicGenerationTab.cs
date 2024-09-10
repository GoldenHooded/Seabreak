using System.Collections.Generic;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Stability;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.BasicExamples
{
    public class BasicGenerationTab : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<BasicGenerationTab, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        
        public BasicGenerationTab()
        {
            var codeTextField = this.Q<TextField>("code");

            var prompt = this.Q<PromptInput>("promptInput");
            prompt.OnChanged += t =>
            {
                RefreshCode(codeTextField, t);
            };
            RefreshCode(codeTextField, prompt.value);

            var promptRequired = this.Q<Label>("promptRequiredLabel");
            promptRequired.style.visibility = Visibility.Hidden;

            var generateButton = this.Q<Button>("generateButton");

            var sendingRequest = this.Q<VisualElement>("sendingRequest");
            sendingRequest.style.display = DisplayStyle.None;
            var requestSent = this.Q<VisualElement>("requestSent");
            requestSent.style.display = DisplayStyle.None;
            var requestFailed = this.Q<VisualElement>("requestFailed");
            requestFailed.style.display = DisplayStyle.None;

            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;
                if (string.IsNullOrWhiteSpace(prompt.value))
                {
                    promptRequired.style.visibility = Visibility.Visible;
                    return;
                }

                generateButton.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                ContentGenerationApi.Instance.RequestStabilityTextToImageGeneration
                (new StabilityTextToImageParameters
                {
                    TextPrompts = new[]
                    {
                        new Prompt
                        {
                            Text = prompt.value,
                            Weight = 1,
                        }
                    },
                }, data: new
                {
                    player_id = ContentGenerationStore.editorPlayerId
                }).ContinueInMainThreadWith(
                    t =>
                    {
                        generateButton.SetEnabled(true);
                        sendingRequest.style.display = DisplayStyle.None;
                        if (t.IsFaulted)
                        {
                            requestFailed.style.display = DisplayStyle.Flex;
                            Debug.LogException(t.Exception);
                        }
                        else
                        {
                            prompt.value = null;
                            requestSent.style.display = DisplayStyle.Flex;
                        }
                        ContentGenerationStore.Instance.RefreshRequestsAsync().Finally(() => ContentGenerationStore.Instance.RefreshStatsAsync().CatchAndLog());
                    });
            });
        }
        void RefreshCode(TextField codeTextField, string promptText)
        {
            codeTextField.value =
                "var requestId = await ContentGenerationApi.Instance.RequestGeneration\n" +
                "\t(new StabilityTextToImageParameters\n" +
                "\t{\n" +
                "\t\tTextPrompts = new[]\n" +
                "\t\t{\n" +
                "\t\t\tnew Prompt\n" +
                "\t\t\t{\n" +
                $"\t\t\t\tText = \"{promptText}\",\n" +
                "\t\t\t\tWeight = 1,\n" +
                "\t\t\t}\n" +
                "\t\t}\n" +
                "\t})";
        }
    }
}