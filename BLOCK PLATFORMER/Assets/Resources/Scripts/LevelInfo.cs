using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public static LevelInfo instance;

    public int waves;
    public int[] ordersPerWave;
    public int[] labelsAddedPerWave;

    private void Awake()
    {
        instance = this;
    }
}
