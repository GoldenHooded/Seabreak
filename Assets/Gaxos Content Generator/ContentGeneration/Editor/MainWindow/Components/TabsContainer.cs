using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components
{
    public class TabsContainer : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<TabsContainer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield return new UxmlChildElementDescription(typeof(Tab)); }
            }
        }

        public override VisualElement contentContainer => this.Q<VisualElement>("contentContainer");
        VisualElement tabToggles => this.Q<VisualElement>("tabToggles");

        readonly HashSet<string> createdTabs = new();

        public TabsContainer()
        {
            RegisterCallback<AttachToPanelEvent>(e =>
            {
                RadioButton firstRadioButton = null;
                foreach (var visualElement in contentContainer!.Children())
                {
                    if (visualElement is Tab t)
                    {
                        if (createdTabs.Add(t.tabName))
                        {
                            var tabToggle = new RadioButton(t.tabName);
                            if (firstRadioButton == null)
                            {
                                firstRadioButton = tabToggle;
                            }
                            tabToggle.RegisterValueChangedCallback(v =>
                            {
                                visualElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                            });
                            tabToggles.Add(tabToggle);
                            visualElement.style.display = DisplayStyle.None;
                        }
                    }
                }

                if (firstRadioButton != null)
                {
                    firstRadioButton.value = true;
                }
            });
        }
    }
}