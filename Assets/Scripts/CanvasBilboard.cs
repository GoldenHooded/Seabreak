using UnityEngine;

public class CanvasBilboard : MonoBehaviour
{
    private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Obtén una referencia a la cámara principal
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Hacer que el objeto siempre mire hacia la cámara
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}
