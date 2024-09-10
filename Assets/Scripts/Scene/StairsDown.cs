using System.Collections;
using UnityEngine;

public class StairsDown : MonoBehaviour
{
    public GameObject sunOverlay;
    public Transform upShip;
    public Transform player;
    public GameManager manager;

    public Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Logic());
        }
    }

    private IEnumerator Logic()
    {
        SceneChanger.PlaySceneChangeAnim();

        yield return new WaitForSeconds(0.2f);

        sunOverlay.SetActive(true);
        player.transform.position = destination.position;
        player.transform.up = upShip.transform.up;
        player.transform.right = upShip.transform.forward;
        player.transform.SetParent(upShip);

        CameraFollow cf = FindFirstObjectByType<CameraFollow>();
        cf.transform.position = cf.target.position;

        GameObject target = FindFirstObjectByType<PlayerController>().targetObject;

        manager.playerUp = true;

        if (target != null)
        {
            target.transform.position = destination.position;
            target.transform.SetParent(player);
        }

        yield break;
    }
}
