using UnityEngine;
using System.Collections.Generic;

public class Box : MonoBehaviour
{
    [HideInInspector] public Sprite labelSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer labelSR = transform.Find("Label").GetComponent<SpriteRenderer>();
        labelSR.sprite = labelSprite;
    }
}
