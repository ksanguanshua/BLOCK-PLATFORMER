using UnityEngine;
using System.Collections.Generic;


public class BoxSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] box;
    [SerializeField] int spawnWidth;
    [SerializeField] float spawnTimeMin, spawnTimeMax;

    [SerializeField] int totalBoxes = 10;
    int currentBoxes;

    [SerializeField] int totalLabels = 1;

    List<Sprite> allLabels = new(); //every possible label
    List<Sprite> labelBag = new(); //the bag of every possible label
    List<Sprite> currentLabels = new(); //the current labels being produced

    public List<Sprite> producedBoxes { get; private set; } = new();

    GameManager.GameState currentGameState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        producedBoxes.Clear();
        currentBoxes = totalBoxes;

        for (int i = 0; i < BoxTypes.instance.labelSprites.Length; i++)
        {
            allLabels.Add(BoxTypes.instance.labelSprites[i]);
        }

        foreach (Sprite label in allLabels)
        {
            labelBag.Add(label);
        }
        
        for (int i = 0; i < LevelInfo.instance.labelsAddedPerWave[GameManager.instance.currentWave]; i++)
        {
            Sprite randomLabel = labelBag[Random.Range(0, labelBag.Count)];
            currentLabels.Add(randomLabel);
            labelBag.Remove(randomLabel);
        }
    }

    private void Update()
    {
        StateChanged();
    }

    void SpawnBox()
    {
        if (currentBoxes > 0)
        {
            Vector2 spawnPoint = new Vector2(Mathf.RoundToInt(Random.Range(transform.position.x - spawnWidth / 2, transform.position.x + spawnWidth / 2)), transform.position.y);
            GameObject inst = Instantiate(box[Random.Range(0, box.Length)], spawnPoint, Quaternion.identity);
            Box boxInst = inst.GetComponent<Box>();

            Sprite randomLabel = currentLabels[Random.Range(0, currentLabels.Count)];
            boxInst.labelSprite = randomLabel;

            BoxTypes.instance.createdBoxes.Add(randomLabel); //adding boxes to list for delivery zone to read

            currentBoxes--;
        }
        else
        {
            CancelInvoke("SpawnBox");
        }

        /*int randomIndex = Random.Range(0, labelBag.Count);
        boxInst.labelSprite = labelBag[randomIndex];
        labelBag.RemoveAt(randomIndex);

        //Debug.Log(labelBag.Count);

        if (labelBag.Count == 0)
        {
            //Debug.Log("RESET BAG");
            foreach (Sprite label in allLabels)
            {
                labelBag.Add(label);
            }
        }*/
    }

    void StateChanged()
    {
        if (GameManager.instance.gameState != currentGameState)
        {
            CancelInvoke();

            switch (GameManager.instance.gameState)
            {
                case GameManager.GameState.tutorial:
                    break;
                case GameManager.GameState.organize:

                    currentLabels.Clear();

                    for (int i = 0; i < LevelInfo.instance.labelsAddedPerWave[GameManager.instance.currentWave]; i++)
                    {
                        Sprite randomLabel = labelBag[Random.Range(0, labelBag.Count)];
                        currentLabels.Add(randomLabel);
                        labelBag.Remove(randomLabel);
                    }

                    currentBoxes = totalBoxes;
                    InvokeRepeating("SpawnBox", Random.Range(spawnTimeMin, spawnTimeMax), Random.Range(spawnTimeMin, spawnTimeMax));

                    break;
                case GameManager.GameState.deliver:
                    break;
                case GameManager.GameState.breakTime:
                    break;
            }

            currentGameState = GameManager.instance.gameState;
        }
    }
}
