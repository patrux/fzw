using UnityEngine;
using System;
using System.Collections;
using System.Reflection; 

// For the client which owns the player, the local transform contains 
// the current client _predicted_ position/rotation. The buffered state
// contains the _server_ position/rotation, which is the correct state. The
// predicted and server states are interpolated together so the predicted
// state will always converge with the server state.
//
// For the server the transform contains the current 100% legal position and
// rotation of the player in question. He sends this over the network
// to all clients.
//
// For other clients, which don't own this player, the buffered state contains 
// the correct values. This is played back with a 100 ms delay to elminate
// choppyness. The delay needs to be higher is the ping time between the server
// and client is larger than 100 ms.
public class GraduallyUpdateState : MonoBehaviour
{
    // Get velocity info directly from the rigidbody
    Rigidbody2D body;

    public float lerpOtherPlayer = 0.2f;

    internal struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }

    // How far behind should server data be played back during remote player
    // interpolation. The poorer the connection the higher this value needs to
    // be. Fast connections should do fine with 0.1. The server latency
    // affects this the most.
    public double m_InterpolationBackTime = 0.1f;

    // The position vector distance to start error correction. The higher the latency the higher this
    // value should be or it constantly tries to correct errors in prediction, of course this depends
    // on the game too.
    public float m_PredictionThreshold = 0.25f;

    // Time difference in milliseconds where we check for error in position. If the server time value
    // of a state is too different from the local state time then the error correction comparison is
    // highly unreliable and you might try to correct more errors than there really are.
    public float m_TimeThreshold = 0.05f;

    // We store twenty states with "playback" information
    State[] m_BufferedState = new State[32]; // default: 20
    State[] m_LocalBufState = new State[32]; // default: 20

    // Keep track of what slots are used
    int m_TimestampCount;
    int m_LocalStateCount;

    bool m_IsMine = false;

    // Stat variables for latency, msg rate
    double m_MsgLatencyTotal = 0;

    // Stat variabels for prediction stuff
    float m_TimeAccuracy = 0;
    float m_PredictionAccuracy = 0;
    bool m_FixError = false;
    Vector3 m_NewPosition;

    Vector3 targetPosition;

    IEnumerator MonitorLocalMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(1 / 15);

            // Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
            for (int i = m_LocalBufState.Length - 1; i >= 1; i--)
            {
                m_LocalBufState[i] = m_LocalBufState[i - 1];
            }

            // Save currect received state as 0 in the buffer, safe to overwrite after shifting
            State state;
            state.timestamp = PhotonNetwork.time;
            state.pos = transform.position;
            state.rot = transform.rotation;
            m_LocalBufState[0] = state;

            // Increment state count but never exceed buffer size
            m_LocalStateCount = Mathf.Min(m_LocalStateCount + 1, m_LocalBufState.Length);

            // *** Check if the client side prediction has an error

            // Find the local buffered state which is closest to network state in time
            int j = 0;
            bool match = false;

            for (j = 0; j < m_LocalStateCount - 1; j++)
            {
                if (m_BufferedState[0].timestamp <= m_LocalBufState[j].timestamp && m_LocalBufState[j].timestamp - m_BufferedState[0].timestamp <= m_TimeThreshold)
                {
                    m_PredictionAccuracy = (Vector3.Distance(m_LocalBufState[j].pos, m_BufferedState[0].pos));

                    match = true;
                    break;
                }
            }

            if (!match)
            {
                //Debug.Log("No match!");
            }
            // If prediction is off, diverge current location by the amount of the offset
            else if (m_PredictionAccuracy > m_PredictionThreshold)
            {
                //Debug.Log("Error in prediction("+m_PredictionAccuracy+"), local is " + m_LocalBufState[j].pos + " network is " + m_BufferedState[0].pos);
                //Debug.Log("Local time: " + m_LocalBufState[j].timestamp + " Network time: " + m_BufferedState[0].timestamp);

                // Find how far we travelled since the prediction failed
                Vector3 localMovement = m_LocalBufState[j].pos - m_LocalBufState[0].pos;

                // "Erase" old values in the local buffer
                m_LocalStateCount = 1;

                // New position which we need to converge to in the update loop                         
                m_NewPosition = m_BufferedState[0].pos + localMovement;

                // Trigger the new position convergence routine                         
                m_FixError = true;
            }
            else
            {
                m_FixError = false;
            }
        }
    }

    // This only runs where the component is enabled, which is only on remote peers (server/clients)
    void Update()
    {
        targetPosition = transform.position;

        double currentTime = PhotonNetwork.time;

        //Debug.Log("currentTime[" + currentTime + "] sendRate[" + PhotonNetwork.sendRate + "] sndRSerialize[" + PhotonNetwork.sendRateOnSerialize + "]");

        // We have a window of interpolationBackTime where we basically play 
        // By having interpolationBackTime the average ping, you will usually use interpolation.
        // And only if no more data arrives we will use extrapolation
        double interpolationTime = currentTime - m_InterpolationBackTime;

        //Debug.Log("m_IsMine[" + m_IsMine + "] IsMoving[" + IsMoving() + "] trans.pos[" + transform.position + "] ");

        if (m_IsMine && m_FixError) // If this is my player interpolate server position with the position set by me
        {
            // This is a cheap method for interpolating server and client positions. The higher
            // the difference the closer to the client state we will go. This is to minimize big jumps
            // in the movment. This can be done differenctly, like for example just doing 50/50 server
            // and client position.
            Vector3 difference = m_NewPosition - transform.position;

            targetPosition = Vector3.Lerp(m_NewPosition, transform.position, difference.magnitude);

            //Debug.Log("[MOVING] difference[" + difference + "] = m_NewPosition[" + m_NewPosition + "] - trans.pos[" + transform.position + "] mag[" + difference.magnitude + "]");
        }
        else if (m_IsMine && !IsMoving()) // If we are not moving converge to the 100% accurate server position
        {
            targetPosition = Vector3.Lerp(m_BufferedState[0].pos, transform.position, 0.95F);

            //Debug.Log("[IDLE] m_BufferedState[0].pos[" + m_BufferedState[0].pos + "] trans.pos[" + transform.position + "]");
        }
        //else if (PhotonNetwork.isClient && !m_IsMine)
        else if (!m_IsMine) // Use interpolation for other remote clients
        {
            // Use interpolation
            // Check if latest state exceeds interpolation time, if this is the case then
            // it is too old and extrapolation should be used

            //print("OtherPlayer[" + gameObject.name + "] m_BufferedState.timestamp[" + m_BufferedState[0].timestamp + "] > interpolationTime[" + interpolationTime + "] interp[" + (m_BufferedState[0].timestamp > interpolationTime) + "]");

            if (m_BufferedState[0].timestamp > interpolationTime)
            {
                for (int i = 0; i < m_TimestampCount; i++)
                {
                    // Find the state which matches the interpolation time (time+0.1) or use last state
                    if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                    {
                        // The state one slot newer (<100ms) than the best playback state
                        State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
                        // The best playback state (closest to 100 ms old (default time))
                        State lhs = m_BufferedState[i];

                        // Use the time between the two slots to determine if interpolation is necessary
                        double length = rhs.timestamp - lhs.timestamp;
                        float t = 0.0f;
                        // As the time difference gets closer to 100 ms t gets closer to 1 in 
                        // which case rhs is only used
                        if (length > 0.0001)
                            t = (float)((interpolationTime - lhs.timestamp) / length);

                        // if t=0 => lhs is used directly
                        targetPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);
                        transform.rotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                        //m_InterpolationTime = (Network.time - m_BufferedState[i].timestamp)*1000;
                        return;
                    }
                }
            }
            // Use extrapolation. Here we do something really simple and just repeat the last
            // received state. You can do clever stuff with predicting what should happen.
            else
            {
                State latest = m_BufferedState[0];

                targetPosition = latest.pos;
                transform.localRotation = latest.rot;

                //Debug.Log("Extrapolating " + latest.pos);
            }

            // Custom smoothing
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpOtherPlayer);
        }
        else
        {
            // Since this is our local player, we have full control over the unit, so sample our own live data for the states

            // Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
            for (int i = m_BufferedState.Length - 1; i >= 1; i--)
            {
                m_BufferedState[i] = m_BufferedState[i - 1];
            }

            // Save currect received state as 0 in the buffer, safe to overwrite after shifting
            State state;
            state.timestamp = currentTime;
            state.pos = transform.position;
            state.rot = Quaternion.identity;
            m_BufferedState[0] = state;

            // Increment state count but never exceed buffer size
            m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

            //transform.position = Vector3.Lerp(transform.position, targetPosition, lerpOtherPlayer);
        }
    }

    // The network sync routine makes sure m_BufferedState always contains the last 20 updates
    // The latest update is in slot 0, oldest in slot 19
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Always send transform (depending on reliability of the network view)
        if (stream.isWriting)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        // When receiving, buffer the information
        else
        {
            m_MsgLatencyTotal += (PhotonNetwork.time - info.timestamp);

            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = transform.rotation; //Quaternion.identity;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            // Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
            for (int i = m_BufferedState.Length - 1; i >= 1; i--)
            {
                m_BufferedState[i] = m_BufferedState[i - 1];
            }

            // Save currect received state as 0 in the buffer, safe to overwrite after shifting
            State state;
            state.timestamp = info.timestamp;
            state.pos = pos;
            state.rot = rot;
            m_BufferedState[0] = state;

            // Increment state count but never exceed buffer size
            m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

            // Check integrity, lowest numbered state in the buffer is newest and so on
            for (int i = 0; i < m_TimestampCount - 1; i++)
            {
                if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
                    Debug.Log("State inconsistent");
            }

            //Debug.Log("stamp: " + info.timestamp + "my time: " + Network.time + "delta: " + (PhotonNetwork.time - info.timestamp));
        }
    }

    bool IsMoving()
    {
        return ((Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y)) > 0f);
    }


    public void SetIsLocalPlayer()
    {
        m_IsMine = true;
        StartCoroutine(MonitorLocalMovement());
    }

    void OnDisconnectedFromPhoton()
    {
        // Debug.Log("[GraduallyUpdateState] Disconnected from server. Destroying.");
        Destroy(gameObject);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        Player.OnLocalPlayerSpawned += SetIsLocalPlayer;
    }

    string WriteVector(Vector3 _v)
    {
        return "(" + _v.x + ", " + _v.y + ", " + _v.z + ")";
    }

    Rect connInfo = new Rect(10, 100, 320, 160);

    void OnGUI()
    {
        connInfo = GUILayout.Window(0, connInfo, MakeConnInfoWindow, "Local Player");
    }

    void MakeConnInfoWindow(int windowID)
    {
        for (int i = 0; i < Mathf.Min(m_BufferedState.Length, 16); i++)
        {
            GUILayout.Label(string.Format("m_BufferedState[" + i + "]: ({0}, {1})", m_BufferedState[i].pos.x, m_BufferedState[i].pos.y));
        }
    }
}
