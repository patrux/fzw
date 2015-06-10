using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class NetSync : MonoBehaviour
{
    // Net
    private PhotonView photonView;

    Vector3 realPosition = Vector3.zero;
    Quaternion realRotation = Quaternion.identity;

    public float networkUpdateLerp = 0.15f;

    void Update()
    {
        if (photonView == null)
            photonView = gameObject.GetComponent<PhotonView>();

        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, realPosition, networkUpdateLerp);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, networkUpdateLerp);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            realPosition = (Vector3)stream.ReceiveNext();
            realRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
