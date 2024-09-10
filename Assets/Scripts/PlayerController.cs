using UnityEngine;
public class PlayerController : MonoBehaviour
{
    // Enum para las direcciones
    public enum Direction
    {
        Forward,
        Backward,
        Right,
        Left
    }

    // Variables de control
    public bool canMove = true;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float gridDistance = 1f; // Distancia a recorrer en cada paso de la rejilla
    public float timeBeforeStartWalking = 0.125f;
    private float currentSpeed;

    private bool isMoving = false;
    private Vector3 moveDirection;
    public GameObject targetObject; // Objeto objetivo para el movimiento

    // Variables de estado
    public bool isWalking { get; private set; }
    public bool isRunning { get; private set; }
    public bool isIdle { get; private set; }
    public Direction lastDirection { get; private set; } // Última dirección en la que se ha movido el jugador
    private bool wasMoving;
    private float inputTimer;

    // Variables para colisiones
    public LayerMask obstacleLayerMask;
    public LayerMask groundLayerMask; // Nueva LayerMask para el suelo
    public float boxSize = 0.9f;

    // Variables para el animator
    public Animator anim;

    // Start is called before the first frame update
    private void Start()
    {
        currentSpeed = walkSpeed;
        isIdle = true;
        isWalking = false;
        isRunning = false;
        moveDirection = Vector3.zero;
        lastDirection = Direction.Forward; // Valor inicial para la dirección
    }

    // Update is called once per frame
    private void Update()
    {
        HandleInput();
        AlignPlayerWithGround();
        MovePlayer();
        UpdatePlayerState();
    }

    private void LateUpdate()
    {
        HandleAnimations();
    }

    private void HandleInput()
    {
        if (!canMove) return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        if (isMoving)
            return;

        moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection = transform.forward;
            lastDirection = Direction.Forward; // Actualizar la dirección

            anim.SetFloat("Y", 1);
            anim.SetFloat("X", 0);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection = -transform.forward;
            lastDirection = Direction.Backward; // Actualizar la dirección

            anim.SetFloat("Y", -1);
            anim.SetFloat("X", 0);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveDirection = -transform.right;
            lastDirection = Direction.Left; // Actualizar la dirección

            anim.SetFloat("X", -1);
            anim.SetFloat("Y", 0);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDirection = transform.right;
            lastDirection = Direction.Right; // Actualizar la dirección

            anim.SetFloat("X", 1);
            anim.SetFloat("Y", 0);
        }

        bool timerEnded = inputTimer > timeBeforeStartWalking;

        if (!wasMoving && moveDirection != Vector3.zero)
        {
            if (!timerEnded && currentSpeed != runSpeed)
            {
                inputTimer += Time.deltaTime;
                return;
            }
        }
        else
        {
            inputTimer = 0;
        }

        // Move

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

    private void HandleAnimations()
    {
        anim.SetBool("Walk", isWalking);
        anim.SetBool("Run", isRunning);
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

    // Dibuja el BoxCast en la escena
    private void OnDrawGizmos()
    {
        if (lastBoxCenter != Vector3.zero)
        {
            // Establecer el color para el Gizmo
            Gizmos.color = Color.blue;

            // Dibujar la caja del BoxCast
            Gizmos.matrix = Matrix4x4.TRS(lastBoxCenter, lastBoxRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, lastHalfExtents * 2);
        }
    }


    private void MovePlayer()
    {
        if (isMoving && targetObject != null)
        {
            // Mover el jugador hacia la posición del objeto objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetObject.transform.position, currentSpeed * Time.deltaTime);

            // Comprobar si ha llegado a la posición del objeto objetivo
            if (Vector3.Distance(transform.position, targetObject.transform.position) < 0.01f)
            {
                transform.position = targetObject.transform.position;
                isMoving = false;
                Destroy(targetObject); // Destruir el objeto objetivo al llegar a su posición
            }
        }
    }

    private void UpdatePlayerState()
    {
        if (isMoving || wasMoving)
        {
            isIdle = false;

            if (currentSpeed == walkSpeed)
            {
                isWalking = true;
                isRunning = false;
            }
            else if (currentSpeed == runSpeed)
            {
                isWalking = false;
                isRunning = true;
            }
        }
        else
        {
            isIdle = true;
            isWalking = false;
            isRunning = false;
        }

        wasMoving = isMoving;
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
            Vector3 offset = hit.point - transform.position;
            transform.position = hit.point;
            if (targetObject == null) return;
            targetObject.transform.position += offset;
        }
    }
}
