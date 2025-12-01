using UnityEngine;

public class Static : Box
{
    [SerializeField] float throwTime;
    [SerializeField] float staticTime;

    public override void OnThrow()
    {
        base.OnThrow();
        Invoke("BecomeStatic", throwTime);
    }

    public override void OnHold()
    {
        base.OnHold();
        CancelInvoke("BecomeStatic");
    }

    void BecomeStatic()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        gameObject.layer = LayerMask.NameToLayer("Ground");
        GetComponent<Animator>().Play("staticBoxTimer");
        //Invoke("BecomeDynamic", staticTime);
    }

    void BecomeDynamic()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        gameObject.layer = LayerMask.NameToLayer("Box");
        GetComponent<Animator>().Play("staticBoxIdle");
    }
}
