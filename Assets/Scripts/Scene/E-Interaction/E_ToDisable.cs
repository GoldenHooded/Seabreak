using UnityEngine;
using UnityEngine.UI;

public class E_ToDisable : MonoBehaviour
{
    public float timeRequired;
    public Image image;
    public GameObject toDisable;
    public GameObject doneSound;
    public bool needsBucket = false;
    public GameObject bucket;

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


        bool bucketBool = !needsBucket || needsBucket && bucket.activeSelf;

        if (dist && bucketBool)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.5f, Time.deltaTime * 10);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10);
        }


        if (Input.GetKey(KeyCode.E) && dist && bucketBool)
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

        if (image != null)
            image.fillAmount = timer / timeRequired;

        if (timer >= timeRequired)
        {
            timer = 0;
            if (doneSound != null)
                Instantiate(doneSound);
            toDisable.SetActive(false);
            if (needsBucket)
            {
                bucket.SetActive(false);
            }
        }
    }
}
