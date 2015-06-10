using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityPush : Ability 
{
    //public WeaponIcon weaponIcon;
    string prefabName = "ProjectilePush";

    List<ProjectileBase> projectiles = new List<ProjectileBase>(); // change to projectile script?

    void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Returns whether or not the ability was used or not.
    /// </summary>
    public override bool UseAbility()
    {
        if (!IsOnCooldown())
        {
            isOnCooldown = true;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Maybe make RPC call instead
            GameObject go = (GameObject)PhotonNetwork.Instantiate(prefabName, playerOwner.transform.position, Quaternion.Euler(new Vector3(0f, 0f, 0f)), 0);
            ProjectilePush pp = go.GetComponent<ProjectilePush>();
            pp.Initialize(playerOwner, playerOwner.transform.position, mouseWorld);

            return true;
        }
        else
        {
            return false;
        }
    }
}
