using System;
using ContentGeneration.Models;
using UnityEngine.UIElements;

namespace ContentGeneration.Editor.MainWindow.Components.RequestsList
{
    public interface IRequestedItem
    {
        event Action OnDeleted;
        IStyle style { get; }
        Request value { get; set; }
    }
}