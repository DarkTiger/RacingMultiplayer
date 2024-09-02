using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;


[NetworkSettings(channel = 0, sendInterval = 2f)]
public class CarController : NetworkBehaviour 
{
    public Vector3 centerOfMass;
    public List<WheelInfo> wheelInfos;
    public float maxSpeed;
    public float decelerationSpeed;
    public float brakeForce;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float forwardStiffness;
    public float sidewaysStiffness;
    public float handbrakedForwardStiffness;
    public float handbrakedSidewaysStiffness;
    private NetworkClient nClient;
    private Text lblSpeed;
    private Text lblPing;
    private float currentSpeed;
    private Rigidbody rb;
    private InputField inputTextChat;
    private RaceManager raceManager;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        nClient = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().client;
        inputTextChat = GameObject.Find("InputTextChat").GetComponent<InputField>();
        
        raceManager = gameObject.GetComponent<RaceManager>();
        
        foreach (WheelInfo wheelInfo in wheelInfos)
        {
            SetWheelFriction(wheelInfo.wheelCollider, forwardStiffness, sidewaysStiffness);
        }

        if (isLocalPlayer)
        {
            lblSpeed = GameObject.Find("lblSpeed").GetComponent<Text>();
            lblPing = GameObject.Find("lblPing").GetComponent<Text>();
        }
    }

    public void ApplyLocalPositionToVisuals(WheelInfo wheelInfo)
    {
        WheelCollider wheelCollider = wheelInfo.wheelCollider;
        Transform visualWheel = wheelCollider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    void Update()
    {
        CalculateSpeed();
        ShowClientPing();
    }

    void FixedUpdate()
    {
        if (isLocalPlayer && !inputTextChat.isFocused && !Cursor.visible && raceManager.raceIsStarted && !raceManager.raceCompleted)
        {
            foreach (WheelInfo wheelInfo in wheelInfos)
            {
                Acceleration(wheelInfo);
                Steering(wheelInfo);
                DecelerationAndBraking(wheelInfo);
                HandBrake(wheelInfo);

                ApplyLocalPositionToVisuals(wheelInfo);
            }
        }

        if (!raceManager.raceIsStarted || raceManager.raceCompleted)
        {
            AutoBraking();
        }
    }

    void ShowClientPing()
    {
        if (isLocalPlayer)
        {
            int clientPing = nClient.GetRTT();
            lblPing.text = "ping: " + clientPing;
        }
    }

    void CalculateSpeed()
    {
        if (isLocalPlayer)
        {
            currentSpeed = Mathf.Round(transform.InverseTransformDirection(rb.velocity).z * 3.6f);
            lblSpeed.text = "km/h: " + currentSpeed.ToString();
        }
    }

    void AutoBraking()
    {
        foreach (WheelInfo wheelInfo in wheelInfos)
        {
            wheelInfo.wheelCollider.brakeTorque = 25000;
        }
    }
        
    void Acceleration(WheelInfo wheelInfo)
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");

        if (wheelInfo.motor)
        {
            wheelInfo.wheelCollider.motorTorque = motor;
        }
    }
    
    void Steering(WheelInfo wheelInfo)
    {
        float steering = (maxSteeringAngle - (currentSpeed / 8)) * Input.GetAxis("Horizontal");

        if (wheelInfo.steering)
        {
            wheelInfo.wheelCollider.steerAngle = steering;
        }
    }
        
    void HandBrake(WheelInfo wheelInfo)
    {
        if (wheelInfo.handbraking)
        {
            if (Input.GetButton("Jump"))
            {
                wheelInfo.wheelCollider.brakeTorque = brakeForce;

                if (currentSpeed > 1)
                {
                    SetWheelFriction(wheelInfo.wheelCollider, handbrakedForwardStiffness, handbrakedSidewaysStiffness);
                }
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                wheelInfo.wheelCollider.brakeTorque = 0;
                SetWheelFriction(wheelInfo.wheelCollider, forwardStiffness, sidewaysStiffness);
            }
        }
    }
    
    void DecelerationAndBraking(WheelInfo wheelInfo)
    {
        if (!Input.GetButton("Vertical"))
        {
            wheelInfo.wheelCollider.motorTorque = 0;
            wheelInfo.wheelCollider.brakeTorque = decelerationSpeed;
        }
        else if (Input.GetAxis("Vertical") < 0 && currentSpeed > 0)
        {
            wheelInfo.wheelCollider.motorTorque = 0;
            wheelInfo.wheelCollider.brakeTorque = brakeForce;
        }
        else
        {
            wheelInfo.wheelCollider.brakeTorque = 0;
        }
    }

    void SetWheelFriction(WheelCollider wheelCollider, float forwardStiffness, float sidewaysStiffness)
    {
        WheelFrictionCurve tempForwardFriction = (WheelFrictionCurve)wheelCollider.forwardFriction;
        tempForwardFriction.stiffness = forwardStiffness;
        wheelCollider.forwardFriction = tempForwardFriction;

        WheelFrictionCurve tempSidewaysFriction = (WheelFrictionCurve)wheelCollider.sidewaysFriction;
        tempSidewaysFriction.stiffness = sidewaysStiffness;
        wheelCollider.sidewaysFriction = tempSidewaysFriction;
    }

  
    [System.Serializable]
    public class WheelInfo
    { 
        public WheelCollider wheelCollider;
        public bool motor;
        public bool steering;
        public bool handbraking;
    }

    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + centerOfMass, 0.25f);
    }
}
