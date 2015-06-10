using UnityEngine;
using System.Collections;

public class NetObject : MonoBehaviour
{
    double interpolationBackTime = 0.1;

    // Smoothing time (ms) for position updates
    float netSyncSmoothing = 0.15f;

    // References
    Vector3 targetPosition = Vector3.zero;
    Rigidbody2D body;

    internal struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }

    // Store game states
    State[] m_BufferedState = new State[20];

    // Current state slot
    int m_TimestampCount;

    bool isInterpolating = false;

    // Debug
    double interpolationTime2;

    // Extrapolation
    Vector3 latestCorrectPos = Vector3.zero;
    Vector3 lastMovement = Vector3.zero;
    double lastTime = 0;

    void FixedUpdate()
    {
        double currentTime = PhotonNetwork.time;
        double interpolationTime = currentTime - interpolationBackTime;

        interpolationTime2 = interpolationTime;

        // We have a window of interpolationBackTime where we basically play 
        // By having interpolationBackTime the average ping, you will usually use interpolation.
        // And only if no more data arrives we will use extrapolation
        if (m_BufferedState[0].timestamp > interpolationTime)
            Interpolate(interpolationTime);
        else
            Extrapolate();
    }

    // Use interpolation
    // Check if latest state exceeds interpolation time, if this is the case then
    // it is too old and extrapolation should be used
    void Interpolate(double _interpolationTime)
    {
        isInterpolating = true;
        Debug.Log("Interpolating!");
        //Debug.Log("[* Interpolating] m_BufferedState[0].timestamp[" + m_BufferedState[0].timestamp + "] > interpolationTime[" + interpolationTime + "] difference[" + (m_BufferedState[0].timestamp - interpolationTime) + "]");

        for (int i = 0; i < m_TimestampCount; i++)
        {
            // Find the state which matches the interpolation time (time+0.1) or use last state
            if (m_BufferedState[i].timestamp <= _interpolationTime ||
                i == m_TimestampCount - 1)
            {
                //Debug.Log("[State matching interp time] m_BufferedState[" + i + "].timestamp[" + m_BufferedState[i].timestamp + "] <= interpolationTime[" + interpolationTime + "]");

                // The state one slot newer (<100ms) than the best playback state
                State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
                //Debug.Log("[OneFrameNewerThanBest] rhs[" + rhs + "] for m_BufferedState[" + Mathf.Max(i - 1, 0) + "]");

                // The best playback state (closest to 100 ms old (default time))
                State lhs = m_BufferedState[i];
                //Debug.Log("[BestPlayackFrame] lhs[" + rhs + "] for m_BufferedState[" + i + "]");

                // Use the time between the two slots to determine if interpolation is necessary
                double length = rhs.timestamp - lhs.timestamp;
                float t = 0.0f;
                // As the time difference gets closer to 100 ms t gets closer to 1 in 
                // which case rhs is only used
                if (length > 0.0001)
                {
                    t = (float)((_interpolationTime - lhs.timestamp) / length);
                    //Debug.Log("[Lerp] t[" + t + "] = interpolationTime[" + interpolationTime + "] - lhs[" + lhs.timestamp + "] / length(rhs-lhs)[" + length + "]");
                }

                //Debug.Log("[Lerp] transform.localPosition = Vector3.Lerp(lhs,rhs,t)[" + Vector3.Lerp(lhs.pos, rhs.pos, t) + "] lhs.pos[" + lhs.pos + "] rhs.pos[" + rhs.pos + "] t[" + t + "]");
                // if t=0 => lhs is used directly
                targetPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);
                transform.localRotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                return;
            }
        }
    }

    // Use extrapolation. Here we do something really simple and just repeat the last
    // received state. You can do clever stuff with predicting what should happen.
    void Extrapolate()
    {
        isInterpolating = false;

        //double timeDifference = PhotonNetwork.time - lastTime;

        //if (timeDifference <= 0)
        //    timeDifference = 0.000001f;

        //lastMovement = new Vector3(
        //    (float)((transform.position.x - latestCorrectPos.x) / timeDifference),
        //    (float)((transform.position.y - latestCorrectPos.y) / timeDifference),
        //    0f);

        //Debug.Log("Extrapolating! t[" + timeDifference + "] direction[" + FZW.WriteVector((latestCorrectPos - transform.position).normalized) + "] magnitude[" + (latestCorrectPos - transform.position).magnitude + "] state0[" + m_BufferedState[0].timestamp + "]");
        
        //lastTime = PhotonNetwork.time;
        //latestCorrectPos = m_BufferedState[0].pos;

        //targetPosition = (transform.position + (lastMovement * Time.deltaTime));
        //transform.localRotation = m_BufferedState[0].rot;

        targetPosition = m_BufferedState[1].pos - m_BufferedState[0].pos;
        transform.localRotation = m_BufferedState[0].rot;
    }

    public float linePower = 2f;
    SpriteRenderer sr;

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, netSyncSmoothing);

        //if (true)
        //{
        //    if (sr == null)
        //        sr = gameObject.GetComponentInChildren<SpriteRenderer>();

        //    if (isInterpolating)
        //        sr.color = Color.cyan;
        //    else
        //        sr.color = Color.blue;
        //}

        Vector3 direction = (targetPosition - transform.position).normalized;

        // Custom smoothing
        if (isInterpolating)
            Debug.DrawLine(transform.position, transform.position + (direction * linePower), Color.cyan, Time.deltaTime);
        else
            Debug.DrawLine(transform.position, transform.position + (direction * linePower), Color.blue, Time.deltaTime);
    }

    void Start()
    {
        targetPosition = transform.position;
        body = GetComponent<Rigidbody2D>();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Always send transform (depending on reliability of the network view)
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        // When receiving, buffer the information
        else
        {
            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
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
        }
    }

    public void SetNetInterpTime(float _value) { interpolationBackTime = _value; }

    public void SetNetSyncSmoothing(float _value) { netSyncSmoothing = _value; }

    // *** Create an information display window
    Rect infoWindowRect = new Rect(Screen.width - 320, 10, 310, 160);

    void OnGUI()
    {
        infoWindowRect = GUILayout.Window(0, infoWindowRect, InfoWindow, "NetObject");
    }

    void InfoWindow(int windowID)
    {
        GUILayout.Label(string.Format("m_BufferedState[0]: ({0}, {1})", m_BufferedState[0].pos.x, m_BufferedState[0].pos.y));
        GUILayout.Label(string.Format("m_BufferedState[" + (m_BufferedState.Length - 1) + "]: ({0}, {1})", m_BufferedState[m_BufferedState.Length - 1].pos.x, m_BufferedState[m_BufferedState.Length - 1].pos.y));
        GUILayout.Label(string.Format("differenceState: {0}", m_BufferedState[0].timestamp - m_BufferedState[m_BufferedState.Length - 1].timestamp));
        GUILayout.Label(string.Format("difference: {0}", m_BufferedState[0].timestamp - interpolationTime2));
        GUILayout.Label(string.Format("interpolating: {0}", isInterpolating));
    }

    //void InfoWindow(int windowID)
    //{
    //    GUILayout.Label(string.Format("m_BufferedState[0]: ({0}, {1})", m_BufferedState[0].pos.x, m_BufferedState[0].pos.y));
    //    GUILayout.Label(string.Format("m_BufferedState[8]: ({0}, {1})", m_BufferedState[8].pos.x, m_BufferedState[8].pos.y));
    //    GUILayout.Label(string.Format("differenceState: {0}", m_BufferedState[0].timestamp - m_BufferedState[m_BufferedState.Length-1].timestamp));
    //    GUILayout.Label(string.Format("difference: {0}", m_BufferedState[0].timestamp - interpolationTime2));
    //    GUILayout.Label(string.Format("interpolating: {0}", isInterpolating));

    //    //for (int i = 0; i < Mathf.Min(m_BufferedState.Length, 16); i++)
    //    //{
    //    //    GUILayout.Label(string.Format("m_BufferedState[" + i + "]: ({0}, {1})", m_BufferedState[i].pos.x, m_BufferedState[i].pos.y));
    //    //}
    //}
}