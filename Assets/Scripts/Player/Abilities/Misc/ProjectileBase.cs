using UnityEngine;
using System.Collections;

public class ProjectileBase : MonoBehaviour 
{
    public Player playerOwner;
    public Rigidbody2D body;

    public Vector3 sourcePosition;
    public Vector3 targetPosition;
}
