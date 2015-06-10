using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
    // Events
    public delegate void EventHandler();
    public static event EventHandler OnLocalPlayerSpawned;
    public static event EventHandler OnPlayerSpawned;

    // References
    internal Rigidbody2D body;
    internal PlayerUI playerUI;
    internal PhotonView photonView;

    // General
    internal bool isLocal = false;
    internal string playerName = "unnamed";

    float moveSpeed = 300f;
    float life = 1000f;
    internal Ability abilityA;
    internal Ability abilityB;

    // Local only
    internal LocalUI localUI;

    // Remote only
    internal NetObject netObject;

    void Awake()
    {
        // Handle references
        body = GetComponent<Rigidbody2D>();
        playerUI = GameObject.Find("PlayerUI").GetComponent<PlayerUI>();
        photonView = gameObject.GetComponent<PhotonView>();

        GameLogic.playerList.Add(this);

        gameObject.name = "RemotePlayer(" + playerName + ")";

        // Any player spawn event
        if (OnPlayerSpawned != null)
            OnPlayerSpawned();
    }

    public void SendUpdateNetSettings(float _moveSpeed, float _netObjectSmoothing, float _netObjectInterp)
    {
        photonView.RPC("ReceiveUpdateNetSettings", PhotonTargets.AllBuffered, _moveSpeed, _netObjectSmoothing, _netObjectInterp);
    }

    [RPC]
    public void ReceiveUpdateNetSettings(float _moveSpeed, float _netObjectSmoothing, float _netObjectInterp)
    {
        if (isLocal)
        {
            moveSpeed = _moveSpeed;

            Debug.Log("[ReceiveUpdateNetSettings] speed[" + _moveSpeed + "] smoothing[" + _netObjectSmoothing + "] interp[" + _netObjectInterp + "]");
        }
        else
        {
            netObject.SetNetSyncSmoothing(_netObjectSmoothing);
            netObject.SetNetInterpTime(_netObjectInterp);
        }
    }

    void Start()
    {
        if (!isLocal) // If its not set to local yet, it must be a remote player
            InitializeRemotePlayer();

        abilityA = gameObject.GetComponent<AbilityPush>();
        abilityA.cooldown = 0.2f;
        abilityA.Initialize(this);
    }

    /// <summary>
    /// Initialize the local player.
    /// </summary>
    public void InitializeLocalPlayer()
    {
        isLocal = true;
        gameObject.name = "LocalPlayer";

        localUI = GameObject.Find("LocalUI").GetComponent<LocalUI>();
        localUI.InitializeLocalUI();

        gameObject.AddComponent<PlayerControls>();
        GameLogic.localPlayer = this;

        // Local player spawn event
        if (OnLocalPlayerSpawned != null)
            OnLocalPlayerSpawned();

        // Randomize class
        // Send class and other data to other players
    }

    /// <summary>
    /// Initialize a remote player.
    /// </summary>
    public void InitializeRemotePlayer()
    {
        Destroy(body);
        netObject = gameObject.AddComponent<NetObject>();
        photonView.ObservedComponents.Clear();
        photonView.ObservedComponents.Add(netObject);
    }

    void OnDestroy()
    {
        GameLogic.playerList.Remove(this);
    }

    public float GetMoveSpeed() { return moveSpeed; }

    void LoadClass(ClassBase _classBase)
    {
        life = _classBase.life;
        moveSpeed = _classBase.moveSpeed;

        abilityA = _classBase.abilityA;
        abilityB = _classBase.abilityB;
    }
}
