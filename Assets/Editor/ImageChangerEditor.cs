using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImageChanger))]
public class ImageChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Llamar a la implementaci�n predeterminada del Inspector
        DrawDefaultInspector();

        // Obtener una referencia al script ImageChanger
        ImageChanger imageChanger = (ImageChanger)target;

        // A�adir un espacio y un bot�n personalizado
        GUILayout.Space(10);
        if (GUILayout.Button("Change Image"))
        {
            // Llamar a la funci�n ChangeImage cuando el bot�n sea presionado
            imageChanger.ChangeImage();
        }
    }
}
