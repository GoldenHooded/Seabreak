using System.Collections.Generic;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Gaxos;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.Gaxos
{
    public class Masking : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<Masking, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        TextField code => this.Q<TextField>("code");
        GaxosParametersElement gaxosParametersElement => this.Q<GaxosParametersElement>("gaxosParametersElement");
        GenerationOptionsElement generationOptionsElement => this.Q<GenerationOptionsElement>("generationOptions");
        ImageSelection mask => this.Q<ImageSelection>("mask");
        Label maskRequired => this.Q<Label>("maskRequiredLabel");
        VisualElement requestSent => this.Q<VisualElement>("requestSent");
        VisualElement requestFailed => this.Q<VisualElement>("requestFailed");
        VisualElement sendingRequest => this.Q<VisualElement>("sendingRequest");
        Button generateButton => this.Q<Button>("generateButton");

        public Masking()
        {
            gaxosParametersElement.OnCodeChanged += RefreshCode;
            generationOptionsElement.OnCodeChanged += RefreshCode;

            maskRequired.style.visibility = Visibility.Hidden;

            requestSent.style.display = DisplayStyle.None;
            requestFailed.style.display = DisplayStyle.None;
            sendingRequest.style.display = DisplayStyle.None;

            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;
                maskRequired.style.visibility = Visibility.Hidden;

                if (!gaxosParametersElement.Valid())
                {
                    return;
                }

                if (mask.image == null)
                {
                    maskRequired.style.visibility = Visibility.Visible;
                    return;
                }

                generateButton.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                var parameters = new GaxosMaskingParameters
                {
                    Mask = (Texture2D)mask.image,
                };
                gaxosParametersElement.ApplyParameters(parameters);
                ContentGenerationApi.Instance.RequestGaxosMaskingGeneration(
                    parameters,
                    generationOptionsElement.GetGenerationOptions(), data: new
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
                            requestSent.style.display = DisplayStyle.Flex;
                        }

                        ContentGenerationStore.Instance.RefreshRequestsAsync().Finally(() => ContentGenerationStore.Instance.RefreshStatsAsync().CatchAndLog());
                    });
            });

            RefreshCode();
        }

        void RefreshCode()
        {
            code.value =
                "var requestId = await ContentGenerationApi.Instance.RequestGaxosMaskingGeneration\n" +
                "\t(new GaxosMaskingParameters\n" +
                "\t{\n" +
                "\t\tMask = <Texture2D object>,\n" +
                gaxosParametersElement?.GetCode() +
                "\t},\n" +
                $"{generationOptionsElement?.GetCode()}" +
                ")";
        }
    }
}