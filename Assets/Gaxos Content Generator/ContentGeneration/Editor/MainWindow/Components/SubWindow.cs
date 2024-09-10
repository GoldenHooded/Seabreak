using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components
{
    public class SubWindow : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<SubWindow, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription _subWindowName = new() { name = "Sub-Window-Name" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get
                {
                    yield return new UxmlChildElementDescription(typeof (VisualElement));
                }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (SubWindow)ve;
                element.subWindowName = _subWindowName.GetValueFromBag(bag, cc);
            }
        }
        
        Label label => this.Q<Label>("label");
        VisualElement icon => this.Q<VisualElement>("icon");
        VisualElement contents => this.Q<VisualElement>("contents");

        public string subWindowName
        {
            get => label.text;
            set
            {
                label.text = value;
                icon.style.backgroundImage = new StyleBackground(
                    AssetDatabase.LoadAssetAtPath<Sprite>(
                        System.IO.Path.Combine(componentsBasePath, $"MainWindow/{value}.png")));
            }
        }

        public void SetSubWindowNameAndIcon(string subWindowName, string iconName)
        {
            label.text = subWindowName;
            if (string.IsNullOrEmpty(iconName))
            {
                icon.style.backgroundImage = null;
            }
            else
            {
                icon.style.backgroundImage = new StyleBackground(
                    AssetDatabase.LoadAssetAtPath<Sprite>(
                        System.IO.Path.Combine(componentsBasePath, $"MainWindow/{iconName}.png")));
            }
        }
        
        public override VisualElement contentContainer => contents;
    }
}
