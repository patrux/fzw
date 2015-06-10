using UnityEngine;
using System.Collections;

public class AutoDestroyObject : MonoBehaviour 
{
    private float timer = 0f;
    public float duration;
	
	void Update () 
    {
        timer += Time.deltaTime;

        if (timer > duration)
        {
            Destroy(gameObject);
        }
	}
}
