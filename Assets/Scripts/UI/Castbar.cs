using UnityEngine;
using System.Collections;

public class Castbar : MonoBehaviour 
{
    public UISprite sprite;
    public UISprite spriteBG;

    void Awake()
    {
        LocalUI.OnEnableUI += Initialize;
    }

    void Initialize()
    {
        spriteBG.enabled = true;
    }
}
