using System;
using System.Collections.Generic;
using System.Linq;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Stability;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.StabilityAI
{
    public class TextToImage : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<TextToImage, UxmlTraits>
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
        DropdownField engine => this.Q<DropdownField>("engine");
        DropdownField width => this.Q<DropdownField>("width");
        DropdownField height => this.Q<DropdownField>("height");
        Button generateButton => this.Q<Button>("generate");
        VisualElement sendingRequest => this.Q<VisualElement>("sendingRequest");
        VisualElement requestSent => this.Q<VisualElement>("requestSent");
        VisualElement requestFailed => this.Q<VisualElement>("requestFailed");
        StabilityParametersElement stabilityParameters => this.Q<StabilityParametersElement>("stabilityParameters");
        GenerationOptionsElement generationOptions => this.Q<GenerationOptionsElement>("generationOptions");

        public TextToImage()
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

            string[] GetPossibleResolutions(string engineId)
            {
                uint xMin;
                uint xMax;
                uint yMin;
                uint yMax;
                switch (engineId)
                {
                    case "stable-diffusion-xl-1024-v0-9":
                    case "stable-diffusion-xl-1024-v1-0":
                        return new[]
                        {
                            "1024x1024",
                            "1152x896",
                            "1216x832",
                            "1344x768",
                            "1536x640",
                            "640x1536",
                            "768x1344",
                            "832x1216",
                            "896x1152"
                        };
                    case "stable-diffusion-v1-6":
                        xMin = 320;
                        xMax = 1536;
                        yMin = 320;
                        yMax = 1536;
                        break;
                    case "stable-diffusion-xl-beta-v2-2-2":
                        xMin = 128;
                        xMax = 512;
                        yMin = 128;
                        yMax = 512;
                        break;
                    case "esrgan-v1-x2plus":
                    case "stable-diffusion-512-v2-1":
                        xMin = 128;
                        xMax = 1536;
                        yMin = 128;
                        yMax = 1536;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(StabilityTextToImageParameters.EngineId), engineId);
                }

                var ret = new List<string>();
                for (var x = xMin; x <= xMax; x += 64)
                {
                    for (var y = yMin; y <= yMax; y += 64)
                    {
                        ret.Add($"{x}x{y}");
                    }
                }

                if (engineId == "stable-diffusion-xl-beta-v2-2-2")
                {
                    for (var i = 512 + 64; i <= 896; i += 64)
                    {
                        ret.Add($"512x{i}");
                        ret.Add($"{i}x512");
                    }
                }

                return ret.ToArray();
            }

            void RefreshResolutions(string newWidth = null, string newHeight = null)
            {
                var possibleResolutions = GetPossibleResolutions(engine.value);
                var widths = new HashSet<string>(possibleResolutions.
                    Select(i => i.Split('x')[0])).ToList();
                var heights = new HashSet<string>(possibleResolutions.
                        Select(i => i.Split('x')[1])).ToList();
                width.choices = widths;
                height.choices = heights;

                if (newWidth != null && !widths.Contains(newWidth))
                {
                    RefreshResolutions(null, newHeight);
                    return;
                }

                if (newHeight != null && !heights.Contains(newHeight))
                {
                    RefreshResolutions(newWidth, null);
                    return;
                }

                if (newWidth == null && newHeight == null)
                {
                    newWidth = widths[0]; 
                }

                if (newWidth != null)
                {
                    width.value = newWidth;
                    
                    var possibleHeights = new HashSet<string>(
                        possibleResolutions.
                            Where(i => i.StartsWith($"{width.value}x")).
                            Select(i => i.Split('x')[1])).ToList();
                    if (newHeight == null)
                    {
                        newHeight = height.value;
                    }
                    if (newHeight == null || !possibleHeights.Contains(newHeight))
                    {
                        newHeight = possibleHeights[0];
                    }
                    height.value = newHeight;
                }
                else
                {
                    height.value = newHeight;

                    var possibleWidths = (new HashSet<string>(
                        possibleResolutions.
                            Where(i => i.EndsWith($"x{height.value}")).
                            Select(i => i.Split('x')[0]))).ToList();
                    if (!possibleWidths.Contains(width.value))
                    {
                        width.value = possibleWidths[^1];
                    }
                }
            }

            engine.RegisterValueChangedCallback(_ =>
            {
                RefreshResolutions(width.value, height.value);
            });
            RefreshResolutions();

            sendingRequest.style.display = DisplayStyle.None;
            requestSent.style.display = DisplayStyle.None;
            requestFailed.style.display = DisplayStyle.None;

            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;
                if (!stabilityParameters.Valid())
                {
                    return;
                }

                generateButton.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                var parameters = new StabilityTextToImageParameters
                {
                    EngineId = engine.value,
                    Width = uint.Parse(width.value),
                    Height = uint.Parse(height.value),
                };
                stabilityParameters.ApplyParameters(parameters);
                ContentGenerationApi.Instance.RequestStabilityTextToImageGeneration(
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
            width.RegisterValueChangedCallback(evt =>
            {
                RefreshResolutions(evt.newValue, null);
                RefreshCode();
            });
            height.RegisterValueChangedCallback(evt =>
            {
                RefreshResolutions(null, evt.newValue);
                RefreshCode();
            });

            code.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
            RefreshCode();
        }

        void RefreshCode()
        {
            code.value =
                "var requestId = await ContentGenerationApi.Instance.RequestTextToImageGeneration\n" +
                "\t(new StabilityTextToImageParameters\n" +
                "\t{\n" +
                $"\t\tEngineId = \"{engine.value}\",\n" +
                $"\t\tWidth = {uint.Parse(width.value)},\n" +
                $"\t\tHeight = {uint.Parse(height.value)},\n" +
                stabilityParameters.GetCode() +
                "\t},\n" +
                $"{generationOptions.GetCode()}" +
                ")";
        }
    }
}