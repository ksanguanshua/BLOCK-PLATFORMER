using UnityEngine;

public class BoxTypes : MonoBehaviour
{
    public enum ItemLabel
    {
        apple,
        banana,
        carrot,
        cheese,
        cherries
    }

    [SerializeField]
    public Sprite[] labelSprites;

    private void Awake()
    {
        labelSprites = Resources.LoadAll<Sprite>("Sprites/Labels");
    }
}
