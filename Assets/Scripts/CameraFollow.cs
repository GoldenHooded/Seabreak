using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    public LayerMask layerMask; // Máscara de capa para el Raycast
    public float smoothSpeed = 0.125f; // Velocidad de suavizado
    public float smoothSpeed2 = 1; // Velocidad de suavizado

    private Vector3 startLocalPos;

    [HideInInspector] public Transform target; // El transform que la cámara intentará seguir

    void Start()
    {
        StartCoroutine(StartThen());
    }

    private IEnumerator StartThen()
    {
        yield return null;
        yield return null;
        yield return null;
        // Crear un nuevo GameObject como transform objetivo
        GameObject targetObject = new("CameraTarget");
        targetObject.transform.position = transform.position;
        targetObject.transform.parent = transform.parent;

        // Quitar el parent de la cámara
        transform.parent = null;

        // Asignar el transform del nuevo GameObject como objetivo de la cámara
        target = targetObject.transform;

        startLocalPos = target.localPosition;

        yield break;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Dirección desde la cámara hacia el objetivo
        Vector3 direction = target.position - transform.position;

        // Realizar un Raycast hacia el objetivo
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, direction.magnitude, layerMask))
        {
            // Si colisiona, moverse hacia el punto de colisión
            Vector3 desiredPosition = hit.point;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Si no colisiona, moverse hacia el transform objetivo
            Vector3 desiredPosition = target.position;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        target.localPosition = Vector3.Lerp(target.localPosition, startLocalPos, smoothSpeed2 * Time.deltaTime);
    }
}
