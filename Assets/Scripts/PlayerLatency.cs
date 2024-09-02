using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerLatency : NetworkBehaviour 
{
    private NetworkClient nClient;
    private int latency;
    private Text latencyText;


    public override void OnStartLocalPlayer()
    {
        nClient = GameObject.Find("NetworkManager").GetComponent<NetworkManager>().client;
        latencyText = GameObject.Find("txtPing").GetComponent<Text>();
    }

	void Update () 
    {
        ShowLatency();	
	}

    void ShowLatency()
    {
        //if (isLocalPlayer)
        //{
            //latency = nClient.GetRTT();
            //latencyText.text = "Ping: " + latency.ToString();
        //}
    }
}
