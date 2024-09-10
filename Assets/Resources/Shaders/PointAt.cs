using UnityEngine;

public class PointAt : MonoBehaviour
{
    public Transform pointAtTransform;
    public bool onlyY;

    void Update()
    {
        if (pointAtTransform != null)
        {
            if (onlyY)
            {
                Vector3 direction = pointAtTransform.position - transform.position;
                direction.y = 0; // Ignorar la diferencia en Y para solo rotar en el eje Y
                if (direction != Vector3.zero)
                {
                    transform.forward = direction;
                }
            }
            else
            {
                transform.forward = pointAtTransform.position - transform.position;
            }
        }
    }
}
