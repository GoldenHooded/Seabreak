using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class E_ToCallEvent : MonoBehaviour
{
    public float timeRequired;
    public float timeSubtractMult = 3;
    public Image image;
    public GameObject doneSound;
    public UnityEvent eventToCall;

    [SerializeField] private float timer;
    [SerializeField] bool dist;
    private Transform player;
    [SerializeField] private float minDist = 1.5f;
    public GameObject needsToBeActive;
    public bool canBeUsed = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        dist = Vector3.Distance(transform.position, player.position) <= minDist;

        bool needsToBeActiveBool = needsToBeActive == null || needsToBeActive != null && needsToBeActive.activeSelf;

        if (dist && needsToBeActiveBool)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.5f, Time.deltaTime * 10);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10);
        }

        if (Input.GetKey(KeyCode.E) && dist && needsToBeActiveBool)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer -= timeSubtractMult * Time.deltaTime;
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

            if (canBeUsed && needsToBeActive != null)
            {
                needsToBeActive.SetActive(false);
            }

            eventToCall.Invoke();
        }
    }
}
