using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControls : MonoBehaviour
{
    // The body
    Rigidbody2D body;

    // References that change every update
    Vector2 moveInput;
    Vector2 force = Vector2.zero;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateMoveInput();
        UpdateAttack();
    }

    void UpdateAttack()
    {
        if (Input.GetKeyDown(GameLogic.Keybinds.attackA))
        {
            GameLogic.localPlayer.abilityA.UseAbility();
        }
        else if (Input.GetKeyDown(GameLogic.Keybinds.attackB))
        {
            GameLogic.localPlayer.abilityB.UseAbility();
        }
    }

    void FixedUpdate()
    {
        force = GetVelocity(moveInput);

        if (IsMoving())
            body.AddForce((force * GetForceScale()), ForceMode2D.Force);

        force = Vector2.zero;
    }

    /// <summary>
    /// Update the velocity of the player
    /// </summary>
    Vector2 GetVelocity(Vector2 _moveInput)
    {
        if (IsMoving())
        {
            // Slow down diagonal movement
            if (IsMovingDiagonally())
                _moveInput *= 0.7071f;

            // Scale by delta time
            Vector2 v = new Vector2(
                    (_moveInput.x * GameLogic.localPlayer.GetMoveSpeed()) * Time.fixedDeltaTime,
                    (_moveInput.y * GameLogic.localPlayer.GetMoveSpeed()) * Time.fixedDeltaTime);

            return v;
        }
        else
            return Vector2.zero;
    }

    /// <summary>
    /// Sample player input and return as a Vector3(-1, 0, 1) for directions.
    /// </summary>
    void UpdateMoveInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(GameLogic.Keybinds.moveUp))
        {
            input.y = 1f;
        }
        else if (Input.GetKey(GameLogic.Keybinds.moveDown))
        {
            input.y = -1f;
        }

        if (Input.GetKey(GameLogic.Keybinds.moveLeft))
        {
            input.x = -1f;
        }
        else if (Input.GetKey(GameLogic.Keybinds.moveRight))
        {
            input.x = 1f;
        }

        moveInput = new Vector2(input.x, input.y);
    }

    /// <summary>
    /// If the player moving diagonally this frame
    /// </summary>
    bool IsMovingDiagonally()
    {
        return (Mathf.Abs(moveInput.x) > 0f && Mathf.Abs(moveInput.y) > 0f);
    }

    /// <summary>
    /// If the player moving in any direction manually
    /// </summary>
    bool IsMoving()
    {
        return (Mathf.Abs(moveInput.x) + Mathf.Abs(moveInput.y)) > 0f;
    }

    float GetForceScale()
    {
        return body.mass * 10f;
    }
}
