using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControls : MonoBehaviour
{
    // The body
    Rigidbody2D body;

    // References that change every update
    Vector2 moveInput;
    Vector2 force = Vector2.zero;

    // Debug variable
    int debugToggleMove = 0;
    int debugToggleMoveCount = 3;

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
            float diagonalPenalty = 0.7071f;

            // Scale by delta time
            Vector2 v;

            if (IsMovingDiagonally())
                v = new Vector2(
                    ((_moveInput.x * diagonalPenalty) * GameLogic.localPlayer.GetMoveSpeed()) * Time.fixedDeltaTime,
                    ((_moveInput.y * diagonalPenalty) * GameLogic.localPlayer.GetMoveSpeed()) * Time.fixedDeltaTime);
            else
                v = new Vector2(
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
        if (DebugInput()) // Steals "focus" if there was input
            return;

        Vector2 input = Vector2.zero;

        if (Input.GetKey(GameLogic.Keybinds.moveUp))
            input.y = 1f;
        else if (Input.GetKey(GameLogic.Keybinds.moveDown))
            input.y = -1f;

        if (Input.GetKey(GameLogic.Keybinds.moveLeft))
            input.x = -1f;
        else if (Input.GetKey(GameLogic.Keybinds.moveRight))
            input.x = 1f;

        moveInput = input;
    }

    float randomMoveTime = 0f;
    float randomMoveDur = 0f;

    bool DebugInput()
    {
        if (Input.GetKeyDown(GameLogic.Keybinds.debugMoveToggle))
        {
            debugToggleMove = (debugToggleMove < debugToggleMoveCount) ? (debugToggleMove + 1) : 0; // Toggle

            if (debugToggleMove == 1)
            {
                Debug.Log("[PlayerControls::BOT] Moving randomly.");
                randomMoveTime = 0f;
                randomMoveDur = Random.Range(0.1f, 0.6f);
                moveInput = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2)); // random int has non-inclusive max
            }
            
            else if (debugToggleMove == 2)
            {
                Debug.Log("[PlayerControls::BOT] Moving left.");
                moveInput = new Vector2(-1f, 0f);
            }
            else if (debugToggleMove == 3)
            {
                Debug.Log("[PlayerControls::BOT] Moving right.");
                moveInput = new Vector2(1f, 0f);
            }
            else
            {
                Debug.Log("[PlayerControls] Resuming control.");
            }
        }

        if (debugToggleMove == 1)
        {
            randomMoveTime += Time.deltaTime;

            if (randomMoveTime > randomMoveDur)
            {
                randomMoveTime = 0f;
                randomMoveDur = Random.Range(0.1f, 0.6f);
                moveInput = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));  // random int has non-inclusive max
                //print("RandomInput[" + moveInput + "]");
            }
        }

        return (debugToggleMove > 0); // True if there was debug input
    }

    // If the player moving diagonally this frame
    bool IsMovingDiagonally() { return (Mathf.Abs(moveInput.x) > 0f && Mathf.Abs(moveInput.y) > 0f); }

    // If the player moving in any direction manually
    bool IsMoving() { return (Mathf.Abs(moveInput.x) + Mathf.Abs(moveInput.y)) > 0f; }

    // Scale AddForce by this to correct the "force scale"
    float GetForceScale() { return body.mass * 10f; }
}
