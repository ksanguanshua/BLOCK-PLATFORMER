using UnityEngine;

public class BoxTypes : MonoBehaviour
{
    public static BoxTypes instance;

    [SerializeField]
    public Sprite[] labelSprites;

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
