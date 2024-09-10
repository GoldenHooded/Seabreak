using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float t;

    private void Awake()
    {
        Destroy(gameObject, t);
    }
}
