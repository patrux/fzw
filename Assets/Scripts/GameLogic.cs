using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GameLogic : MonoBehaviour
{
    static protected GameLogic instance;
    void Awake() { instance = this; }
    void OnDestroy() { instance = null; }

    public static Transform spawnedObjectsParent;

    public static List<Player> playerList = new List<Player>();
    public static Player localPlayer;

    /// <summary>
    /// Instantiates the object on the network.
    /// </summary>
    public static GameObject NetInstantiate(string _name, Vector3 _position, float _rotation, Player _playerOwner)
    {
        GameObject go = (GameObject)PhotonNetwork.Instantiate(_name, _position, Quaternion.Euler(new Vector3(0f, 0f, _rotation)), 0);
        go.transform.parent = spawnedObjectsParent;
        return go;
    }

    //public static void InstantiateProjectile(string _prefabName, Vector3 _position, Vector3 _target, float _rotation, Player _playerOwner)
    //{
    //    GameObject go = (GameObject)Instantiate(Resources.Load(_prefabName), _position, Quaternion.Euler(new Vector3(0f, 0f, _rotation)));
    //    ProjectilePush pp = go.GetComponent<ProjectilePush>();
    //    pp.Initialize(_playerOwner, _position, _target, 600f);
    //}

    public static class Keybinds
    {
        // Debug
        public static KeyCode consoleWindow = KeyCode.F1;
        public static KeyCode debugUpdateNetSettings = KeyCode.F4;
        public static KeyCode debugMoveToggle = KeyCode.G;

        // Player
        public static KeyCode moveUp = KeyCode.W;
        public static KeyCode moveDown = KeyCode.S;
        public static KeyCode moveLeft = KeyCode.A;
        public static KeyCode moveRight = KeyCode.D;

        public static KeyCode attackA = KeyCode.Mouse0;
        public static KeyCode attackB = KeyCode.Mouse1;
    }
}
