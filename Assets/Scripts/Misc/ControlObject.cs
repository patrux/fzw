using UnityEngine;
using System.Collections;

public class ControlObject : MonoBehaviour
{
    public float speed = 800f;
    public Rigidbody2D body;

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
        {
            input.y = speed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            input.y = -speed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            input.x = -speed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            input.x = speed;
        }

        body.velocity = new Vector2(
            input.x * Time.deltaTime,
            input.y * Time.deltaTime);
    }
}
