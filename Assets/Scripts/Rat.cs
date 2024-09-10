using UnityEngine;

public class Rat : MonoBehaviour
{
    public GameObject pref;
    private Transform player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= 0.5f)
        {
            Instantiate(pref, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
