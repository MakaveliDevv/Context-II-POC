using System.Collections;
using UnityEngine;

public class playSoundAtRandom : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] clips;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(playRandom());
    }

    IEnumerator playRandom()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 30f));
            if (clips.Length > 0)
            {
                AudioClip clip = clips[Random.Range(0, clips.Length)];
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(clip);
            }
        }
    }

}
