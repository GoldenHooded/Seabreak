using System.Collections;
using UnityEngine;

public class StairsUp : MonoBehaviour
{
    public GameObject sunOverlay;
    public Transform downShip;
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

        sunOverlay.SetActive(false);
        player.transform.position = destination.position;
        player.transform.up = downShip.transform.up;
        player.transform.right = downShip.transform.forward;
        player.transform.SetParent(downShip);

        CameraFollow cf = FindFirstObjectByType<CameraFollow>();
        cf.transform.position = cf.target.position;

        GameObject target = FindFirstObjectByType<PlayerController>().targetObject;

        manager.playerUp = false;

        if (target != null)
        {
            target.transform.position = destination.position;
            target.transform.SetParent(player);
        }

        yield break;
    }
}
