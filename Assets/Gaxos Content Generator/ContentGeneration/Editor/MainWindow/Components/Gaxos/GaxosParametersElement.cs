using System;
using System.Collections.Generic;
using System.Linq;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Gaxos;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.Gaxos
{
    public class GaxosParametersElement : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<GaxosParametersElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        PromptInput prompt => this.Q<PromptInput>("prompt");
        VisualElement promptRequired => this.Q<VisualElement>("promptRequired");
        PromptInput negativePrompt => this.Q<PromptInput>("negativePrompt");

        TextField checkpoint => this.Q<TextField>("checkpoint");
        SliderInt nSamples => this.Q<SliderInt>("nSamples");
        Toggle sendSeed => this.Q<Toggle>("sendSeed");
        SliderInt seed => this.Q<SliderInt>("seed");
        SliderInt steps => this.Q<SliderInt>("steps");
        Slider cfg => this.Q<Slider>("cfg");
        TextField sampler => this.Q<TextField>("sampler");
        TextField scheduler => this.Q<TextField>("scheduler");
        Slider denoise => this.Q<Slider>("denoise");
        
        TextField loras => this.Q<TextField>("loras");
        
        public GaxosParametersElement()
        {
            prompt.OnChanged += _=> RefreshCode();
            var improvePromptButton = this.Q<Button>("improvePromptButton");
            improvePromptButton.clicked += () =>
            {
                if (string.IsNullOrEmpty(prompt.value))
                    return;

                if (!improvePromptButton.enabledSelf)
                    return;

                improvePromptButton.SetEnabled(false);
                prompt.SetEnabled(false);
                ContentGenerationApi.Instance.ImprovePrompt(prompt.value, "midjourney").ContinueInMainThreadWith(
                    t =>
                    {
                        improvePromptButton.SetEnabled(true);
                        prompt.SetEnabled(true);

                        if (t.IsFaulted)
                        {
                            Debug.LogException(t.Exception!.InnerException!);
                            return;
                        }

                        prompt.value = t.Result;
                    });
            };
            
            negativePrompt.OnChanged += _  => RefreshCode();
            checkpoint.RegisterValueChangedCallback(_=> RefreshCode());
            nSamples.RegisterValueChangedCallback(_=> RefreshCode());
            sendSeed.RegisterValueChangedCallback(evt =>
            {
                seed.SetEnabled(evt.newValue);
                RefreshCode();
            });
            seed.SetEnabled(sendSeed.value);
            seed.RegisterValueChangedCallback(_=> RefreshCode());
            steps.RegisterValueChangedCallback(_=> RefreshCode());
            cfg.RegisterValueChangedCallback(_=> RefreshCode());
            sampler.RegisterValueChangedCallback(_=> RefreshCode());
            scheduler.RegisterValueChangedCallback(_=> RefreshCode());
            denoise.RegisterValueChangedCallback(_=> RefreshCode());
            loras.RegisterValueChangedCallback(_=> RefreshCode());
        }

        public event Action OnCodeChanged;

        void RefreshCode()
        {
            OnCodeChanged?.Invoke();
        }

        public bool Valid()
        {
            var thereArePrompts = !string.IsNullOrEmpty(prompt.value);

            promptRequired.style.visibility = thereArePrompts ? Visibility.Hidden : Visibility.Visible;

            return !string.IsNullOrEmpty(prompt.value);
        }

        public string GetCode()
        {
            var code =
                $"\t\tPrompt = \"{prompt.value}\",\n" +
                (!string.IsNullOrWhiteSpace(negativePrompt.value) ? $"\t\tNegativePrompt = \"{negativePrompt.value}\",\n" : null) +
                (!string.IsNullOrWhiteSpace(checkpoint.value) ? $"\t\tCheckpoint = \"{checkpoint.value}\",\n" : null) +
                $"\t\tNSamples = {nSamples.value},\n" +
                (sendSeed.value ? $"\t\tSeed = {seed.value},\n" : null) +
                $"\t\tSteps = {steps.value},\n" +
                $"\t\tCfg = {cfg.value},\n" +
                (!string.IsNullOrWhiteSpace(sampler.value) ? $"\t\tSamplerName = \"{sampler.value}\",\n" : null) +
                (!string.IsNullOrWhiteSpace(scheduler.value) ? $"\t\tScheduler = \"{scheduler.value}\",\n" : null) +
                $"\t\tDenoise = {denoise.value}" +
                (!string.IsNullOrWhiteSpace(loras.value) ? 
                    $"\t\tLoras = [{string.Join(", ", loras.value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => $"\"{i.Trim()}\""))}]" : null) +
                "";
            return code;
        }

        public void ApplyParameters(GaxosParameters gaxosParameters)
        {
            gaxosParameters.Prompt = prompt.value;
            gaxosParameters.NegativePrompt = string.IsNullOrWhiteSpace(negativePrompt.value) ? null : negativePrompt.value;
            gaxosParameters.Checkpoint = string.IsNullOrWhiteSpace(checkpoint.value) ? null : checkpoint.value;
            gaxosParameters.NSamples = (uint)nSamples.value;
            gaxosParameters.Seed = !sendSeed.value ? null : seed.value;
            gaxosParameters.Steps = (uint)steps.value;
            gaxosParameters.Cfg = cfg.value;
            gaxosParameters.SamplerName = string.IsNullOrWhiteSpace(sampler.value) ? null : sampler.value;
            gaxosParameters.Scheduler = string.IsNullOrWhiteSpace(scheduler.value) ? null : scheduler.value;
            gaxosParameters.Denoise = denoise.value;
            gaxosParameters.Loras = string.IsNullOrWhiteSpace(loras.value) ? null : 
                loras.value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
        }
    }
}