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

    public virtual void OnHold()
    {

    }
    
    public virtual void BeingHeld()
    {

    }

    public virtual void OnThrow()
    {

    }

    public virtual void OnTouch()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GetComponent<Rigidbody2D>().linearVelocity.magnitude > 5)
        {
            AudioManager.instance.PlayBoxBump();
        }
    }
}
