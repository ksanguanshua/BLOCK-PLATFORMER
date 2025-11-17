using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [SerializeField] GameObject box;

    [SerializeField] BoxTypes boxTypes;

    [SerializeField] int spawnWidth;

    [SerializeField] float spawnTimeMin, spawnTimeMax;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnBox", Random.Range(spawnTimeMin, spawnTimeMax), Random.Range(spawnTimeMin, spawnTimeMax));
    }

    void SpawnBox()
    {
        Vector2 spawnPoint = new Vector2(Mathf.RoundToInt(Random.Range(transform.position.x - spawnWidth / 2, transform.position.x + spawnWidth / 2)), transform.position.y);
        GameObject inst = Instantiate(box, spawnPoint, Quaternion.identity);
        Box boxInst = inst.GetComponent<Box>();
        boxInst.label = (BoxTypes.ItemLabel)Random.Range(0, boxTypes.labelSprites.Length);
    }
}
