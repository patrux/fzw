using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class NetLogic : MonoBehaviour 
{
    // Allow user to enter player name and room name before connecting.
    string roomName = "default";
    string playerName = "ptx";

    /// <summary>
    /// Initialize.
    /// </summary>
    void Start()
    {
        UnityEngine.Random.seed = DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + +DateTime.Now.Millisecond;
        Connect();
    }

    void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("1.0.0");

        PhotonNetwork.sendRate = 30;
        PhotonNetwork.sendRateOnSerialize = 15;
    }

    void OnJoinedLobby()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.cleanupCacheOnLeave = true;
        roomOptions.maxPlayers = 32;

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Fail to join");
        PhotonNetwork.CreateRoom(null);
    }

    void OnJoinedRoom()
    {
        float playerCollisionsSize = 2f;
        Vector3 mapSize = GameObject.Find("Map").GetComponent<MapGenerationLogic>().GetFloorMapSize();
        Vector2 randomPosition = new Vector2(
            Random.Range(-mapSize.x + playerCollisionsSize, mapSize.x - playerCollisionsSize),
            Random.Range(-mapSize.y + playerCollisionsSize, mapSize.y - playerCollisionsSize));


        GameObject playerObj = (GameObject)PhotonNetwork.Instantiate("Player", new Vector3(randomPosition.x, randomPosition.y, 0f), Quaternion.identity, 0);
        Player player = playerObj.GetComponent<Player>();
        PhotonView photonView = playerObj.GetComponent<PhotonView>();
        player.InitializeLocalPlayer();
    }

    //void OnGUI()
    //{
    //    GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    //}
}
