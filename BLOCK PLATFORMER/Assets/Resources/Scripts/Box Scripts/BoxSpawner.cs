using UnityEngine;
using System.Collections.Generic;


public class BoxSpawner : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] BoxTypes boxTypes;
    [SerializeField] int spawnWidth;
    [SerializeField] float spawnTimeMin, spawnTimeMax;

    List<BoxTypes.ItemLabel> allLabels = new(); 
    List<BoxTypes.ItemLabel> labelBag = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnBox", Random.Range(spawnTimeMin, spawnTimeMax), Random.Range(spawnTimeMin, spawnTimeMax));

        for (int i = 0; i < boxTypes.labelSprites.Length; i++)
        {
            allLabels.Add((BoxTypes.ItemLabel)i);
        }

        foreach(BoxTypes.ItemLabel label in allLabels)
        {
            labelBag.Add(label);
        }
    }

    void SpawnBox()
    {
        Vector2 spawnPoint = new Vector2(Mathf.RoundToInt(Random.Range(transform.position.x - spawnWidth / 2, transform.position.x + spawnWidth / 2)), transform.position.y);
        GameObject inst = Instantiate(box, spawnPoint, Quaternion.identity);
        Box boxInst = inst.GetComponent<Box>();

        int randomLabel = Random.Range(0, labelBag.Count);
        boxInst.label = labelBag[randomLabel];
        labelBag.RemoveAt(randomLabel);

        Debug.Log(labelBag.Count);

        if (labelBag.Count == 0)
        {
            Debug.Log("RESET BAG");
            foreach (BoxTypes.ItemLabel label in allLabels)
            {
                labelBag.Add(label);
            }
        }
    }
}
