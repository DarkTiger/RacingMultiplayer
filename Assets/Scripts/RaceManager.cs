using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;


[NetworkSettings(channel = 0, sendInterval = 1f)]
public class RaceManager : NetworkBehaviour 
{
    [HideInInspector] [SyncVar] public int totalLaps = 0;
    [HideInInspector] [SyncVar] public int secondToStartRace = 10;
    [HideInInspector] [SyncVar] public bool raceCanStart = false;
    [HideInInspector] [SyncVar] public LapTime bestTime = null;
    [HideInInspector] public bool newRace = false;
    [HideInInspector] public bool raceIsStarted = false;
    private int lastCheckpoint;
    public LapTime lapTime;
    public PersonalStatistics statistics;
    private Text lblCheckpoints;
    private Text lblLaps;
    private Text lblLapTime;
    private Text lblEventMessage;
    private Text lblEventMessageShadow;
    public Text lblBestLapTime;
    public Text lblBestLapTimeShadow;
    private bool increaseLap;
    private bool startLineReleased = true;
    private bool timeStarted = false;
    private bool finishedMessageShowed = false;
    private float endTime = 0;
    private CustomNetworkManager networkManager;
    private int currentPlayers = 0;
    private int spawnIndex = 1;
    private bool spawned = false;
    private float messageEndTime = 0;
    private bool messageIsShowed;
    private PlayerNetworkSetup playerNetworkSetup;
    public bool raceCompleted = false;
    GameObject[] players;
    bool winnerElected = false;
    string winnerName = "";
    private BoxCollider[] myBoxColliders;
    private WheelCollider[] myWheelColliders;
    private bool onPositioning = false;
    private GameObject spawnPoint = null;


    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
        playerNetworkSetup = gameObject.GetComponent<PlayerNetworkSetup>();
        
        if (isLocalPlayer)
        {
            lblCheckpoints = GameObject.Find("lblCheckpoints").GetComponent<Text>();
            lblLaps = GameObject.Find("lblLaps").GetComponent<Text>();
        }      

        lblLapTime = GameObject.Find("lblLapTime").GetComponent<Text>();

        lblEventMessage = GameObject.Find("lblEventMessageWhite").GetComponent<Text>();
        lblEventMessageShadow = GameObject.Find("lblEventMessage").GetComponent<Text>();
        lblEventMessage.text = "";
        lblEventMessageShadow.text = "";

        lblBestLapTime = GameObject.Find("lblBestLapTimeWhite").GetComponent<Text>();
        lblBestLapTimeShadow = GameObject.Find("lblBestLapTime").GetComponent<Text>();
        lblBestLapTime.text = "";
        lblBestLapTimeShadow.text = "";

        if (isLocalPlayer)
        {
            myBoxColliders = GetComponents<BoxCollider>();
            myWheelColliders = GetComponentsInChildren<WheelCollider>();
        }
        
        if (isServer)
        {
            totalLaps = (int)GameObject.Find("SliderLapsNumber").GetComponent<Slider>().value;
        }

        if (isServer)
        {
            currentPlayers = networkManager.numPlayers;

            if (currentPlayers > 1)
            {
                CmdStartRace();
            }
        }
        else
        {
            currentPlayers = ClientScene.objects.Count;
        }

        players = GameObject.FindGameObjectsWithTag("Player");
        SetPlayerOnStartingGrid();
    }

    public void NewRace(int lapsNumber)
    {
        if (isServer && currentPlayers > 1)
        {
            totalLaps = (int)GameObject.Find("SliderLapsNumberInGame").GetComponent<Slider>().value;
            CmdNewRace();
        }
    }

    [Command]
    public void CmdNewRace()
    {
        RpcNewRace();
    }

    [ClientRpc]
    public void RpcNewRace()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<RaceManager>().newRace = true;
            player.GetComponent<RaceManager>().bestTime.minutes = 0;
            player.GetComponent<RaceManager>().bestTime.seconds = 0;
            player.GetComponent<RaceManager>().bestTime.milliseconds= 0;
            player.GetComponent<RaceManager>().statistics.resetTimes();
            player.GetComponent<RaceManager>().totalLaps = totalLaps;
        }
    }

    void Update()
    {
        ResetPlayerPosition();
        CountdownToStartRace();
        LapTimeCalculation();
        CheckStartLineTrigger();
        CheckCheckpointTrigger();
        ShowLaps();

        int playersCount;
        if (isServer)
        {
            playersCount = networkManager.numPlayers;
        }
        else
        {
            playersCount = ClientScene.objects.Count;
        }
                
        if (newRace || currentPlayers < playersCount)
        {
            WriteEventMessage("", 0);
            lblBestLapTime.text = "";
            lblBestLapTimeShadow.text = "";
            SetRaceStatus(false);
            SetPlayerOnStartingGrid();
            endTime = Time.time + 11;
            currentPlayers = playersCount;
            players = GameObject.FindGameObjectsWithTag("Player");
            newRace = false;
            raceCompleted = false;

            if (isServer)
            {
                if (isLocalPlayer)
                {
                    playerNetworkSetup.CmdSendMessage(name, "STARTED A NEW RACE!");
                    ResetStatisticRace();
                    CmdStartRace();
                }
            }
        }
    }

    void ResetPlayerPosition()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            if (raceIsStarted)
            {
                if (isLocalPlayer)
                {
                    Vector3 newPosition;
                    Vector3 newRotation;

                    if (lastCheckpoint > 0)
                    {
                        newPosition = GameObject.Find("Checkpoint" + lastCheckpoint).transform.position;
                        newRotation = GameObject.Find("Checkpoint" + lastCheckpoint).transform.localRotation.eulerAngles;
                    }
                    else
                    {
                        newPosition = GameObject.Find("StartLine").transform.position;
                        newRotation = GameObject.Find("StartLine").transform.localRotation.eulerAngles;
                    }

                    float newY = newRotation.y + 90;
                    newRotation.y = newY;

                    transform.position = newPosition;
                    transform.rotation = Quaternion.identity;

                    transform.Rotate(newRotation);

                    Rigidbody r = GetComponent<Rigidbody>();
                    r.velocity = Vector3.zero;
                    r.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    void CountdownToStartRace()
    {
        if (raceCanStart && !raceCompleted && ((isServer && networkManager.numPlayers > 1) || (!isServer && ClientScene.objects.Count > 1)))
        {
            if (isServer)
            {
                if (endTime == 0)
                {
                    endTime = Time.time + 11;
                }

                secondToStartRace = (int)(endTime - Time.time);

                if (secondToStartRace < 0)
                {
                    secondToStartRace = 0;
                }
            }

            if (secondToStartRace > 0)
            {
                SetRaceStatus(false);
                WriteEventMessage("race start in: " + secondToStartRace.ToString(), 0);
            }
            else
            {
                if (!raceCompleted)
                {
                    SetRaceStatus(true);
                    WriteEventMessage("", 0);
                }
            }
        }
        else
        {
            if (!raceCompleted)
            {
                WriteEventMessage("WAITING PLAYERS..", 0);
            }
            else
            {
                bool allPlayerFinished = true;

                foreach (GameObject player in players)
                {
                    if (player != null)
                    {
                        LapTime playerLapTime = player.GetComponent<RaceManager>().bestTime;
                        if (playerLapTime.seconds == 0 && playerLapTime.milliseconds == 0)
                        {
                            allPlayerFinished = false;
                        }
                    }
                }

                if (!allPlayerFinished)
                {
                    WriteEventMessage("RACE FINISHED! WAITING OTHER PLAYERS..", 0);
                }
                else
                {
                    CalcolateTheWinner();
                    WriteEventMessage("THE WINNER IS: " + winnerName + "!", 0);
                }
            }
        }
    }

    void SetRaceStatus(bool isStarted)
    {
        if (!isStarted)
        {
            lapTime = new LapTime();

            timeStarted = false;
            lapTime.minutes = 0;
            lapTime.seconds = 0;
            lapTime.milliseconds = 0;
            lapTime.lap = 1;
            lastCheckpoint = 0;
            lblLapTime.text = "lap time: 0:00:000";

            if (lblLaps != null)
            {
                lblLaps.text = "lap: " + lapTime.lap + "/" + totalLaps;
            }
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<RaceManager>().raceIsStarted = isStarted;
        }
    }

    [Command]
    public void CmdStartRace()
    {
        raceIsStarted = false;
        raceCanStart = true;
    }


    void SetCollidersSolid(bool isSolid)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            BoxCollider[] boxColliders = player.GetComponents<BoxCollider>();
            foreach (BoxCollider boxCollider in boxColliders)
            {
                boxCollider.isTrigger = !isSolid;
            }

            WheelCollider[] wheelColliders = player.GetComponentsInChildren<WheelCollider>();
            foreach (WheelCollider wheelCollider in wheelColliders)
            {
                wheelCollider.isTrigger = !isSolid;
            }
        }
    }

    void SetPlayerOnStartingGrid()
    {
        if (isLocalPlayer)
        {
            if (spawnPoint == null)
            {
                onPositioning = true;
                SetCollidersSolid(false);

                spawnIndex = 1;
                spawned = false;

                while (!spawned)
                {
                    GameObject spawn = GameObject.Find(("SpawnPoint" + spawnIndex.ToString()));
                    Collider[] colliders = Physics.OverlapSphere(spawn.transform.position, 1f);

                    if (colliders.Length > 1)
                    {
                        if (colliders[0].gameObject.name == name)
                        {
                            spawn = GameObject.Find(("SpawnPoint" + spawnIndex.ToString()));
                            colliders = Physics.OverlapSphere(spawn.transform.position, 1);
                            transform.position = spawn.transform.position;
                            transform.rotation = spawn.transform.rotation;
                            spawned = true;
                            SetCollidersSolid(true);
                            onPositioning = false;
                        }
                        else
                        {
                            spawnIndex += 1;
                        }
                    }
                    else
                    {
                        spawn = GameObject.Find(("SpawnPoint" + spawnIndex.ToString()));
                        colliders = Physics.OverlapSphere(spawn.transform.position, 1);
                        transform.position = spawn.transform.position;
                        transform.rotation = spawn.transform.rotation;
                        spawned = true;
                        SetCollidersSolid(true);
                        onPositioning = false;
                    }
                }
            }
            else
            {
                transform.position = spawnPoint.transform.position;
                transform.rotation = spawnPoint.transform.rotation;
                spawned = true;
                SetCollidersSolid(true);
                onPositioning = false;
            }

            if (spawnPoint == null)
            {
                spawnPoint = GameObject.Find(("SpawnPoint" + spawnIndex.ToString()));
            } 
        }        
    }

    void LapTimeCalculation()
    {
        if (timeStarted)
        {
            lapTime.milliseconds += (int)(Time.deltaTime * 1000f);

            if (lapTime.milliseconds >= 1000)
            {
                lapTime.milliseconds = 0;
                lapTime.seconds += 1;
            }

            if (lapTime.seconds >= 60)
            {
                lapTime.seconds = 0;
                lapTime.minutes += 1;
            }

            string secondsString = lapTime.seconds.ToString();
            if (secondsString.Length < 2)
            {
                secondsString = "0" + lapTime.seconds;
            }

            string millisecondsString = lapTime.milliseconds.ToString();
            while (millisecondsString.Length < 3)
            {
                millisecondsString = "0" + millisecondsString;
            }

            lblLapTime.text = "lap time: " + lapTime.minutes + ":" + secondsString + ":" + millisecondsString;
        }
    }

    void CheckStartLineTrigger()
    {
        if (timeStarted && !startLineReleased && lastCheckpoint == 7 && raceIsStarted)
        {
            statistics.AddNewTime(lapTime);
            
            if ((lapTime.lap + 1) > totalLaps && !raceCompleted)
            {
                timeStarted = false;
                raceIsStarted = false;
                ShowBestTime("RACE FINISHED! BEST TIME: ", statistics.GetBestTime(), true);
                raceCompleted = true;

                CmdSendMyBestTime(gameObject.name, statistics.GetBestTime());
  
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players)
                {
                    player.GetComponent<RaceManager>().raceCompleted = true;
                }
            }
            else if ((lapTime.lap + 1) <= totalLaps)
            {
                ShowBestTime("BEST TIME: ", statistics.GetBestTime(), false);
                lapTime = new LapTime();
                lapTime.milliseconds = 0;
                lapTime.seconds = 0;
                lapTime.minutes = 0;
                lapTime.lap += 1;
                lastCheckpoint = 0;
            }
        }
    }

    [ClientRpc]
    void RpcSendMyBestTime(string playerName, LapTime myBestTime)
    {
        bestTime = statistics.GetBestTime();
        GameObject player = GameObject.Find(playerName);
        player.GetComponent<RaceManager>().bestTime = myBestTime;
    }

    [Command]
    void CmdSendMyBestTime(string playerName, LapTime bestTime)
    {
        RpcSendMyBestTime(playerName, bestTime);
    }

    void ShowBestTime(string text, LapTime bestTime, bool raceFinished)
    {
        string secondsString = bestTime.seconds.ToString();
        if (secondsString.Length < 2)
        {
            secondsString = "0" + bestTime.seconds;
        }

        string millisecondsString = bestTime.milliseconds.ToString();
        while (millisecondsString.Length < 3)
        {
            millisecondsString = "0" + millisecondsString;
        }

        lblBestLapTime.text = text + bestTime.minutes + ":" + secondsString + ":" + millisecondsString;
        lblBestLapTimeShadow.text = text + bestTime.minutes + ":" + secondsString + ":" + millisecondsString;

        if (raceFinished)
        {
            playerNetworkSetup.CmdSendMessage(name, "(Finish) Lap " + lapTime.lap + " : " + lapTime.minutes + ":" + secondsString + ":" + millisecondsString);
        }
        else 
        {
            playerNetworkSetup.CmdSendMessage(name, "Lap " + lapTime.lap + " : " + lapTime.minutes + ":" + secondsString + ":" + millisecondsString);
        }
    }

    void WriteEventMessage(string message, int seconds)
    {
        lblEventMessage.text = message;
        lblEventMessageShadow.text = message;
    }

    void CheckCheckpointTrigger()
    {
        if (lblCheckpoints != null)
        {
            lblCheckpoints.text = "checkpoint: " + lastCheckpoint + "/7";
        }
    }
        
    void ShowLaps()
    {
        if (lapTime.lap <= totalLaps)
        {
            if (lblLaps != null)
            {
                lblLaps.text = "lap: " + lapTime.lap + "/" + totalLaps;
            }
        }
        else
        {
            if (lblLaps != null)
            {
                lblLaps.text = "finished!";
            }

            if (!finishedMessageShowed)
            {
                WriteEventMessage("FINISHED!", 3); 
                finishedMessageShowed = true;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (onPositioning)
            {
                SetPlayerOnStartingGrid();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "StartLine")
        {
            startLineReleased = true;
        }
    }
        
    void OnTriggerEnter(Collider other)
    {
        if (isLocalPlayer)
        {
            if (other.tag == "StartLine")
            {
                startLineReleased = false;

                if (!timeStarted && raceIsStarted)
                {
                    timeStarted = true;
                }
            }

            for (int i = 0; i < 8; i++)
			{
			    if (other.tag == "Checkpoint" + (i+1).ToString())
                {
                    if (lastCheckpoint == i)
                    {
                        lastCheckpoint = (i+1);
                    }
                }
			}
        }
    }

    void CalcolateTheWinner()
    {
        if (!winnerElected)
        {
            string[] playersTimeName = new string[currentPlayers];
            int index = 0;

            foreach (GameObject player in players)
            {
                LapTime playerBestTime = player.GetComponent<RaceManager>().bestTime;

                int minutesValue = playerBestTime.minutes * 100000;
                int secondsValue = playerBestTime.seconds * 1000;
                int timeValue = minutesValue + secondsValue + bestTime.milliseconds;

                playersTimeName[index] = timeValue.ToString() + "§" + player.gameObject.name;
                index += 1;
            }

            Array.Sort(playersTimeName);
            winnerName = playersTimeName[0].Substring(playersTimeName[0].IndexOf("§")+1);
            winnerElected = true;
        }
    }

    void ResetStatisticRace()
    {
        winnerElected = false;
        bestTime.minutes = 0;
        bestTime.seconds = 0;
        bestTime.milliseconds = 0;
        statistics.resetTimes();
    }


    [Serializable]
    public class LapTime
    {
        public int lap = 1;
        public int minutes = 0;
        public int seconds = 0;
        public int milliseconds = 0;
    }


    [Serializable]
    public class PersonalStatistics
    {
        public List<LapTime> lapsTimes;
        public LapTime bestTime;
                

        public void resetTimes()
        {
            lapsTimes.Clear();
            bestTime.milliseconds = 0;
            bestTime.seconds = 0;
            bestTime.minutes = 0;
        }

        public void AddNewTime(LapTime newLapTime)
        {
            lapsTimes.Add(newLapTime);
            CalculateBestTime(newLapTime);
        }

        void CalculateBestTime(LapTime newLapTime)
        {
            foreach (LapTime lapTime in lapsTimes)
            {
                if (bestTime.minutes == 0 && bestTime.seconds == 0 && bestTime.milliseconds == 0)
                {
                    bestTime = newLapTime;
                    return;
                }
                else
                {
                    if (newLapTime.minutes < bestTime.minutes)
                    {
                        bestTime = newLapTime;
                        return;
                    }
                    else if (newLapTime.minutes == bestTime.minutes)
                    {
                        if (newLapTime.seconds < bestTime.seconds)
                        {
                            bestTime = newLapTime;
                            return;
                        }
                        else if (newLapTime.seconds == bestTime.seconds)
                        {
                            if (newLapTime.milliseconds < bestTime.milliseconds)
                            {
                                bestTime = newLapTime;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public LapTime GetBestTime()
        {
            return bestTime;
        }
    }
}
