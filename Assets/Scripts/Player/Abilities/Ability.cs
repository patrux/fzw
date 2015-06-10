using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour, IAbility 
{
    [Range(0, 10)]
    public float cooldown;

    protected float cooldownTimer = 0.0f;
    protected bool isOnCooldown = false;

    protected Player playerOwner;

    public void Initialize(Player _playerOwner)
    {
        playerOwner = _playerOwner;
    }

    public void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer > cooldown)
            {
                isOnCooldown = false;
                cooldownTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// Returns whether or not the ability was used or not.
    /// </summary>
    public virtual bool UseAbility()
    {
        return true;
    }

    protected bool IsOnCooldown()
    {
        return isOnCooldown;
    }
}
