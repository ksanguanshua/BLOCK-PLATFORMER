using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<Manager> managers = new();

    public string scene;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SceneHandler();
        AddManagersToList(); //adds all the managers to the manager list

        foreach (Manager manager in managers) //calls start in every manager
        {
            manager.Awake(this);
        }
    }

    void Start()
    {
        foreach (Manager manager in managers) //calls start in every manager
        {
            manager.Start();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Manager manager in managers) //calls update in every manager
        {
            //Debug.Log("running update in: " + manager.ToString());

            manager.Update();
        }

        foreach (Manager manager in managers) //calls update in every manager
        {
            //Debug.Log("running late update in: " + manager.ToString());

            manager.LateUpdate();
        }
    }

    void AddManagersToList()
    {

    }

    void SceneHandler()
    {
        scene = SceneManager.GetActiveScene().name;
    }

    public void ResetSaveData()
    {
        PlayerPrefs.DeleteAll();
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
