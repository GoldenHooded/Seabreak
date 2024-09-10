using UnityEngine;
using UnityEngine.UI;

public class E_ToEnable : MonoBehaviour
{
    public float timeRequired;
    public Image image;
    public GameObject toEnable;
    public GameObject doneSound;

    [SerializeField] private float timer;
    [SerializeField] bool dist;
    private Transform player;
    [SerializeField] private float minDist = 1.5f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        dist = Vector3.Distance(transform.position, player.position) <= minDist;

        if (dist)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.5f, Time.deltaTime * 10);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10);
        }


        if (Input.GetKey(KeyCode.E) && dist)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer -= 3 * Time.deltaTime;
        }

        if (timer < 0)
        {
            timer = 0;
        }

        image.fillAmount = timer / timeRequired;

        if (timer >= timeRequired)
        {
            timer = 0;
            Instantiate(doneSound);
            toEnable.SetActive(true);
        }
    }
}
