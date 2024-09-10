using System.Collections.Generic;
using System.IO;
using ContentGeneration.Helpers;
using ContentGeneration.Models.Meshy;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Resolution = ContentGeneration.Models.Meshy.Resolution;

namespace ContentGeneration.Editor.MainWindow.Components.Meshy
{
    public class TextToTexture : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<TextToTexture, UxmlTraits>
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
        GenerationOptionsElement generationOptionsElement => this.Q<GenerationOptionsElement>("generationOptions");
        VisualElement requestSent => this.Q<VisualElement>("requestSent");
        VisualElement requestFailed => this.Q<VisualElement>("requestFailed");
        VisualElement sendingRequest => this.Q<VisualElement>("sendingRequest");
        Button generateButton => this.Q<Button>("generateButton");

        ObjectField model => this.Q<ObjectField>("model");
        VisualElement modelRequired => this.Q<VisualElement>("modelRequired");
        PromptInput objectPrompt => this.Q<PromptInput>("objectPrompt");
        VisualElement objectPromptRequired => this.Q<VisualElement>("objectPromptRequired");
        PromptInput stylePrompt => this.Q<PromptInput>("stylePrompt");
        VisualElement stylePromptRequired => this.Q<VisualElement>("stylePromptRequired");

        PromptInput negativePrompt => this.Q<PromptInput>("negativePrompt");
        Toggle enableOriginalUv => this.Q<Toggle>("enableOriginalUv");
        Toggle enablePbr => this.Q<Toggle>("enablePbr");
        EnumField resolution => this.Q<EnumField>("resolution");
        
        EnumField artStyle => this.Q<EnumField>("artStyle");

        byte[] _modelBytes;
        string _modelExtension;
        
        Button improvePrompt => this.Q<Button>("improvePromptButton");

        public TextToTexture()
        {
            generationOptionsElement.OnCodeChanged += RefreshCode;

            model.RegisterValueChangedCallback(v =>
            {
                _modelBytes = null;
                _modelExtension = null;
                if(v.newValue!= null)
                {
                    var path = AssetDatabase.GetAssetPath(v.newValue);
                    _modelBytes = File.ReadAllBytes(path);
                    _modelExtension = Path.GetExtension(path).TrimStart('.');
                }
                RefreshCode();
            });
            
            objectPrompt.OnChanged += _ => RefreshCode();
            stylePrompt.OnChanged += _ => RefreshCode();
            
            negativePrompt.OnChanged += _ => RefreshCode();

            enableOriginalUv.RegisterValueChangedCallback(_ => RefreshCode());
            enablePbr.RegisterValueChangedCallback(_ => RefreshCode());
            resolution.RegisterValueChangedCallback(_ => RefreshCode());
            
            artStyle.RegisterValueChangedCallback(_ => RefreshCode());

            requestSent.style.display = DisplayStyle.None;
            requestFailed.style.display = DisplayStyle.None;
            sendingRequest.style.display = DisplayStyle.None;

            improvePrompt.clicked += () =>
            {
                if (string.IsNullOrEmpty(stylePrompt.value))
                    return;
                
                if(!improvePrompt.enabledSelf)
                    return;

                improvePrompt.SetEnabled(false);
                stylePrompt.SetEnabled(false);

                ContentGenerationApi.Instance.ImprovePrompt(stylePrompt.value, "dalle-3").ContinueInMainThreadWith(
                    t =>
                    {
                        improvePrompt.SetEnabled(true);
                        stylePrompt.SetEnabled(true);
                        if (t.IsFaulted)
                        {
                            Debug.LogException(t.Exception!.InnerException!);
                            return;
                        }

                        stylePrompt.value = t.Result;
                    });
            };
            
            generateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (!generateButton.enabledSelf) return;

                if (_modelBytes == null)
                {
                    modelRequired.style.visibility = Visibility.Visible;
                    return;
                }
                modelRequired.style.visibility = Visibility.Hidden;

                if (string.IsNullOrEmpty(objectPrompt.value))
                {
                    objectPromptRequired.style.visibility = Visibility.Visible;
                    return;
                }
                objectPromptRequired.style.visibility = Visibility.Hidden;
                if (string.IsNullOrEmpty(stylePrompt.value))
                {
                    stylePromptRequired.style.visibility = Visibility.Visible;
                    return;
                }
                stylePromptRequired.style.visibility = Visibility.Hidden;

                requestSent.style.display = DisplayStyle.None;
                requestFailed.style.display = DisplayStyle.None;

                generateButton.SetEnabled(false);
                sendingRequest.style.display = DisplayStyle.Flex;

                var parameters = new MeshyTextToTextureParameters
                {
                    Model = _modelBytes,
                    ModelExtension = _modelExtension,
                    ObjectPrompt = objectPrompt.value,
                    StylePrompt = stylePrompt.value,
                    NegativePrompt = string.IsNullOrEmpty(negativePrompt.value) ? null : negativePrompt.value,
                    EnableOriginalUV = enableOriginalUv.value,
                    EnablePbr = enablePbr.value,
                    Resolution = (Resolution)resolution.value,
                    ArtStyle = (TextToTextureArtStyle)artStyle.value
                };
                ContentGenerationApi.Instance.RequestMeshyTextToTextureGeneration(
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
                "var requestId = await ContentGenerationApi.Instance.RequestMeshyTextToTextureGeneration\n" +
                "\t(new MeshyTextToTextureParameters\n" +
                "\t{\n" +
                $"\t\tModel = <Model bytes>,\n" +
                $"\t\tModel = \"{_modelExtension}\",\n" +
                $"\t\tObjectPrompt = \"{objectPrompt.value}\",\n" +
                $"\t\tStylePrompt = \"{stylePrompt.value}\",\n" +
                (string.IsNullOrEmpty(negativePrompt.value) ? "" : $"\t\tNegativePrompt = \"{negativePrompt.value}\",\n") +
                $"\t\tEnableOriginalUV = {enableOriginalUv.value},\n" +
                $"\t\tEnablePbr = {enablePbr.value},\n" +
                $"\t\tResolution = Resolution.{resolution.value},\n" +
                $"\t\tArtStyle = ArtStyle.{artStyle.value}\n" +
                "\t},\n" +
                $"{generationOptionsElement?.GetCode()}" +
                ")";
        }
    }
}