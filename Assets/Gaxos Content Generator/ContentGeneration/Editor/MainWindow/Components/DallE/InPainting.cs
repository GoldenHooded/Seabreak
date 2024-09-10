using System.Collections.Generic;
using ContentGeneration.Helpers;
using ContentGeneration.Models.DallE;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.DallE
{
    public class InPainting : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<InPainting, UxmlTraits>
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
        DallEParametersElement dallEParametersElement => this.Q<DallEParametersElement>("dallEParametersElement");
        GenerationOptionsElement generationOptionsElement => this.Q<GenerationOptionsElement>("generationOptions");
        ImageSelection image => this.Q<ImageSelection>("image");
        Label imageRequired => this.Q<Label>("imageRequiredLabel");
        ImageSelection mask => this.Q<ImageSelection>("mask");
        VisualElement requestSent => this.Q<VisualElement>("requestSent");
        VisualElement requestFailed => this.Q<VisualElement>("requestFailed");
        VisualElement sendingRequest => this.Q<VisualElement>("sendingRequest");
        Button generateButton => this.Q<Button>("generateButton");

        public InPainting()
        {
            dallEParametersElement.OnCodeChanged += RefreshCode;
            generationOptionsElement.OnCodeChanged += RefreshCode;
            
            dallEParametersElement.model.value = Model.DallE2;
            dallEParametersElement.model.SetEnabled(false);

            imageRequired.style.visibility = Visibility.Hidden;
           
            requestSent.style.display = DisplayStyle.None;
            requestFailed.style.display = DisplayStyle.None;
            sendingRequest.style.display = DisplayStyle.None;

            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;
                imageRequired.style.visibility = Visibility.Hidden;

                if (!dallEParametersElement.Valid())
                {
                    return;
                }

                if (image.image == null)
                {
                    imageRequired.style.visibility = Visibility.Visible;
                    return;
                }

                generateButton.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                var parameters = new DallEInpaintingParameters
                {
                    Image = (Texture2D)image.image,
                    Mask = (Texture2D)mask.image,
                };
                dallEParametersElement.ApplyParameters(parameters);
                ContentGenerationApi.Instance.RequestDallEInpaintingGeneration(
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
                "var requestId = await ContentGenerationApi.Instance.RequestDallETextToImageGeneration\n" +
                "\t(new DallETextToImageParameters\n" +
                "\t{\n" +
                dallEParametersElement?.GetCode() +
                "\t},\n" +
                $"{generationOptionsElement?.GetCode()}" +
                ")";
        }
    }
}