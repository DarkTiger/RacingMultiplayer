using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class MPMenuScript : MonoBehaviour 
{
    [HideInInspector] public GameObject pnlMenuMP;
    private GameObject pnlCreateMP;
    private GameObject pnlInsertJoinPassword;
    [HideInInspector] public GameObject pnlConnectingMsg;
    private InputField inputFieldNickName;
    private InputField inputFieldServerName;
    private InputField inputFieldCreatePassword;
    private InputField inputFieldJoinPassword;
    private Button btnCreateMPServer;
    private Button btnJoinMPServer;
    private Button btnQuitGame;
    private Button btnStartMP;
    private Button btnBackCreateMP;
    private Button btnBackMP;
    private Button btnJoinWithPassword;
    private Button btnCredits;
    private Button btnMultiplayerOnline;
    private Slider sliderMaxPlayers;
    public Slider sliderLapsNumber;
    private Text lblMaxPlayer;
    private Text lblLapsNumber;
    private Toggle togglePrivateServer;
    private Canvas NMCanvasMenu;
    public CustomNetworkManager networkManager;
    public Texture2D sprCursor;
    private Canvas NMCanvasGame;
    private GameObject pnlStartMenu;
    private GameObject creditsText;
    private GameObject pnlTitle;
    private GameObject pnlBottom;
    [HideInInspector] public Text lblJoinWrongPassword;
    [HideInInspector] public string playerName = "";

    
    
    void Start () 
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();

        pnlStartMenu = GameObject.Find("PnlStartMenu");
        btnCredits = GameObject.Find("btnCredits").GetComponent<Button>();
        btnMultiplayerOnline = GameObject.Find("btnMultiplayerOnline").GetComponent<Button>();
        pnlMenuMP = GameObject.Find("PnlMenuMP");
        pnlCreateMP = GameObject.Find("PnlCreateMP");
        pnlInsertJoinPassword = GameObject.Find("PnlInsertJoinPassword");
        pnlConnectingMsg = GameObject.Find("PnlConnectingMsg");
        lblJoinWrongPassword = GameObject.Find("lblJoinWrongPassword").GetComponent<Text>();
        inputFieldNickName = GameObject.Find("inputNickname").GetComponent<InputField>();
        inputFieldServerName = GameObject.Find("inputServerName").GetComponent<InputField>();
        inputFieldCreatePassword = GameObject.Find("InputServerPassword").GetComponent<InputField>();
        inputFieldJoinPassword = GameObject.Find("inputJoinPassword").GetComponent<InputField>();
        btnCreateMPServer = GameObject.Find("btnCreateMP").GetComponent<Button>();
        btnJoinMPServer = GameObject.Find("btnJoinMP").GetComponent<Button>();
        btnQuitGame = GameObject.Find("btnQuitGame").GetComponent<Button>();
        btnBackMP = GameObject.Find("btnBackMP").GetComponent<Button>();
        btnStartMP = GameObject.Find("btnStartMP").GetComponent<Button>();
        btnBackCreateMP = GameObject.Find("btnBackCreateMP").GetComponent<Button>();
        btnJoinWithPassword = GameObject.Find("btnJoinWithPassword").GetComponent<Button>();
        btnMultiplayerOnline = GameObject.Find("btnBackFromPassword").GetComponent<Button>();
        sliderMaxPlayers = GameObject.Find("SliderMaxPlayers").GetComponent<Slider>();
        sliderLapsNumber = GameObject.Find("SliderLapsNumber").GetComponent<Slider>();
        lblMaxPlayer = GameObject.Find("lblMaxPlayers").GetComponent<Text>();
        lblLapsNumber = GameObject.Find("lblLapsNumber").GetComponent<Text>();
        togglePrivateServer = GameObject.Find("TogglePrivateServer").GetComponent<Toggle>();
        creditsText = GameObject.Find("CreditsText");
        pnlTitle = GameObject.Find("PanelTitle");
        pnlBottom = GameObject.Find("PanelBottom");
        NMCanvasMenu = GameObject.Find("NMCanvasMenu").GetComponent<Canvas>();
        NMCanvasGame = GameObject.Find("NMCanvasGame").GetComponent<Canvas>();

        Cursor.SetCursor(sprCursor, Vector2.zero, CursorMode.Auto);
        SetCreditsTextStartPosition();

        pnlInsertJoinPassword.SetActive(false);
        pnlCreateMP.SetActive(false);
        pnlConnectingMsg.SetActive(false);
        pnlMenuMP.SetActive(false);
    }
        
    public void InputNickname_OnEndEdit()
    {
        string nickName = inputFieldNickName.text;

        if (nickName.Trim() != "" && nickName != null)
        {
            playerName = nickName;
            btnCreateMPServer.interactable = true;

            if (networkManager.selectedMatch != null)
            {
                btnJoinMPServer.interactable = true;
            }
            else
            {
                btnJoinMPServer.interactable = false;
            }
        }
    }
        
    public void BtnCreateMP_OnClick()
    {
        pnlMenuMP.SetActive(false);
        pnlCreateMP.SetActive(true);

        inputFieldServerName.text = "Server of " + playerName;
    }

    public void BtnMultiplayerOnline_OnClick()
    {
        pnlMenuMP.SetActive(true);
        pnlStartMenu.SetActive(false);

        networkManager.SearchOnlineMatches();
    }

    public void BtnStartMP_OnClick()
    {
        string name = inputFieldServerName.text;
        uint size = (uint)sliderMaxPlayers.value;
        bool isPrivate = togglePrivateServer.isOn;
        string password = inputFieldCreatePassword.text;

        Text lblErrorCreateMP = GameObject.Find("lblErrorCreateMP").GetComponent<Text>();
        List<MatchInfoSnapshot> matchList = networkManager.matchList;
        foreach (MatchInfoSnapshot match in matchList)
        {
            if (match.name == inputFieldServerName.text)
            {
                lblErrorCreateMP.text = "NAME ALREADY EXISTS!";
                return;
            }
        }

        lblErrorCreateMP.text = "";
        networkManager.StartOnlineServer(name, size, isPrivate, password);
    }

    public void BtnBackMP_OnClick()
    {
        pnlMenuMP.SetActive(false);
        pnlStartMenu.SetActive(true);
    }

    public void BtnBackCreateMP_OnClick()
    {
        Text lblErrorCreateMP = GameObject.Find("lblErrorCreateMP").GetComponent<Text>();
        lblErrorCreateMP.text = "";

        pnlMenuMP.SetActive(true);
        pnlCreateMP.SetActive(false);
    }

    public void BtnCredits_OnClick()
    {
        pnlStartMenu.SetActive(false);
        pnlTitle.SetActive(false);
        pnlBottom.SetActive(false);
        creditsText.GetComponent<Text>().enabled = true;
    }

    public void btnQuit_OnClick()
    {
        Application.Quit();
    }
    
    public void BtnJoinMP_OnClick()
    {
        MatchInfoSnapshot selectedMatch = networkManager.selectedMatch;

        if (selectedMatch != null)
        {
            if (selectedMatch.isPrivate)
            {
                pnlInsertJoinPassword.SetActive(true);
            }
            else
            {
                networkManager.JoinOnlineMatch("");
            }
        }
    }

    public void BtnRefreshListMP_OnClick()
    {
        networkManager.SearchOnlineMatches();
    }

    public void SliderMaxPlayers_OnValueChanged()
    {
        lblMaxPlayer.text = "max players: " + (int)sliderMaxPlayers.value;
    }

    public void SliderLapsNumber_OnValueChanged()
    {
        lblLapsNumber.text = "laps number: " + (int)sliderLapsNumber.value;
    }

    public void TogglePrivateServer_OnValueChanged()
    {
        inputFieldCreatePassword.interactable = togglePrivateServer.isOn;
    }

    public void btnJoinWithPassword_OnClick()
    {
        string password = inputFieldJoinPassword.text;
        networkManager.JoinOnlineMatch(password); 
    }

    public void btnBackFromPassword_OnClick()
    {
        HidePnlInsertJoinPassword();
    }

    public void ShowPnlInsertJoinPassword()
    {
        pnlInsertJoinPassword.SetActive(true);
    }

    public void HidePnlInsertJoinPassword()
    {
        inputFieldJoinPassword.text = "";
        pnlInsertJoinPassword.SetActive(false);
    }

    public void SetCreditsTextStartPosition()
    {
        Vector3 posTemp = Vector3.zero;
        posTemp.y = -(Screen.height + 200);
        creditsText.transform.localPosition = posTemp;
    }

    public void ShowMPMenu()
    {
        if (!pnlStartMenu.active)
        {
            Cursor.visible = true;
            NMCanvasMenu.enabled = true;
            pnlMenuMP.SetActive(true);
            pnlCreateMP.SetActive(false);
        }
    }

	void Update () 
    {
        if (!pnlStartMenu.active && !pnlMenuMP.active && !pnlCreateMP.active)
        {
            Vector3 posTemp = creditsText.transform.position;
            posTemp.y += 0.01f;
            creditsText.transform.position = posTemp;

            if (creditsText.transform.localPosition.y >= 1400)
            {
                SetCreditsTextStartPosition();
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                SetCreditsTextStartPosition();
                pnlStartMenu.SetActive(true);
                pnlTitle.SetActive(true);
                pnlBottom.SetActive(true);
                creditsText.GetComponent<Text>().enabled = false;
            }
        }

        if (SceneManager.GetActiveScene().name != "RaceTrack")
        {
            NMCanvasGame.enabled = false;
            Cursor.visible = true;
        }

        if (networkManager.selectedMatch != null && !inputFieldNickName.isFocused && inputFieldNickName.text.Trim() != "")
        {
            btnJoinMPServer.interactable = true;
        }
        else
        {
            btnJoinMPServer.interactable = false;
        }
        
        if (inputFieldNickName.isFocused)
        {
            btnCreateMPServer.interactable = false;
            btnJoinMPServer.interactable = false;
        }

        if (pnlCreateMP != null)
        {    
            if (inputFieldServerName.text.Trim() != "" && (togglePrivateServer.isOn && inputFieldCreatePassword.text.Trim() != "" || !togglePrivateServer.isOn))
            {
                btnStartMP.interactable = true;
            }
            else
            {
                btnStartMP.interactable = false;
            }
        }
	}
}
