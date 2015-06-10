using UnityEngine;
using System.Collections;

public class AutoDestroySound : MonoBehaviour 
{
    public bool randomPitch;

    private AudioSource audioSource;

    private float timer = 0.0f;
    private float minWaitTime = 1.0f;

	void Start () 
    {
        audioSource = GetComponent<AudioSource>();

        if (randomPitch)
        {
            audioSource.pitch += Random.Range(-0.2f, 0.2f);
        }
	}
	
	void Update () 
    {
        if (timer > minWaitTime)
        {
            if (!audioSource.isPlaying)
                Destroy(gameObject);
        }
        else
        {
            timer += Time.deltaTime;
        }
	}
}
