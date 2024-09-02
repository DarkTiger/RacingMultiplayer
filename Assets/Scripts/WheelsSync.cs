using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


[NetworkSettings(channel = 0, sendInterval = 0.5f)]
public class WheelsSync : NetworkBehaviour 
{
    public float lerpRatePosition = 15;
    public float lerpRateRotation = 15;

    [SyncVar] private Quaternion syncWheelFLRotation;
    [SyncVar] private Quaternion syncWheelFRRotation;
    [SyncVar] private Quaternion syncWheelBLRotation;
    [SyncVar] private Quaternion syncWheelBRRotation;
        
    [SerializeField] private Transform wheelFLTransform;
    [SerializeField] private Transform wheelFRTransform;
    [SerializeField] private Transform wheelBLTransform;
    [SerializeField] private Transform wheelBRTransform;

    private Quaternion lastWheelFLRotation;
    private Quaternion lastWheelFRRotation;
    private Quaternion lastWheelBLRotation;
    private Quaternion lastWheelBRRotation;
    public float threshold = 5;


    void FixedUpdate()
    {
        TransmitTransforms();
        LerpTransforms();
    }
    
    void LerpTransforms()
    {
        if (!isLocalPlayer)
        {
            wheelFLTransform.rotation = Quaternion.Lerp(wheelFLTransform.rotation, syncWheelFLRotation, Time.deltaTime * lerpRateRotation);
            wheelFRTransform.rotation = Quaternion.Lerp(wheelFRTransform.rotation, syncWheelFRRotation, Time.deltaTime * lerpRateRotation);
            wheelBLTransform.rotation = Quaternion.Lerp(wheelBLTransform.rotation, syncWheelBLRotation, Time.deltaTime * lerpRateRotation);
            wheelBRTransform.rotation = Quaternion.Lerp(wheelBRTransform.rotation, syncWheelBRRotation, Time.deltaTime * lerpRateRotation);
        }
    }
                
    [Command]
    void CmdProvideTransformsToServer(Quaternion wheelRotFL, Quaternion wheelRotFR, Quaternion wheelRotBL, Quaternion wheelRotBR)
    {
        syncWheelFLRotation = wheelRotFL;
        syncWheelFRRotation = wheelRotFR;
        syncWheelBLRotation = wheelRotBL;
        syncWheelBRRotation = wheelRotBR;
    }

    [Client]
    void TransmitTransforms()
    {
        if (isLocalPlayer)
        {
            if (Quaternion.Angle(wheelFLTransform.rotation, lastWheelFLRotation) > threshold || Quaternion.Angle(wheelFRTransform.rotation, lastWheelFRRotation) > threshold || Quaternion.Angle(wheelBLTransform.rotation, lastWheelBLRotation) > threshold || Quaternion.Angle(wheelBRTransform.rotation, lastWheelBRRotation) > threshold)
            {
                CmdProvideTransformsToServer(wheelFLTransform.rotation, wheelFRTransform.rotation, wheelBLTransform.rotation, wheelBRTransform.rotation);
                lastWheelFLRotation = wheelFLTransform.rotation;
                lastWheelFRRotation = wheelFRTransform.rotation;
                lastWheelBLRotation = wheelBLTransform.rotation;
                lastWheelBRRotation = wheelBRTransform.rotation;
            }
        }
    }
}
