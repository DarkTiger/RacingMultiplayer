using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Timers;
using System;
using UnityEngine.EventSystems;



[NetworkSettings(channel = 0, sendInterval = 2f)]
public class PlayerNetworkSetup : NetworkBehaviour
{   
    [SyncVar] int nConnectedPlayers;
    public Camera playerCam;
    public GameObject InGameHUDManager;

    private MPMenuScript mpMenuScript;
    private bool needUpdate = false;
    private int secondsToUpdate = 1;
    private Canvas networkManagerMenuCanvas;
    private Canvas networkManagerGameCanvas;
    private bool cameraIsCreated = false;
    private float secondsNow = 0;
    private bool secondsTaked = false;
    private InputField inputTextChat;
    private Text lblNPlayers;
    public CustomNetworkManager networkManager;


    public override void OnStartClient()
    {
        mpMenuScript = GameObject.Find("NMCanvasMenu").GetComponent<MPMenuScript>();
        networkManagerMenuCanvas = GameObject.Find("NMCanvasMenu").GetComponent<Canvas>();
        networkManagerGameCanvas = GameObject.Find("NMCanvasGame").GetComponent<Canvas>();
        networkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();

        needUpdate = true;
    }

    public override void OnStartLocalPlayer()
    {
        networkManagerMenuCanvas.enabled = false;
        networkManagerGameCanvas.enabled = true;

        if (isLocalPlayer)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        inputTextChat = GameObject.Find("InputTextChat").GetComponent<InputField>();
        GameObject.Find("Scene Camera").SetActive(false);
        GameObject objName = GetComponent<PlayerID>().objPlayerName;
        HideMyName(objName);

        CmdSendMessage(transform.name, "Connected");

        mpMenuScript.pnlMenuMP.SetActive(true);
        mpMenuScript.pnlConnectingMsg.SetActive(false);
    }
    
    void Start()
    {
        lblNPlayers = GameObject.Find("lblNPlayers").GetComponent<Text>();         
        InGameHUDManager = GameObject.Find("NMCanvasGame");
        Cursor.visible = false;
    }

    void SetPlayerCamToObjNames(Camera camera)
    {
        GameObject[] objNames = GameObject.FindGameObjectsWithTag("PlayerName");
        foreach (GameObject obj in objNames)
        {
            obj.GetComponent<SetOnPlayer>().playerCam = camera;
        }
    }

    void HideMyName(GameObject objName)
    {
        MeshRenderer[] meshRenderers = objName.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (isServer)
        {
            lblNPlayers.text = "players: " + networkManager.numPlayers;
        }
        else
        {
            lblNPlayers.text = "players: " + ClientScene.objects.Count;
        }
         
        if (inputTextChat != null)
        {                   
            if (!inputTextChat.isFocused && inputTextChat.text.Trim() != "")
            {
                CmdSendMessage(transform.name, inputTextChat.text.Trim());
                inputTextChat.text = "";
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        if (needUpdate)
        {
            if (Mathf.Round(Time.time) > secondsNow)
            {
                secondsNow = Mathf.Round(Time.time) + secondsToUpdate;
                
                if (secondsTaked)
                {
                    SetNewClientName();
                    needUpdate = false;
                    secondsTaked = false;
                    secondsNow = 0;
                }

                secondsTaked = true;
            }   
        }
    }
        
    void SetNewClientName()
    {
        GameObject[] playersNamesObj = GameObject.FindGameObjectsWithTag("PlayerName");
        foreach (GameObject obj in playersNamesObj)
        {
            string name = obj.GetComponent<SetOnPlayer>().target.name;
            obj.name = name + " Name";
                
            TextMesh[] textMeshes = obj.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in textMeshes)
            {
                textMesh.text = name;
            }
        }
    }

    [Command]
    public void CmdSendMessage(string playerName, string chatMessage)
    {
        chatMessage = playerName + ": " + chatMessage;
        RpcWriteChatMessage(chatMessage);
    }
        
    [ClientRpc]
    void RpcWriteChatMessage(string chatMessage)
    {
        GameObject hudManager = GameObject.Find("NMCanvasGame");
        hudManager.GetComponent<InGameHUDManager>().AddTextChat(chatMessage);
    }

    [Command]
    public void CmdProvideIncrementNPlayers()
    {
        nConnectedPlayers = Network.connections.Length;
        Debug.Log(nConnectedPlayers);
    }

    [Client]
    void OnChangeNPlayers()
    {
        CmdProvideIncrementNPlayers();
    }
}

