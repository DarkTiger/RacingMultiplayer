using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ServerController : NetworkBehaviour 
{

    public override void OnStartClient()
    {
        Debug.Log("new player");
        //RpcAddMessageOnClientsChat();
    }



    


}
