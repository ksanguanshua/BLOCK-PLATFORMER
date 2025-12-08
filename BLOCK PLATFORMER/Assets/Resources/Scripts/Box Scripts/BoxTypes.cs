using UnityEngine;
using System.Collections.Generic;

public class BoxTypes : MonoBehaviour
{
    public static BoxTypes instance;

    [SerializeField]
    public Sprite[] labelSprites;

    public List<Sprite> createdBoxes = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);

        instance.labelSprites = Resources.LoadAll<Sprite>("Sprites/Labels/boxLabels");
    }
}
