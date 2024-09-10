using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentGeneration.Editor.MainWindow.Components.Meshy;
using ContentGeneration.Helpers;
using ContentGeneration.Models;
using UnityEngine;
using UnityEngine.UIElements;
using QueryParameters = ContentGeneration.Models.QueryParameters;

namespace ContentGeneration.Editor.MainWindow.Components.RequestsList
{
    public class RequestsListTab : VisualElementComponent
    {
        public new class UxmlFactory : UxmlFactory<RequestsListTab, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

        Button refreshButton => this.Q<Button>("refreshButton");
        MultiColumnListView listView => this.Q<MultiColumnListView>();
        IRequestedItem defaultRequestedItem => this.Q<RequestedItem>("defaultRequestedItem");
        IRequestedItem meshyTextToMeshRequestedItem => this.Q<MeshyTextToMeshRequestedItem>("meshyTextToMeshRequestedItem");
        IRequestedItem meshyTextToTextureRequestedItem => this.Q<MeshyTextToTextureRequestedItem>("meshyTextToTextureRequestedItem");

        IRequestedItem[] allRequestedItems => new[]
        {
            defaultRequestedItem, meshyTextToMeshRequestedItem, meshyTextToTextureRequestedItem
        };

        string _selectedId;
        public RequestsListTab()
        {
            refreshButton.RegisterCallback<ClickEvent>(_ => { Refresh(); });

            ContentGenerationStore.Instance.OnRequestsChanged += _ =>
            {
                var previousSelectId = _selectedId;
                listView.RefreshItems();
                listView.selectedIndex = -1;
                if(previousSelectId != null)
                {
                    for (var i = 0; i < ContentGenerationStore.Instance.Requests.Count; i++)
                    {
                        if (ContentGenerationStore.Instance.Requests[i].ID == previousSelectId)
                        {
                            listView.selectedIndex = i;
                            break;
                        }
                    }
                }
            };
            listView.itemsSource = ContentGenerationStore.Instance.Requests;
            if(!string.IsNullOrEmpty(Settings.instance.apiKey))
            {
                Refresh();
            }

            Func<VisualElement> CreateCell(int i1)
            {
                return () =>
                {
                    var ret = new Label
                    {
                        name = $"p{i1}"
                    };
                    ret.AddToClassList(ret.name);

                    return ret;
                };
            }

            for (var i = 0; i < listView.columns.Count; i++)
            {
                var listViewColumn = listView.columns[i];
                listViewColumn.makeCell = CreateCell(i);
            }

            listView.columnSortingChanged += Refresh;
            listView.columns["id"].bindCell = (element, index) =>
                (element as Label)!.text = ContentGenerationStore.Instance.Requests[index].ID.ToString();
            listView.columns["generator"].bindCell = (element, index) =>
                (element as Label)!.text = ContentGenerationStore.Instance.Requests[index].Generator.ToString();
            listView.columns["timeTaken"].bindCell = (element, index) =>
            {
                var completedAt = ContentGenerationStore.Instance.Requests[index].CompletedAt;
                var createdAt = ContentGenerationStore.Instance.Requests[index].CreatedAt;
                if(completedAt < createdAt)
                {
                    completedAt = DateTime.UtcNow;
                }
                (element as Label)!.text = $"{(completedAt - createdAt).TotalSeconds:0.} seconds";
            };
            listView.columns["created"].bindCell = (element, index) =>
                (element as Label)!.text = ContentGenerationStore.Instance.Requests[index].CreatedAt.ToString(CultureInfo.InvariantCulture);
            listView.columns["completed"].bindCell = (element, index) =>
            {
                if(ContentGenerationStore.Instance.Requests[index].CompletedAt > DateTime.UnixEpoch)
                {
                    (element as Label)!.text = ContentGenerationStore.Instance.Requests[index].CompletedAt
                        .ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    (element as Label)!.text = "---";
                }
            };
            listView.columns["status"].bindCell = (element, index) =>
            {
                var label = (element as Label)!;
                label.text = ContentGenerationStore.Instance.Requests[index].Status.ToString();
                label.RemoveFromClassList("generated");
                label.RemoveFromClassList("failed");
                label.AddToClassList(ContentGenerationStore.Instance.Requests[index].Status.ToString().ToLower());
            };

            foreach (var requestedItem in allRequestedItems)
            {
                requestedItem.OnDeleted += () =>
                {
                    listView.selectedIndex = -1;
                    Refresh();
                };
                requestedItem.style.display = DisplayStyle.None;
            }
            listView.selectionChanged += objects =>
            {
                _selectedId = null;
                var objectsArray = objects.ToArray();
                foreach (var requestedItem in allRequestedItems)
                {
                    requestedItem.value = null;
                }
                if (objectsArray.Length > 0)
                {
                    var request = (objectsArray[0] as Request)!;
                    _selectedId = request.ID;
                    if (request.Generator == Generator.MeshyTextToMesh)
                    {
                        meshyTextToMeshRequestedItem.value = request;
                    }
                    else if (request.Generator == Generator.MeshyTextToTexture)
                    {
                        meshyTextToTextureRequestedItem.value = request;
                    }
                    else
                    {
                        defaultRequestedItem.value = request;
                    }
                }
                foreach (var requestedItem in allRequestedItems)
                {
                    requestedItem.style.display =
                        requestedItem.value == null ? DisplayStyle.None : DisplayStyle.Flex;
                }
            };
        }

        void Refresh()
        {
            ContentGenerationStore.Instance.sortBy = null;
            
            if (listView.sortColumnDescriptions.Count > 0)
            {
                var sortColumnDescription = listView.sortColumnDescriptions[0];
                if (sortColumnDescription.columnName == "id")
                {
                    ContentGenerationStore.Instance.sortBy = new QueryParameters.SortBy()
                    {
                        Target = QueryParameters.SortTarget.Id,
                        Direction = sortColumnDescription.direction == SortDirection.Ascending
                            ? QueryParameters.SortDirection.Ascending
                            : QueryParameters.SortDirection.Descending
                    };
                }
                else if (sortColumnDescription.columnName == "created")
                {
                    ContentGenerationStore.Instance.sortBy = new QueryParameters.SortBy()
                    {
                        Target = QueryParameters.SortTarget.CreatedAt,
                        Direction = sortColumnDescription.direction == SortDirection.Ascending
                            ? QueryParameters.SortDirection.Ascending
                            : QueryParameters.SortDirection.Descending
                    };
                }
                else
                {
                    listView.sortColumnDescriptions.Clear();
                }
            }
            ContentGenerationStore.Instance.RefreshRequestsAsync().CatchAndLog();
        }
    }
}