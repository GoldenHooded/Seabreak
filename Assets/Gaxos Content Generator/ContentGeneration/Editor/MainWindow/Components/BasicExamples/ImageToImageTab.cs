using System;
using System.Collections.Generic;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Stability;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.BasicExamples
{
    public class ImageToImageTab : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<ImageToImageTab, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        public ImageToImageTab()
        {
            var codeTextField = this.Q<TextField>("code");

            var prompt = this.Q<PromptInput>("promptInput");
            prompt.OnChanged += value =>
            {
                RefreshCode(codeTextField, value);
            };
            RefreshCode(codeTextField, prompt.value);
        
            var promptRequired = this.Q<Label>("promptRequiredLabel");
            promptRequired.style.visibility = Visibility.Hidden;

            var image = this.Q<ImageSelection>("image");
            var imageRequired = this.Q<Label>("imageRequiredLabel");
            imageRequired.style.visibility = Visibility.Hidden;

            var generateButton = this.Q<Button>("generateButton");
            var sendingRequest = this.Q<VisualElement>("sendingRequest");
            var requestSent = this.Q<VisualElement>("requestSent");
            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                if (string.IsNullOrWhiteSpace(prompt.value))
                {
                    promptRequired.style.visibility = Visibility.Visible;
                    return;
                }

                if (image.image == null)
                {
                    imageRequired.style.visibility = Visibility.Visible;
                    return;
                }

                generateButton.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                ContentGenerationApi.Instance.RequestStabilityImageToImageGeneration
                (new StabilityImageToImageParameters
                {
                    TextPrompts = new[]
                    {
                        new Prompt
                        {
                            Text = prompt.value,
                            Weight = 1,
                        }
                    },
                    InitImage = (Texture2D)image.image
                }, data: new
                {
                    player_id = ContentGenerationStore.editorPlayerId
                }).ContinueInMainThreadWith(
                    t =>
                    {
                        try
                        {
                            if (t.IsFaulted)
                            {
                                Debug.LogException(t.Exception);
                            }
                            else
                            {
                                prompt.value = null;
                                image.image = null;
                                requestSent.style.display = DisplayStyle.Flex;
                            }

                            generateButton.SetEnabled(true);
                            sendingRequest.style.display = DisplayStyle.None;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                        ContentGenerationStore.Instance.RefreshRequestsAsync().Finally(() => ContentGenerationStore.Instance.RefreshStatsAsync().CatchAndLog());
                    });
            });
        }

        void RefreshCode(TextField codeTextField, string promptText)
        {
            codeTextField.value =
                "var requestId = await ContentGenerationApi.Instance.RequestImageToImageGeneration\n" +
                "\t(new StabilityImageToImageParameters\n" +
                "\t{\n" +
                "\t\tTextPrompts = new[]\n" +
                "\t\t{\n" +
                "\t\t\tnew Prompt\n" +
                "\t\t\t{\n" +
                $"\t\t\t\tText = \"{promptText}\",\n" +
                "\t\t\t\tWeight = 1,\n" +
                "\t\t\t},\n" +
                "\t\t\tInitImage = <Texture2D object>\n" +
                "\t\t}\n" +
                "\t})";
        }
    }
}