using UnityEngine;
using System.Collections.Generic;


public class BoxSpawner : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] int spawnWidth;
    [SerializeField] float spawnTimeMin, spawnTimeMax;

    List<Sprite> allLabels = new(); 
    List<Sprite> labelBag = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnBox", Random.Range(spawnTimeMin, spawnTimeMax), Random.Range(spawnTimeMin, spawnTimeMax));

        for (int i = 0; i < BoxTypes.instance.labelSprites.Length; i++)
        {
            allLabels.Add(BoxTypes.instance.labelSprites[i]);
        }

        foreach(Sprite label in allLabels)
        {
            labelBag.Add(label);
        }
    }

    void SpawnBox()
    {
        Vector2 spawnPoint = new Vector2(Mathf.RoundToInt(Random.Range(transform.position.x - spawnWidth / 2, transform.position.x + spawnWidth / 2)), transform.position.y);
        GameObject inst = Instantiate(box, spawnPoint, Quaternion.identity);
        Box boxInst = inst.GetComponent<Box>();

        int randomIndex = Random.Range(0, labelBag.Count);
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
        }
    }
}
