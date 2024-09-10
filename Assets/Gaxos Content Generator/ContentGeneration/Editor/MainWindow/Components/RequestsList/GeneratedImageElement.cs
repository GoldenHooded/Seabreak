using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ContentGeneration.Models;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.RequestsList
{
    public class GeneratedImageElement : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<GeneratedImageElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        Image image => this.Q<Image>("image");
        Button saveToProject => this.Q<Button>("saveToProject");

        public GeneratedImageElement() : this(null)
        {
        }

        public GeneratedImageElement(GeneratedAsset generatedAsset)
        {
            this.image.image = null;
            this.image.AddManipulator(new Clickable(evt =>
            {
                Application.OpenURL(generatedAsset.URL);
            }));
            saveToProject.SetEnabled(false);
            EditorCoroutineUtility.StartCoroutine(
                LoadImage(generatedAsset.URL), this);
            
            saveToProject.RegisterCallback<ClickEvent>(_ =>
            {
                if (!saveToProject.enabledSelf || this.image.image == null) return;
                
                var path = EditorUtility.SaveFilePanel(
                    "Save texture as PNG",
                    "Assets/",
                    "",
                    "png");

                if (path.Length == 0) return;
                
                var pngData = ((Texture2D)this.image.image).EncodeToPNG();
                if (pngData != null)
                {
                    File.WriteAllBytes(path, pngData);
                    AssetDatabase.Refresh();
                }
            });
        }

        IEnumerator LoadImage(string imageUrl)
        {
            var www = UnityWebRequestTexture.GetTexture(imageUrl);
            www.SetRequestHeader("Authorization", $"Bearer {Settings.instance.apiKey}");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"{www.error}: {www.downloadHandler?.text}");
            }

            var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            image.image = texture;
            saveToProject.SetEnabled(true);
        }
    }
}