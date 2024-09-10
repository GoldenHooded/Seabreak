using UnityEngine;
using UnityEngine.UI;

public class E_ToOpenDialog : MonoBehaviour
{
    public DialogString[] dialogs;
    public GameObject doneSound;

    [SerializeField] bool dist;
    private Transform player;
    private DialogSystem ds;
    [SerializeField] private float minDist = 1.5f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ds = FindFirstObjectByType<DialogSystem>();
    }

    void Update()
    {
        dist = Vector3.Distance(transform.position, player.position) <= minDist;

        DialogString dialog = dialogs[Random.Range(0, dialogs.Length)];

        if (dist && ds.activeDialogString != dialog)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.5f, Time.deltaTime * 10);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10);
        }


        if (Input.GetKeyDown(KeyCode.E) && dist && ds.activeDialogString != dialog)
        {
            ds.activeDialogString = dialog;
        }
    }
}
