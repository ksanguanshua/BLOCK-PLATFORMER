using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<Manager> managers = new();
    public string scene;

    bool runTutorial;

    public float phaseTimer = 60;
    float phaseTimer_t;

    [SerializeField] TextMeshProUGUI timer;
    [SerializeField] TextMeshProUGUI phaseStart;
    [SerializeField] TextMeshProUGUI orderTracker;
    [SerializeField] TextMeshProUGUI cash;

    [SerializeField] GameObject orderTrackerObject;
    [SerializeField] GameObject timerObject;
    [SerializeField] GameObject cashObject;

    int totalWaves;
    [HideInInspector] public int currentWave { get; private set; }
    [HideInInspector] public int playerCash;
    int cashMadeThisShift;

    bool startTimer;

    int ordersToComplete;
    int completedOrders;

     public int cashPerLabelInOrder = 25;
     public int cashPerBonusOrder = 5;

    public GameState gameState { get; private set; }

    public enum GameState
    {
        tutorial,
        setup,
        organize,
        deliver,
        breakTime
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        playerCash = SaveData.instance.playerStats.playerCash;

        cashPerLabelInOrder = SaveData.instance.playerStats.cashPerLabelInOrder;
        cashPerBonusOrder = SaveData.instance.playerStats.cashPerBonusOrder;
    }

    // Update is called once per frame
    void Update()
    {
        StateHandler();
        CashText();
    }

    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUpState(GameState.setup);
    }

    private void OnDisable()
    {
        //SaveData.instance.UpdateStats();
    }

    void CashText()
    {
        cash.text = "$" + playerCash.ToString();
    }

    IEnumerator ShowPhase(string phaseText, int word = 0)
    {
        string[] stringArray = phaseText.Split(' ');
        phaseStart.text = stringArray[word];
        phaseStart.gameObject.GetComponent<Animator>().Play("PhaseStartFadeOut", -1, 0);

        yield return new WaitForSecondsRealtime(1);

        //Debug.Log("Word Index: " + word.ToString());

        if (word + 1 < stringArray.Length)
        {
            StartCoroutine(ShowPhase(phaseText, word + 1));
        }
        else
        {
            switch (gameState)
            {
                case GameState.deliver:

                    phaseStart.text = "";
                    startTimer = true;

                    break;
                case GameState.organize:

                    phaseStart.text = "";
                    startTimer = true;

                    break;
                case GameState.breakTime:

                    phaseStart.text = "";
                    startTimer = false;
                    ChangeScene("Break Room");

                    break;
            }
        }
        
    }

    public void CompletedOrder(int size) //called from delivery zones
    {
        completedOrders++;

        playerCash += size * cashPerLabelInOrder;
        cashMadeThisShift += size * cashPerLabelInOrder;

        int ordersLeft = ordersToComplete - completedOrders;

        if (ordersLeft > 0)
        {
            orderTracker.text = "ORDERS LEFT: " + ordersLeft.ToString();
        }
        else if (ordersLeft == 0)
        {
            orderTracker.text = "COMPLETED!";
            StartCoroutine(ShowPhase("BONUS TIME"));
        }
        else if (ordersLeft < 0)
        {
            playerCash += -ordersLeft * cashPerBonusOrder;
            cashMadeThisShift += -ordersLeft * cashPerBonusOrder;
        }

        Debug.Log("Completed Orders: " + completedOrders.ToString());
    }

    public void ResetSaveData()
    {
        PlayerPrefs.DeleteAll();
    }

    void StateHandler()
    {
        switch (gameState)
        {
            case GameState.tutorial:
                break;

            case GameState.setup:
                break;

            case GameState.organize:

                if (startTimer)
                {
                    PhaseTimer(GameState.deliver);
                    timer.text = Mathf.RoundToInt(phaseTimer_t).ToString();
                }
                else
                {
                    timer.text = "";
                }

                break;

            case GameState.deliver:

                if (startTimer)
                {
                    PhaseTimer(GameState.organize);
                    timer.text = Mathf.RoundToInt(phaseTimer_t).ToString();
                }
                else
                {
                    timer.text = "";
                }

                break;

            case GameState.breakTime:
                break;
        }
    }

    public void SetUpState(GameState state)
    {
        gameState = state;

        switch (state)
        {
            case GameState.tutorial:
                break;

            case GameState.setup:

                Debug.Log(SceneManager.GetActiveScene().name);

                SaveData.instance.UpdateStats();

                switch (SceneManager.GetActiveScene().name)
                {

                    case "Break Room":

                        AudioManager.instance.SetBGMParameter("Scene", 0);
                        AudioManager.instance.PlayBGM();

                        cashObject.SetActive(true);
                        timerObject.SetActive(false);
                        orderTrackerObject.SetActive(false);

                        break;

                    case "Tutorial":

                        AudioManager.instance.SetBGMParameter("Scene", 2);

                        Debug.Log("SET SCENE VALUE");

                        cashObject.SetActive(false);
                        timerObject.SetActive(false);
                        orderTrackerObject.SetActive(false);

                        break;

                    case "Level Select":
                        break;

                    default:

                        Debug.Log("IN LEVEL");
                        AudioManager.instance.SetBGMParameter("Scene", 1);

                        totalWaves = LevelInfo.instance.waves;
                        currentWave = 0;
                        phaseTimer_t = phaseTimer;
                        startTimer = false;

                        cashMadeThisShift = 0;

                        SetUpState(GameState.organize);

                        break;
                }

                break;

            case GameState.organize:

                StartCoroutine(ShowPhase("ORGANIZE THE PACKAGES"));
                orderTracker.text = "";

                orderTrackerObject.SetActive(false);
                cashObject.SetActive(true);
                timerObject.SetActive(true);

                completedOrders = 0;
                ordersToComplete = LevelInfo.instance.ordersPerWave[currentWave];

                break;

            case GameState.deliver:

                StartCoroutine(ShowPhase("FULFILL YOUR ORDERS"));
                orderTracker.text = "ORDERS LEFT: " + ordersToComplete.ToString();

                orderTrackerObject.SetActive(true);
                cashObject.SetActive(true);
                timerObject.SetActive(true);

                break;

            case GameState.breakTime:

                Debug.Log("BREAK TIME");
                
                startTimer = false;

                break;
        }
    }

    void PhaseTimer(GameState state)
    {
        if (phaseTimer_t > 0) 
        {
            phaseTimer_t -= Time.deltaTime;
        }
        else
        {
            phaseTimer_t = phaseTimer;
            startTimer = false;
            timer.text = "";

            switch (state)
            {
                case GameState.organize:

                    if (completedOrders < ordersToComplete)
                    {
                        StartCoroutine(ShowPhase("YOU'RE FIRED!"));
                        playerCash -= Mathf.RoundToInt(cashMadeThisShift / 2);
                        Invoke("ShiftLost", 3);

                        SetUpState(GameState.breakTime);

                        return;
                    }
                    else
                    {
                        if (currentWave + 1 < totalWaves)
                        {
                            currentWave++;
                        }
                        else
                        {
                            StartCoroutine(ShowPhase("SHIFT OVER!"));
                            ShiftWon();

                            SetUpState(GameState.breakTime);

                            return;
                        }
                    }

                    break;

                case GameState.deliver:

                    
                    
                    break;
            }

            SetUpState(state);
        }
    }

    void ShiftLost()
    {
        //StopAllCoroutines();
        //ChangeScene("Break Room");
    }

    void ShiftWon()
    {
        AudioManager.instance.StopBGM();
        AudioManager.instance.PlayVictoryMusic();
        SaveData.instance.UpdateStats();
    }

    public void BoxesCleared()
    {
        playerCash += cashMadeThisShift;
        cashMadeThisShift *= 2;
        StartCoroutine(ShowPhase("BOXES CLEARED!"));
    }
}

public class Manager
{
    public GameManager gameManager;

    public virtual void Awake(GameManager gm)
    {
        gameManager = gm;
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void LateUpdate()
    {

    }
}
