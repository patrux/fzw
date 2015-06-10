using UnityEngine;
using System.Collections;

public class LavaDamage : MonoBehaviour 
{
    public float damagePerSec = 10f;

    void OnTriggerStay2D(Collider2D _other)
    {
        //life -= damagePerSec * Time.fixedDeltaTime;
        //print("Lava!!!");
    }
}
