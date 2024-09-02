using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


[NetworkSettings(channel = 0, sendInterval = 0.1f)]
public class PlayerSync : NetworkBehaviour 
{
    public float lerpRatePosition = 15;
    public float lerpRateRotation = 15;
    [SyncVar] private Vector3 syncPosition;
    [SyncVar] private Quaternion syncRotation;
    [SerializeField] private Transform myTransform;
        
    private Vector3 lastPosition;
    public float thresholdPosition = 0.5f;
    private Quaternion lastRotation;
    public float thresholdRotation = 5f;


    void FixedUpdate()
    {
        TransmitPosition();
        TransmitRotation();
        LerpTransforms();
    }

    void LerpTransforms()
    {
        if (!isLocalPlayer)
        {
            myTransform.position = Vector3.Lerp(myTransform.position, syncPosition, Time.deltaTime * lerpRatePosition);
            myTransform.rotation = Quaternion.Lerp(myTransform.rotation, syncRotation, Time.deltaTime * lerpRateRotation);
        }
    }
       
    [Command]
    void CmdProvidePositionToServer(Vector3 position)
    {
        syncPosition = position;
    }

    [Command]
    void CmdProvideRotationToServer(Quaternion rotation)
    {
        syncRotation = rotation;
    }
                    
    [Client]
    void TransmitPosition()
    {
        if (isLocalPlayer && Vector3.Distance(myTransform.position, lastPosition) > thresholdPosition)
        {
            CmdProvidePositionToServer(myTransform.position);
            lastPosition = myTransform.position;
        }
    }

    [Client]
    void TransmitRotation()
    {
        if (isLocalPlayer && Quaternion.Angle(myTransform.rotation, lastRotation) > thresholdRotation)
        {
            CmdProvideRotationToServer(myTransform.rotation);
            lastRotation = myTransform.rotation;
        }
    }
}
