using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImageChanger))]
public class ImageChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Llamar a la implementación predeterminada del Inspector
        DrawDefaultInspector();

        // Obtener una referencia al script ImageChanger
        ImageChanger imageChanger = (ImageChanger)target;

        // Añadir un espacio y un botón personalizado
        GUILayout.Space(10);
        if (GUILayout.Button("Change Image"))
        {
            // Llamar a la función ChangeImage cuando el botón sea presionado
            imageChanger.ChangeImage();
        }
    }
}
