using System;
using System.Collections.Generic;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Stability;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.StabilityAI
{
    public class ImageToImage : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<ImageToImage, UxmlTraits>
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
        Button generateButton => this.Q<Button>("generate");
        VisualElement sendingRequest => this.Q<VisualElement>("sendingRequest");
        VisualElement requestSent => this.Q<VisualElement>("requestSent");
        VisualElement requestFailed => this.Q<VisualElement>("requestFailed");
        ImageSelection image => this.Q<ImageSelection>("image");
        Label imageRequired => this.Q<Label>("imageRequired");
        EnumField initImageMode => this.Q<EnumField>("initImageMode");
        VisualElement imageStrengthModeContainer => this.Q<VisualElement>("imageStrengthModeContainer");
        VisualElement stepScheduleModeContainer => this.Q<VisualElement>("stepScheduleModeContainer");
        Slider imageStrength => this.Q<Slider>("imageStrength");
        Slider stepScheduleStart => this.Q<Slider>("stepScheduleStart");
        Slider stepScheduleEnd => this.Q<Slider>("stepScheduleEnd");
        Toggle sendStepScheduleEnd => this.Q<Toggle>("sendStepScheduleEnd");
        TextField code => this.Q<TextField>("code");
        StabilityParametersElement stabilityParameters => this.Q<StabilityParametersElement>("stabilityParameters");
        GenerationOptionsElement generationOptions => this.Q<GenerationOptionsElement>("generationOptions");

        public ImageToImage()
        {
            stabilityParameters.OnCodeChanged += RefreshCode;
            generationOptions.OnCodeChanged += RefreshCode;

            imageStrength.RegisterValueChangedCallback(_ => RefreshCode());
            stepScheduleStart.RegisterValueChangedCallback(_ => RefreshCode());
            stepScheduleEnd.RegisterValueChangedCallback(_ => RefreshCode());

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

            initImageMode.RegisterValueChangedCallback(_ =>
            {
                switch ((InitImageMode)initImageMode.value)
                {
                    case InitImageMode.ImageStrength:
                        imageStrengthModeContainer.style.display = DisplayStyle.Flex;
                        stepScheduleModeContainer.style.display = DisplayStyle.None;
                        break;
                    case InitImageMode.StepSchedule:
                        imageStrengthModeContainer.style.display = DisplayStyle.None;
                        stepScheduleModeContainer.style.display = DisplayStyle.Flex;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RefreshCode();
            });
            
            sendStepScheduleEnd.RegisterValueChangedCallback(evt =>
            {
                stepScheduleEnd.SetEnabled(evt.newValue);
                RefreshCode();
            });
            stepScheduleEnd.SetEnabled(sendStepScheduleEnd.value);

            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;
                imageRequired.style.visibility = Visibility.Hidden;
                if (!stabilityParameters.Valid())
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

                var imageMode = (InitImageMode)initImageMode.value;
                
                var parameters = new StabilityImageToImageParameters
                {
                    EngineId = engine.value,
                    InitImage = (Texture2D)image.image,
                    InitImageMode = imageMode,
                    StepScheduleStart = imageMode == InitImageMode.StepSchedule ? stepScheduleStart.value : null,
                    StepScheduleEnd = (imageMode == InitImageMode.StepSchedule && sendStepScheduleEnd.value) ? stepScheduleEnd.value : null,
                    ImageStrength = imageMode == InitImageMode.ImageStrength ? imageStrength.value : null,
                };
                stabilityParameters.ApplyParameters(parameters);
                ContentGenerationApi.Instance.RequestStabilityImageToImageGeneration(
                    parameters,
                    generationOptions.GetGenerationOptions(), data: new
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

            engine.RegisterValueChangedCallback(_ => RefreshCode());

            code.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
            RefreshCode();
        }

        void RefreshCode()
        {
            code.value =
                "var requestId = await ContentGenerationApi.Instance.RequestImageToImageGeneration\n" +
                "\t(new StabilityImageToImageParameters\n" +
                "\t{\n" +
                $"\t\tEngineId = \"{engine.value}\",\n" +
                $"\t\tInitImage = <Texture2D object>,\n" +
                $"\t\tInitImageMode = {(InitImageMode)initImageMode.value},\n" +
                ((InitImageMode)initImageMode.value == InitImageMode.ImageStrength
                    ? $"\t\tImageStrength = {imageStrength.value},\n"
                    : $"\t\tStepScheduleStart = {stepScheduleStart.value},\n" +
                      (sendStepScheduleEnd.value ? $"\t\tStepScheduleEnd = {stepScheduleEnd.value},\n" : "")) +
                stabilityParameters.GetCode() +
                "\t},\n" +
                $"{generationOptions.GetCode()}" +
                ")";
        }
    }
}