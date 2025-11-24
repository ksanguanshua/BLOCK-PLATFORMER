using UnityEngine;
using System.Collections.Generic;

public class Box : MonoBehaviour
{
    [SerializeField]
    public BoxTypes.ItemLabel label;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer labelSprite = transform.Find("Label").GetComponent<SpriteRenderer>();
        labelSprite.sprite = GameObject.Find("Box Types").GetComponent<BoxTypes>().labelSprites[(int)label];
    }
}
