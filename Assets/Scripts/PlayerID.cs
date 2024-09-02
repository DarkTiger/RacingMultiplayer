using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;


[NetworkSettings(channel = 0, sendInterval = 2f)]
public class PlayerID : NetworkBehaviour 
{
    [SyncVar] public string playerName;
    public GameObject objPlayerName;
    private NetworkInstanceId playerNetID;
    private Transform myTransform;
    private GameObject networkManager;

    
    public override void OnStartClient()
    {
        objPlayerName = (GameObject)GameObject.Instantiate(objPlayerName, transform.position, transform.rotation);
        objPlayerName.GetComponent<SetOnPlayer>().target = transform;
    }

    public override void OnStartLocalPlayer()
    {
        GetNetIdentity();
        SetIdentity();         
    }

    void Awake() 
    {
        networkManager = GameObject.Find("NetworkManager");
        myTransform = transform;
    }
	
	void Update() 
    {
        if (myTransform.name == "" || myTransform.name == "Player(Clone)")
        {
            SetIdentity();
        }
	}

    [Client]
    void GetNetIdentity()
    {
        playerNetID = GetComponent<NetworkIdentity>().netId;
        CmdTellServerMyIdentity(MakeUniqueIdentity());
    }

    void SetIdentity()
    {
        if (!isLocalPlayer)
        {
            myTransform.name = playerName;
        }
        else
        {
            myTransform.name = MakeUniqueIdentity();
        }
    }

    string MakeUniqueIdentity()
    {
        string uniqueName = GameObject.Find("NMCanvasMenu").GetComponent<MPMenuScript>().playerName;
        
        if (isServer)
        {
            uniqueName = "*" + uniqueName;
        }

        return uniqueName;
    }

    [Command]
    void CmdTellServerMyIdentity(string name)
    {
        playerName = name;
        SetObjPlayerName();
    }

    void SetObjPlayerName()
    {
        objPlayerName.transform.name = playerName + " Name";
        
        TextMesh[] textMeshes = objPlayerName.GetComponentsInChildren<TextMesh>();
        foreach (TextMesh textMesh in textMeshes)
        {
            textMesh.text = playerName;
        }
    }
    
    public override void OnNetworkDestroy()
    {
        Destroy(GameObject.Find(name + " Name"));
    }
}
