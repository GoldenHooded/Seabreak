using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class NPC : MonoBehaviour
{
    // Parámetros de control
    public float startDecisionInteval = 2f; // Tiempo entre decisiones
    public bool ignoreRandomInteval = false; // Tiempo entre decisiones
    private float decisionInterval = 2f; // Tiempo entre decisiones
    public float moveProbability = 0.5f; // Probabilidad de moverse
    public float turnProbability = 0.3f; // Probabilidad de girar sin moverse
    public float gridDistance = 1f; // Distancia a recorrer por movimiento
    public float moveDurationPerTile = 0.25f; // Duración de movimiento por casilla
    public LayerMask obstacleLayerMask; // Detección de colisiones
    public LayerMask groundLayerMask; // Detección de colisiones
    public float groundYOffset;
    public float boxSize = 0.9f;

    // Variables para el estado del NPC
    [SerializeField] private bool isMoving = false;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Direction lastDirection; // Última dirección
    [SerializeField] private float decisionTimer;

    // Animator
    public Animator anim;

    // Enum para direcciones
    public enum Direction
    {
        Forward,
        Backward,
        Right,
        Left
    }

    private void Start()
    {
        // Estado inicial
        lastDirection = Direction.Forward;
        if (!ignoreRandomInteval)
            decisionInterval = Random.Range(startDecisionInteval - 2, startDecisionInteval + 2);
        else
            decisionInterval = startDecisionInteval;

        isMoving = false;
        if (targetObject != null) Destroy(targetObject);
        StartCoroutine(NPCBehaviorRoutine());
    }

    private void OnEnable()
    {
        if (!ignoreRandomInteval)
            decisionInterval = Random.Range(startDecisionInteval - 2, startDecisionInteval + 2);
        else
            decisionInterval = startDecisionInteval;

        if (targetObject != null) Destroy(targetObject);
        isMoving = false;

        StopAllCoroutines(); // Detiene cualquier corrutina previa (por si acaso)
        StartCoroutine(NPCBehaviorRoutine()); // Reinicia la corrutina principal
    }


    private void Update()
    {
        MoveNPC();
        HandleAnimations();
        AlignPlayerWithGround();
    }

    private void LateUpdate()
    {
        anim.SetBool("Walk", isMoving);
    }

    private IEnumerator NPCBehaviorRoutine()
    {
        while (true)
        {
            if (!isMoving)
            {
                // Tomar una nueva decisión después de un intervalo
                decisionTimer = 0f;
                while (decisionTimer < decisionInterval)
                {
                    decisionTimer += Time.deltaTime;
                    yield return null;
                }

                // Determinar si el NPC se mueve o cambia de dirección sin moverse
                float randomValue = Random.value;

                if (randomValue < moveProbability)
                {
                    // Mover al NPC
                    TryMove();
                }
                else if (randomValue < moveProbability + turnProbability)
                {
                    // Cambiar de dirección sin moverse
                    TurnWithoutMoving();
                }

                if (!ignoreRandomInteval)
                    decisionInterval = Random.Range(startDecisionInteval - 2, startDecisionInteval + 2);
                else
                    decisionInterval = startDecisionInteval;
            }

            yield return null;
        }
    }

    private void TryMove()
    {
        moveDirection = GetRandomDirection();

        if (moveDirection != Vector3.zero && !CheckCollisionInDirection(moveDirection))
        {
            // Crear el objeto objetivo en la posición deseada
            Vector3 targetPosition = transform.localPosition + moveDirection.normalized * gridDistance;

            // Redondear la posición al valor más cercano en incrementos de 0.5
            targetPosition = new Vector3(
                Mathf.Round(targetPosition.x * 2f) / 2f,
                targetPosition.y,
                Mathf.Round(targetPosition.z * 2f) / 2f
            );

            targetObject = new GameObject("TargetObject");
            targetObject.transform.parent = transform.parent;
            targetObject.transform.localPosition = targetPosition;

            isMoving = true;
        }
    }

    private void MoveNPC()
    {
        if (targetObject != null)
        {
            if (Vector3.Distance(transform.position, targetObject.transform.position) > gridDistance * 4f)
            {
                Destroy(targetObject);
                targetObject = null;

                isMoving = false;

                TryMove();
            }
        }

        if (isMoving && targetObject != null)
        {
            // Mover el NPC hacia el objeto objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetObject.transform.position, gridDistance / moveDurationPerTile * Time.deltaTime);

            // Comprobar si ha llegado a la casilla
            if (Vector3.Distance(transform.position, targetObject.transform.position) < 0.01f)
            {
                transform.position = targetObject.transform.position;
                isMoving = false;
                Destroy(targetObject);
            }
        }
    }

    private void TurnWithoutMoving()
    {
        moveDirection = GetRandomDirection(); // Cambia de dirección
    }

    private void HandleAnimations()
    {
        anim.SetBool("Walk", isMoving); // Si el NPC se está moviendo, activar la animación de caminar
    }

    private Vector3 GetRandomDirection()
    {
        // Genera una dirección aleatoria (arriba, abajo, izquierda, derecha)
        int randomDir = Random.Range(0, 4);
        switch (randomDir)
        {
            case 0:
                lastDirection = Direction.Forward;
                anim.SetFloat("Y", 1);
                anim.SetFloat("X", 0);
                return transform.forward;
            case 1:
                lastDirection = Direction.Backward;
                anim.SetFloat("Y", -1);
                anim.SetFloat("X", 0);
                return -transform.forward;
            case 2:
                lastDirection = Direction.Right;
                anim.SetFloat("X", 1);
                anim.SetFloat("Y", 0);
                return transform.right;
            case 3:
                lastDirection = Direction.Left;
                anim.SetFloat("X", -1);
                anim.SetFloat("Y", 0);
                return -transform.right;
            default: 
                return Vector3.zero;
        }
    }

    private Vector3 lastBoxCenter;
    private Vector3 lastHalfExtents;
    private Quaternion lastBoxRotation;
    private bool CheckCollisionInDirection(Vector3 direction)
    {
        float gridDistance = this.gridDistance;

        // Dirección en la que se quiere mover el jugador
        Vector3 rayOrigin = transform.position + transform.up * 0.5f; // Levantamos el raycast un poco más para evitar colisiones con el suelo
        Ray ray = new Ray(rayOrigin, direction);

        // Dibujar el Raycast en la escena
        Debug.DrawRay(ray.origin, ray.direction * gridDistance, Color.red, 0.1f);

        // Calcular la posición del centro del boxcast en la rejilla
        Vector3 boxCenter = transform.position + direction.normalized * gridDistance;

        // Ajustar el tamaño del boxcast
        Vector3 halfExtents = new Vector3(boxSize / 2, 0.5f, boxSize / 2); // Aumentamos la altura del boxcast

        // Guardar los parámetros para dibujar en OnDrawGizmos
        lastBoxCenter = boxCenter;
        lastHalfExtents = halfExtents;
        lastBoxRotation = transform.rotation;

        // Ejecutar Raycast para detectar colisión inmediata
        if (Physics.Raycast(ray, gridDistance, obstacleLayerMask))
        {
            //return true;
        }

        // Ejecutar BoxCast en la posición objetivo para detectar colisiones
        if (Physics.BoxCast(transform.position, halfExtents, direction.normalized, out RaycastHit hit, transform.rotation, gridDistance, obstacleLayerMask))
        {
            return true;
        }

        return false;
    }

    private void AlignPlayerWithGround()
    {
        // Crear un Raycast desde 1.5 unidades arriba del jugador hacia abajo
        Vector3 rayOrigin = transform.position + transform.up * 1.5f;
        Ray ray = new Ray(rayOrigin, -transform.up);

        // Dibujar el Raycast en la escena
        Debug.DrawRay(ray.origin, ray.direction * 5f, Color.green, 0.1f);

        // Si el Raycast golpea algo en la capa del suelo, ajustar la posición del jugador
        if (Physics.Raycast(ray, out RaycastHit hit, 5f, groundLayerMask))
        {
            Vector3 point = hit.point;
            point += transform.up * groundYOffset;

            Vector3 offset = point - transform.position;
            transform.position = point;
            if (targetObject == null) return;
            targetObject.transform.position += offset;
        }
    }
}
