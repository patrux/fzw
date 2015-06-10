using UnityEngine;
using System.Collections;

public class ObjectFollow : MonoBehaviour
{
    // The Target to follow
    public GameObject targetFollow;

    // Offset
    public Vector3 offset = new Vector3(0f, 0f, 0f);

    void Update()
    {
        transform.position = targetFollow.transform.position + offset;
    }
}
