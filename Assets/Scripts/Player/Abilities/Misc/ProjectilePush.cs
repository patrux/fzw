using UnityEngine;
using System.Collections;

public class ProjectilePush : ProjectileBase
{
    Vector3 direction = Vector3.zero;

    float kbForce = 2000f;
    float maxSpeed = 5f;

    public void Initialize(Player _playerOwner, Vector3 _sourcePosition, Vector3 _targetPosition)
    {
        playerOwner = _playerOwner;
        sourcePosition = _sourcePosition;
        targetPosition = _targetPosition;
        body = gameObject.GetComponent<Rigidbody2D>();

        _targetPosition.z = 0f;
        _sourcePosition.z = 0f;
        direction = (_targetPosition - _sourcePosition).normalized;

        body.MovePosition(Vector3.Lerp(transform.position, _targetPosition, 0.1f)); // temporary, fire with offset from player

        float force = 10000f; // this is the force that will take the body to maxSpeed and cap it there

        // Clamp magnitude to limit velocity it
        body.velocity = Vector2.ClampMagnitude(new Vector2(
           (direction.x * force) * Time.deltaTime,
          (direction.y * force) * Time.deltaTime), maxSpeed);

        //Debug.Log("mag[" + body.velocity.magnitude + "] velo[" + FZW.WriteVector(body.velocity) + "] dir[" + FZW.WriteVector(direction) + "] src[" + FZW.WriteVector(_sourcePosition) + "] tar[" + FZW.WriteVector(_targetPosition) + "]");
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        // Destroy if we hit a wall
        if (_other.tag == "Walls")
        {
            //Debug.Log("Collision was with a wall.");
            if (playerOwner != null)
                PhotonNetwork.Destroy(gameObject);
        }
        // Destroy and deal damage to player
        else if (_other.tag == "Player")
        {
            //if (_other.gameObject.GetComponent<Player>() != playerOwner)
            if (_other.gameObject.name == "LocalPlayer" && playerOwner == null)
            {
                _other.gameObject.GetComponent<Rigidbody2D>().AddForce((_other.transform.position - transform.position).normalized * kbForce);
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                //Debug.Log("Collision was with owner. Ignore.");
            }
        }
        // Destroy both projectiles
        else if (_other.tag == "Projectile")
        {
            //Debug.Log("Collision was with another projectile.");
            if (playerOwner != null) // this will cause problems, since two players will try to destroy
            {
                PhotonNetwork.Destroy(_other.gameObject);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
