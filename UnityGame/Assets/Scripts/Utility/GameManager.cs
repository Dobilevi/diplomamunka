using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Cpp;

public class GameManager : MonoBehaviour
{
    public bool isServer = false;
    // The script that manages all others
    public static GameManager instance = null;

    public UIManager uiManager = null;

    public GameObject player = null;

    // The current player score in the game
    private static int gameManagerScore = 0;

    // Static getter/setter for player score
    public static int score
    {
        get
        {
            return gameManagerScore;
        }
        set
        {
            gameManagerScore = value;
        }
    }

    public string playerName = "";
    public string serverAddress = NetConstants.defaultServerAddress;
    public int serverPort = NetConstants.defaultServerPort;
    public int port = NetConstants.defaultServerPort;
    public int botCount = 0;
    public int maxPlayers = 10;
    public int aiControlled = 0;

    public enum ErrorMessage
    {
        None,
        Unknown,
        Port,
        CppServerStart,
        ServerFull,
        Timeout,
        Disconnected,
        Closed
    }
    public static ErrorMessage errorMessage = ErrorMessage.None;
    public Text errorMessageText;

    // The highest score obtained by this player
    public int highScore = 0;

    public bool gameIsWinnable = true;
    public int enemiesToDefeat = 10;
    
    // The number of enemies defeated in game
    private int enemiesDefeated = 0;

    public bool printDebugOfWinnableStatus = true;
    public int gameVictoryPageIndex = 0;
    public GameObject victoryEffect;

    //The number of enemies observed by the game manager in this scene at start up"
    private int numberOfEnemiesFoundAtStart;

    public int chosenShip = 0;
    public List<GameObject> ships;
    public List<GameObject> multiplayerShips;
    public List<GameObject> multiplayerBotShips;

    public string selectedLevel = "0";

    private void ShowErrorMessage()
    {
        if (errorMessage != ErrorMessage.None)
        {
            switch (errorMessage)
            {
                case ErrorMessage.Unknown:
                {
                    errorMessageText.text = "An unknown error occured!";
                    break;
                }
                case ErrorMessage.Port:
                {
                    errorMessageText.text = "Couldn't bind to port!";
                    break;
                }
                case ErrorMessage.CppServerStart:
                {
                    errorMessageText.text = "Couldn't start Cpp server!";
                    break;
                }
                case ErrorMessage.ServerFull:
                {
                    errorMessageText.text = "The server is full!";
                    break;
                }
                case ErrorMessage.Timeout:
                {
                    errorMessageText.text = "Server timeout!";
                    break;
                }
                case ErrorMessage.Disconnected:
                {
                    errorMessageText.text = "Disconnected from the server!";
                    break;
                }
                case ErrorMessage.Closed:
                {
                    errorMessageText.text = "The server closed!";
                    break;
                }
                default:
                {
                    break;
                }
            }

            errorMessage = ErrorMessage.None;
            instance.uiManager.GoToPageByName("ErrorMessage");
        }
    }

    private void Awake()
    {
        Application.runInBackground = true;

        if (instance == null)
        {
            instance = this;
            
            if (PlayerPrefs.HasKey("player_name"))
            {
                playerName = PlayerPrefs.GetString("player_name", "Player");
                Debug.Log("Player name: " + playerName);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("PlayerName").GetComponent<InputFieldGrabber>().SetInputField(playerName);
                }
            }
            if (PlayerPrefs.HasKey("server_address"))
            {
                serverAddress = PlayerPrefs.GetString("server_address", NetConstants.defaultServerAddress);
                Debug.Log("Server address: " + serverAddress);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("ServerAddress").GetComponent<InputFieldGrabber>().SetInputField(serverAddress);
                }
            }
            if (PlayerPrefs.HasKey("server_port"))
            {
                serverPort = PlayerPrefs.GetInt("server_port", NetConstants.defaultServerPort);
                Debug.Log("Server Port: " + serverPort);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("ServerPort").GetComponent<InputFieldGrabber>().SetInputField(serverPort.ToString());
                }
            }
            if (PlayerPrefs.HasKey("port"))
            {
                port = PlayerPrefs.GetInt("port", NetConstants.defaultServerPort);
                Debug.Log("Port: " + port);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("Port").GetComponent<InputFieldGrabber>().SetInputField(port.ToString());
                }
            }
            if (PlayerPrefs.HasKey("bot_count"))
            {
                botCount = PlayerPrefs.GetInt("bot_count", 0);
                Debug.Log("Bot Count: " + botCount);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("BotCount").GetComponent<InputFieldGrabber>().SetInputField(botCount.ToString());
                }
            }
            if (PlayerPrefs.HasKey("max_players"))
            {
                maxPlayers = PlayerPrefs.GetInt("max_players", 10);
                Debug.Log("Max Players: " + maxPlayers);
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("MaxPlayers").GetComponent<InputFieldGrabber>().SetInputField(maxPlayers.ToString());
                }
            }
            if (PlayerPrefs.HasKey("ai_control"))
            {
                aiControlled = PlayerPrefs.GetInt("ai_control");
                Debug.Log($"ai_control: {aiControlled}");
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    GameObject.FindGameObjectWithTag("AIControl").GetComponent<ToggleScript>().SetInitialValue(aiControlled);
                }
            }
            if (PlayerPrefs.HasKey("highscore"))
            {
                highScore = PlayerPrefs.GetInt("highscore");
            }
            if (PlayerPrefs.HasKey("score"))
            {
                score = PlayerPrefs.GetInt("score");
            }

            if (SceneManager.GetActiveScene().name == "LevelMultiplayerClient")
            {
                chosenShip = PlayerPrefs.GetInt("ship");
                GameObject player;
                if (aiControlled != 0)
                {
                    player = Instantiate(multiplayerBotShips[chosenShip]);
                }
                else
                {
                    player = Instantiate(multiplayerShips[chosenShip]);

                    player.GetComponentsInChildren<ShootingController>();
                }
                instance.player = player;
                GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<CameraController>().target = player.transform;
            }
            else if (SceneManager.GetActiveScene().name == "LevelMultiplayerServer")
            {
                
            }
            else if (SceneManager.GetActiveScene().name == "LevelUnityMultiplayer")
            {
                
            }
            else if ((SceneManager.GetActiveScene().name != "MainMenu") && !isServer)
            {
                chosenShip = PlayerPrefs.GetInt("ship");
                GameObject player = Instantiate(ships[chosenShip]);
                instance.player = player;
                GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<CameraController>().target = player.transform;
            }

            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                ShowErrorMessage();
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                ShowErrorMessage();
            }
            DestroyImmediate(this);
        }
    }

    private void Start()
    {
        HandleStartUp();
    }

    void HandleStartUp()
    {
        if (!isServer)
        {
            UpdateUIElements();
        }

        if (printDebugOfWinnableStatus)
        {
            FigureOutHowManyEnemiesExist();
        }
    }

    private void FigureOutHowManyEnemiesExist()
    {
        List<EnemySpawner> enemySpawners = FindObjectsOfType<EnemySpawner>().ToList();
        List<Enemy> staticEnemies = FindObjectsOfType<Enemy>().ToList();

        int numberOfInfiniteSpawners = 0;
        int enemiesFromSpawners = 0;
        int enemiesFromStatic = staticEnemies.Count;
        foreach(EnemySpawner enemySpawner in enemySpawners)
        {
            if (enemySpawner.spawnInfinite)
            {
                numberOfInfiniteSpawners += 1;
            }
            else
            {
                enemiesFromSpawners += enemySpawner.maxSpawn;
            }
        }
        numberOfEnemiesFoundAtStart = enemiesFromSpawners + enemiesFromStatic;

        if (gameIsWinnable)
        {
            if (numberOfInfiniteSpawners > 0)
            {
                Debug.Log("There are " + numberOfInfiniteSpawners + " infinite spawners " + " so the level will always be winnable, "
                    + "\nhowever you sshould still playtest for timely completion");
            }
            else if (!SceneManager.GetActiveScene().name.Contains("Multiplayer") && (enemiesToDefeat > numberOfEnemiesFoundAtStart))
            {
                Debug.LogWarning("There are " + enemiesToDefeat + " enemies to defeat but only " + numberOfEnemiesFoundAtStart + 
                    " enemies found at start \nThe level can not be completed!");
            }
            else
            {
                Debug.Log("There are " + enemiesToDefeat + " enemies to defeat and " + numberOfEnemiesFoundAtStart +
                    " enemies found at start \nThe level can completed");
            }
        }
    }

    public void IncrementEnemiesDefeated()
    {
        enemiesDefeated++;
        if (enemiesDefeated >= enemiesToDefeat && gameIsWinnable)
        {
            LevelCleared();
        }
    }

    private void OnApplicationQuit()
    {
        SaveHighScore();
        ResetScore();
    }

    public static void AddScore(int scoreAmount)
    {
        score += scoreAmount;
        if (score > instance.highScore)
        {
            SaveHighScore();
        }
        UpdateUIElements();
    }
    
    public static void ResetScore()
    {
        PlayerPrefs.SetInt("score", 0);
        score = 0;
    }

    public static void SaveHighScore()
    {
        if (score > instance.highScore)
        {
            PlayerPrefs.SetInt("highscore", score);
            instance.highScore = score;
        }
        UpdateUIElements();
    }

    public void SavePref(string pref, string value)
    {
        switch (pref)
        {
            case "player_name":
            {
                instance.playerName = value;
                PlayerPrefs.SetString(pref, instance.playerName);
                break;
            }
            case "server_address":
            {
                instance.serverAddress = value;
                PlayerPrefs.SetString(pref, instance.serverAddress);
                break;
            }
            case "server_port":
            {
                if (!int.TryParse(value, out instance.serverPort))
                {
                    instance.serverPort = NetConstants.defaultServerPort;
                } 
                PlayerPrefs.SetInt(pref, instance.serverPort);
                break;
            }
            case "port":
            {
                if (!int.TryParse(value, out instance.port))
                {
                    instance.port = NetConstants.defaultServerPort;
                } 
                PlayerPrefs.SetInt(pref, instance.port);
                break;
            }
            case "bot_count":
            {
                if (!int.TryParse(value, out instance.botCount))
                {
                    instance.botCount = 0;
                } 
                PlayerPrefs.SetInt(pref, instance.botCount);
                break;
            }
            case "max_players":
            {
                if (!int.TryParse(value, out instance.maxPlayers))
                {
                    instance.maxPlayers = 10;
                } 
                PlayerPrefs.SetInt(pref, instance.maxPlayers);
                break;
            }
            default:
            {
                break;
            }
        }
    }

    public static void ResetHighScore()
    {
        PlayerPrefs.SetInt("highscore", 0);
        if (instance != null)
        {
            instance.highScore = 0;
        }
        UpdateUIElements();
    }

    public static void UpdateUIElements()
    {
        if (instance != null && instance.uiManager != null)
        {
            instance.uiManager.UpdateUI();
        }
    }

    public void LevelCleared()
    {
        PlayerPrefs.SetInt("score", score);
        if (uiManager != null)
        {
            player.SetActive(false);
            uiManager.allowPause = false;
            uiManager.GoToPage(gameVictoryPageIndex);
            if (victoryEffect != null)
            {
                Instantiate(victoryEffect, transform.position, transform.rotation, null);
            }
        }     
    }

    [Header("Game Over Settings:")]
    [Tooltip("The index in the UI manager of the game over page")]
    public int gameOverPageIndex = 0;
    [Tooltip("The game over effect to create when the game is lost")]
    public GameObject gameOverEffect;

    // Whether or not the game is over
    [HideInInspector]
    public bool gameIsOver = false;

    public void GameOver()
    {
        gameIsOver = true;
        if (gameOverEffect != null)
        {
            Instantiate(gameOverEffect, transform.position, transform.rotation, null);
        }
        if (uiManager != null)
        {
            uiManager.allowPause = false;
            uiManager.GoToPage(gameOverPageIndex);
        }
    }

    public static void SetShip(int shipIndex)
    {
        PlayerPrefs.SetInt("ship", shipIndex);
    }

    public void SetLevel(string level)
    {
        selectedLevel = level;
    }

    public void LoadSelectedLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Level" + selectedLevel);
    }
}
