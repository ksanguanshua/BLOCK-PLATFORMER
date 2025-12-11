using UnityEngine;

public class SaveData : MonoBehaviour
{
    public static SaveData instance;

    TongueScript tongueScript;
    MovementPlatformer2D movementScript;

    public PlayerStats playerStats { get; private set; } = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            tongueScript = player.GetComponent<TongueScript>();
            movementScript = player.GetComponent<MovementPlatformer2D>();
        }

        LoadFromJSON();

        if (!playerStats.initFile)
        {
            playerStats.initFile = true;
            UpdateStats();
        }
    }

    private void Start()
    {
        
    }

    public void SaveToJSON()
    {
        string playerStatsString = JsonUtility.ToJson(playerStats);
        string filePath = Application.persistentDataPath + "/PlayerStats.json";
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, playerStatsString);
    }

    public void LoadFromJSON()
    {
        string filePath = Application.persistentDataPath + "/PlayerStats.json";

        if (System.IO.File.Exists(filePath))
        {
            string playerStatsString = System.IO.File.ReadAllText(filePath);
            playerStats = JsonUtility.FromJson<PlayerStats>(playerStatsString);
        }
        else
        {
            UpdateStats();
        }
    }

    public void UpdateStats()
    {
        if (tongueScript != null)
        {
            playerStats.tongueModifiers = tongueScript.M;
        }

        if (movementScript != null)
        {
            playerStats.movementModifiers = movementScript.M;
        }

        playerStats.phaseTime = GameManager.instance.phaseTimer;

        playerStats.cashPerLabelInOrder =  GameManager.instance.cashPerLabelInOrder;
        playerStats.cashPerBonusOrder = GameManager.instance.cashPerBonusOrder;

        playerStats.playerCash = GameManager.instance.playerCash;

        SaveToJSON();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UpdateStats();
        }
    }
}

public class PlayerStats
{
    public bool initFile;

    public TongueScript.Modifiers tongueModifiers;
    public MovementPlatformer2D.Modifiers movementModifiers;

    public int cashPerLabelInOrder = 25;
    public int cashPerBonusOrder = 5;

    public float phaseTime = 60;
    public int playerCash = 0;

    public int[] upgrades = new int[10];
}
