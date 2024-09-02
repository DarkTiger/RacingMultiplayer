using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class InGameHUDManager : NetworkBehaviour 
{
    public Texture2D sprCursor;
    private Text lblTextChat;
    private InputField inputTextChat;
    private GameObject pnlInGameMenu;
    private Button btnInGameDisconnect;
    private List<string> chatMessages;
    public CustomNetworkManager networkManager;
    private Canvas NMCanvasGame;
    private Text lblNPlayers;
    private Text lblFPS;
    private Text lblLapsNumber;
    private Text lblLapsNumberShadow;
    private Slider sliderLapsNumber;
    private float deltaTime;
    private int fps;
        
    
    void Start()
    { 
        chatMessages = new List<string>();
        pnlInGameMenu = GameObject.Find("pnlInGameMenu");
        lblTextChat = GameObject.Find("lblTextChat").GetComponent<Text>();
        lblFPS = GameObject.Find("lblFps").GetComponent<Text>();
        lblLapsNumber = GameObject.Find("lblLapsNumberInGameWhite").GetComponent<Text>();
        lblLapsNumberShadow = GameObject.Find("lblLapsNumberInGame").GetComponent<Text>();
        sliderLapsNumber = GameObject.Find("SliderLapsNumberInGame").GetComponent<Slider>();
        inputTextChat = GameObject.Find("InputTextChat").GetComponent<InputField>();
        btnInGameDisconnect = GameObject.Find("btnInGameDisconnect").GetComponent<Button>();
        networkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
        NMCanvasGame = GameObject.Find("NMCanvasGame").GetComponent<Canvas>();
        
        Cursor.SetCursor(sprCursor, Vector2.zero, CursorMode.Auto);
        pnlInGameMenu.SetActive(false);
    }

    public void AddTextChat(string text)
    {
        string text1 = "";
        string text2 = "";

        if (text.Length > 35)
        {
            if (chatMessages.Count > 6)
            {
                chatMessages.RemoveAt(0);
                chatMessages.RemoveAt(1);
            }

            text1 = text.Substring(0, 25);
            text2 = text.Substring(25);

            chatMessages.Add(text1);
            chatMessages.Add(text2);
        }
        else
        {
            if (chatMessages.Count > 7)
            {
                chatMessages.RemoveAt(0);
            }

            chatMessages.Add(text);
        }
              
        lblTextChat.text = "";
        for (int i = 0; i < chatMessages.Count; i++)
        {
            lblTextChat.text += "\n" + chatMessages[i];
        }     
    }

    public void BtnInGameNewRace_OnClick()
    {
        GameObject playerHost = GameObject.FindGameObjectWithTag("Player");
        int nLaps = (int)GameObject.Find("SliderLapsNumberInGame").GetComponent<Slider>().value;

        playerHost.GetComponent<RaceManager>().NewRace(nLaps);

        Cursor.visible = !pnlInGameMenu.activeSelf;
        pnlInGameMenu.SetActive(!pnlInGameMenu.activeSelf);
    }

    public void SliderInGameLapsNumber_OnValueChanged()
    {
        lblLapsNumber.text = "laps number: " + (int)sliderLapsNumber.value;
        lblLapsNumberShadow.text = "laps number: " + (int)sliderLapsNumber.value;
    }
    
    public void BtnInGameDisconnect_OnClick()
    {
        NMCanvasGame.enabled = false;
        networkManager.Disconnect();
    }
        
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T) && !inputTextChat.isFocused && SceneManager.GetActiveScene().name == "RaceTrack")
        {
            inputTextChat.Select();
        }
        else if (SceneManager.GetActiveScene().name == "MenuMultiplayer")
        {
            chatMessages.Clear();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.visible = !pnlInGameMenu.activeSelf;
            pnlInGameMenu.SetActive(!pnlInGameMenu.activeSelf);   
        }

        FPSCalculation();
    }
    
    void FPSCalculation()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        fps = (int)(1.0f / deltaTime);
        lblFPS.text = "fps: " + fps;
    }
}
