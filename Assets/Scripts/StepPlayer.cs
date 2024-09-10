using UnityEngine;

public class StepPlayer : MonoBehaviour
{
    public AudioClip[] clips;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayStep()
    {
        source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
