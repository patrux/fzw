using UnityEngine;
using System.Collections;

public class FZWDebug : MonoBehaviour
{
    public float moveSpeed = 600f;
    public float netObjectSmoothing = 0.15f;
    public float netObjectInterp = 0.1f;

    void Update()
    {
        if (Input.GetKeyDown(GameLogic.Keybinds.debugUpdateNetSettings))
        {
            foreach (Player p in GameLogic.playerList)
                p.SendUpdateNetSettings(moveSpeed, netObjectSmoothing, netObjectInterp);
        }
    }
}
