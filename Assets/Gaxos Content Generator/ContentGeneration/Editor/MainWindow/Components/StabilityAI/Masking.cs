using System;
using System.Collections.Generic;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Stability;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.StabilityAI
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
        
        DropdownField engine => this.Q<DropdownField>("engine");
        Button generate => this.Q<Button>("generate");
        VisualElement sendingRequest => this.Q<VisualElement>("sendingRequest");
        VisualElement requestSent => this.Q<VisualElement>("requestSent");
        VisualElement requestFailed => this.Q<VisualElement>("requestFailed");
        ImageSelection image => this.Q<ImageSelection>("image");
        Label imageRequired => this.Q<Label>("imageRequired");
        ImageSelection mask => this.Q<ImageSelection>("mask");
        Label maskRequired => this.Q<Label>("maskRequired");
        EnumField maskSource => this.Q<EnumField>("maskSource");
        TextField code => this.Q<TextField>("code");
        StabilityParametersElement stabilityParameters => this.Q<StabilityParametersElement>("stabilityParameters");
        GenerationOptionsElement generationOptions => this.Q<GenerationOptionsElement>("generationOptions");

        public Masking()
        {
            stabilityParameters.OnCodeChanged += RefreshCode;
            generationOptions.OnCodeChanged += RefreshCode;
            
            var engines = new[]
            {
                "esrgan-v1-x2plus",
                "stable-diffusion-xl-1024-v0-9",
                "stable-diffusion-xl-1024-v1-0",
                "stable-diffusion-v1-6",
                "stable-diffusion-512-v2-1",
                "stable-diffusion-xl-beta-v2-2-2",
            };
            engine.choices = new List<string>(engines);
            engine.index = Array.IndexOf(engines, "stable-diffusion-v1-6");

            sendingRequest.style.display = DisplayStyle.None;
            requestSent.style.display = DisplayStyle.None;
            requestFailed.style.display = DisplayStyle.None;

            imageRequired.style.visibility = Visibility.Hidden;

            maskRequired.style.visibility = Visibility.Hidden;

            mask.style.display = (MaskSource)maskSource.value == MaskSource.InitImageAlpha
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            maskSource.RegisterValueChangedCallback(evt =>
            {
                mask.style.display = (MaskSource)evt.newValue == MaskSource.InitImageAlpha
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
                RefreshCode();
            });
            mask.style.display = (MaskSource)maskSource.value == MaskSource.InitImageAlpha
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            generate.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generate.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;
                imageRequired.style.visibility = Visibility.Hidden;
                maskRequired.style.visibility = Visibility.Hidden;
                if (!stabilityParameters.Valid())
                {
                    return;
                }

                if (image.image == null)
                {
                    imageRequired.style.visibility = Visibility.Visible;
                    return;
                }

                var maskSourceValue = (MaskSource)this.maskSource.value;
                if (mask.image == null && maskSourceValue != MaskSource.InitImageAlpha)
                {
                    maskRequired.style.visibility = Visibility.Visible;
                    return;
                }

                generate.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                var parameters = new StabilityMaskedImageParameters
                {
                    EngineId = engine.value,
                    InitImage = (Texture2D)image.image,
                    MaskImage = maskSourceValue == MaskSource.InitImageAlpha ? null : (Texture2D)mask.image,
                    MaskSource = maskSourceValue,
                };
                stabilityParameters.ApplyParameters(parameters);
                ContentGenerationApi.Instance.RequestStabilityMaskedImageGeneration(
                    parameters,
                    generationOptions.GetGenerationOptions(), data: new
                    {
                        player_id = ContentGenerationStore.editorPlayerId
                    }).ContinueInMainThreadWith(
                    t =>
                    {
                        generate.SetEnabled(true);
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

            engine.RegisterValueChangedCallback(_ => RefreshCode());

            code.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
            RefreshCode();
        }

        void RefreshCode()
        {
            var maskSourceValue = (MaskSource)this.maskSource.value;
            code.value =
                "var requestId = await ContentGenerationApi.Instance.RequestMaskedImageGeneration\n" +
                "\t(new StabilityMaskedImageParameters\n" +
                "\t{\n" +
                $"\t\tEngineId = \"{engine.value}\",\n" +
                "\t\tInitImage = <Texture2D object>,\n" +
                (maskSourceValue == MaskSource.InitImageAlpha ? "" : "\t\tMaskImage = <Texture2D object>,\n") +
                $"\t\tMaskSource = MaskSource.{maskSourceValue},\n" +
                stabilityParameters.GetCode() +
                "\t},\n" +
                $"{generationOptions.GetCode()}" +
                ")";
        }
    }
}