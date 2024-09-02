using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class CustomNetworkManager : NetworkManager
{
    public Sprite sprMatchUISelected;
    public Sprite sprMatchUIDefault;
    public Font textFont;
    private MPMenuScript mpMenuScript;
    [HideInInspector] public MatchInfoSnapshot selectedMatch = null;
    [HideInInspector] public List<MatchInfoSnapshot> matchList = new List<MatchInfoSnapshot>();
    private MatchInfo joinedMatch = null;
    private MatchInfo myCreatedMatch;
    protected ulong currentMatchID;
    protected ulong currentNodeID;


    void Awake()
    {
        if (FindObjectsOfType<NetworkManager>().Length > 1)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        mpMenuScript = GameObject.Find("NMCanvasMenu").GetComponent<MPMenuScript>();
        StartMatchMaker();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject spawnPoint = GameObject.Find("SpawnPoint" + (ClientScene.objects.Count + 1).ToString());
        GameObject player = (GameObject)Instantiate(playerPrefab, new Vector3(0, 50, 0), spawnPoint.transform.rotation);

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        joinedMatch = null;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerNetworkSetup>().CmdSendMessage(conn.playerControllers[0].gameObject.name, "Disconnected");
            NetworkServer.DestroyPlayersForConnection(conn);
            return;
        }
    }

    public void Disconnect()
    {
        this.matchMaker.DestroyMatch((NetworkID)currentMatchID, 1, OnMatchDestroyed);
        joinedMatch = null;
    }

    public void OnMatchDestroyed(bool success, string extendedInfo)
    {
        Debug.Log("___OnMatchMakerDrop: " + success + " " + extendedInfo);
        this.matchMaker.DropConnection((NetworkID)currentMatchID, (NodeID)currentNodeID, 1, OnConnectionDropped);

        StopHost();
        StopMatchMaker();

        SearchOnlineMatches();
        GameObject.Find("NMCanvasMenu").GetComponent<Canvas>().enabled = true;

        if (GameObject.Find("pnlInGameMenu") != null)
        {
            GameObject.Find("pnlInGameMenu").SetActive(false);
        }

        if (GameObject.Find("PnlCreateMP") != null)
        {
            GameObject.Find("PnlCreateMP").SetActive(false);
        }
    }

    public void OnConnectionDropped(bool success, string extendedInfo)
    {
        Debug.Log("___OnConnectionDropped " + success + " " + extendedInfo);
    }

    public void StartOnlineServer(string name, uint size, bool isPrivate, string password)
    {
        StartMatchMaker();

        string matchName = name;

        if (isPrivate)
        {
            matchName = "(Private) " + name;
        }

        matchMaker.CreateMatch(matchName, size, !isPrivate, password, "", "", 0, 1, OnMatchCreate);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchResponse)
    {
        if (success)
        {
            joinedMatch = matchResponse;
            myCreatedMatch = matchResponse;

            currentMatchID = (UInt64)matchResponse.networkId;
            currentNodeID = (UInt64)matchResponse.nodeId;

            //Utility.SetAccessTokenForNetwork(matchResponse.networkId, new NetworkAccessToken(matchResponse.accessToken.ToString()));
            //NetworkServer.Listen(matchResponse, 9000);

            StartHost(matchResponse);
        }
    }

    public void SearchOnlineMatches()
    {
        StopMatchMaker();

        if (matchMaker == null)
        {
            StartMatchMaker();

            selectedMatch = null;
            matchList.Clear();

            matchMaker.ListMatches(0, 20, "", true, 0, 1, OnMatchList);
        }
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchListResponse)
    {
        matchList = matchListResponse;

        if (GameObject.FindGameObjectsWithTag("MatchButton").Length > 0)
        {
            GameObject[] goButtons = GameObject.FindGameObjectsWithTag("MatchButton");

            foreach (GameObject goButton in goButtons)
            {
                Destroy(goButton);
            }
        }

        if (matchListResponse != null && matchListResponse.Count > 0)
        {
            GameObject ScrollViewMatches = GameObject.Find("ScrollViewMatches");

            int yOffset = 130;
            for (int i = 0; i < matchListResponse.Count; i++)
            {
                GameObject btn = new GameObject("btnMatch" + (i + 1));
                btn.tag = "MatchButton";
                btn.AddComponent<Image>().sprite = sprMatchUIDefault;
                btn.AddComponent<Button>();
                btn.transform.SetParent(ScrollViewMatches.transform);
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollViewMatches.GetComponent<RectTransform>().sizeDelta.x - 4, 30);
                ColorBlock cb = btn.GetComponent<Button>().colors;
                cb.normalColor = new Color(1, 1, 1, 0.75f);
                cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1);
                btn.GetComponent<Button>().colors = cb;

                string matchName = matchListResponse[i].name;
                GameObject txtBtnMatchName = new GameObject("txtBtnMatchName" + (i + 1));
                Text txtName = txtBtnMatchName.AddComponent<Text>();
                txtName.GetComponent<Text>().text = matchName.ToLower() + "             players: " + matchListResponse[i].currentSize + "/" + matchListResponse[i].maxSize + "          track: Race Track";
                txtName.GetComponent<Text>().color = Color.black;
                txtName.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                txtName.transform.SetParent(btn.transform);
                txtName.transform.localScale = new Vector3(1, 1, 1);
                txtName.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollViewMatches.GetComponent<RectTransform>().sizeDelta.x - 20, 30);
                txtName.font = textFont;
                txtName.fontSize = 20;

                btn.GetComponent<Button>().onClick.AddListener(delegate () { SelectMatch(btn, matchName, matchListResponse); });
                btn.transform.localPosition = new Vector3(0, yOffset, 0);
                yOffset -= 30;

            }
        }
    }

    public void JoinOnlineMatch(string password)
    {
        if (selectedMatch != null)
        {
            mpMenuScript.pnlMenuMP.SetActive(false);
            mpMenuScript.pnlConnectingMsg.SetActive(true);

            matchMaker.JoinMatch(selectedMatch.networkId, password, "", "", 0, 1, OnMatchJoined);
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchJoin)
    {
        if (success)
        {
            joinedMatch = matchJoin;
            currentMatchID = (System.UInt64)matchJoin.networkId;
            currentNodeID = (System.UInt64)matchJoin.nodeId;

            mpMenuScript.lblJoinWrongPassword.text = "";

            Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessToken.ToString()));
            StartClient(matchJoin);

            if (GameObject.FindGameObjectsWithTag("MatchButton").Length > 0)
            {
                GameObject[] goButtons = GameObject.FindGameObjectsWithTag("MatchButton");

                foreach (GameObject goButton in goButtons)
                {
                    Destroy(goButton);
                }
            }
        }
        else
        {
            mpMenuScript.pnlConnectingMsg.SetActive(false);
            mpMenuScript.lblJoinWrongPassword.text = "WRONG PASSWORD!";
        }
    }

    void SelectMatch(GameObject btn, string matchName, List<MatchInfoSnapshot> matchList)
    {
        if (GameObject.FindGameObjectsWithTag("MatchButton").Length > 0)
        {
            GameObject[] goButtons = GameObject.FindGameObjectsWithTag("MatchButton");

            foreach (GameObject goButton in goButtons)
            {
                goButton.GetComponent<Image>().sprite = sprMatchUIDefault;
                ColorBlock cb = goButton.GetComponent<Button>().colors;
                cb.normalColor = new Color(1, 1, 1, 0.75f);
                cb.colorMultiplier = 1f;
                goButton.GetComponent<Button>().colors = cb;
            }
        }

        btn.GetComponent<Image>().sprite = sprMatchUISelected;
        ColorBlock cbs = btn.GetComponent<Button>().colors;
        cbs.normalColor = new Color(1, 1, 1, 1);
        cbs.colorMultiplier = 5f;
        btn.GetComponent<Button>().colors = cbs;

        for (int i = 0; i < matchList.Count; i++)
        {
            if (matchList[i].name == matchName)
            {
                selectedMatch = matchList[i];
            }
        }
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("SERVER ERROR: " + errorCode);
        StopHost();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("CLIENT ERROR: " + errorCode);
        StopClient();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);
        base.OnClientDisconnect(conn);
        joinedMatch = null;

        StopMatchMaker();
        SearchOnlineMatches();

        GameObject.Find("NMCanvasMenu").GetComponent<Canvas>().enabled = true;

        if (GameObject.Find("pnlInGameMenu") != null)
        {
            GameObject.Find("pnlInGameMenu").SetActive(false);
        }

        if (GameObject.Find("PnlCreateMP") != null)
        {
            GameObject.Find("PnlCreateMP").SetActive(false);
        }
    }

    public static void MyDelay(int seconds)
    {
        DateTime dt = DateTime.Now + TimeSpan.FromSeconds(seconds);
        do { } while (DateTime.Now < dt);
    }

    void OnApplicationQuit()
    {
        this.matchMaker.DestroyMatch((NetworkID)currentMatchID, 1, OnMatchDestroyed);
        MyDelay(2);
    }
}
