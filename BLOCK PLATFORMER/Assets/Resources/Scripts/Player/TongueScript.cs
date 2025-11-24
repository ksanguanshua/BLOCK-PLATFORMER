using UnityEngine;
using UnityEngine.InputSystem;
using SaintsField;
using System.Collections;
using Unity.VisualScripting;
using SaintsField.Playa;

public class TongueScript : MonoBehaviour
{
    [System.Serializable]
    public struct Modifiers
    {
        [SerializeField] public float tongueLength;
        [SerializeField] public float throwForce;
        [SerializeField] public float handLerpForce;
        [SerializeField] public AnimationCurve tongueExtensionCurve;
    }

    [System.Serializable]
    public struct States
    {
        [LayoutStart("Box Holding", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public float holdInput;
        [SerializeField][ReadOnly] public Transform heldBox;
        [SerializeField][ReadOnly] public GameObject tongueBox;
        [SerializeField][ReadOnly] public Transform hand;
        [SerializeField][ReadOnly] public Transform grabbableBox;

        [LayoutEnd]
        [LayoutStart("Tongue States", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public bool tongueOut;
        [SerializeField][ReadOnly] public Vector2 tongueLastDir;
        [SerializeField][ReadOnly] public Vector2 tongueOffset;
        [SerializeField][ReadOnly] public bool tongueRetracting;
        [SerializeField][ReadOnly] public Vector2 tongueEndPoint;
        [SerializeField][ReadOnly] public float lerpTime;
    }

    [System.Serializable]
    public struct References
    {
        [SerializeField][ReadOnly] public Movement movement;
        [SerializeField][ReadOnly] public LineRenderer lineRenderer;
        [SerializeField][ReadOnly] public Transform tongueTip;
        [SerializeField] public LayerMask layerBox;
    }

    [SerializeField][SaintsRow][RichLabel("Modifiers")] public Modifiers M;
    [SerializeField][SaintsRow][RichLabel("References")] public References R;
    [SerializeField][SaintsRow][RichLabel("States")] public States S;

    void Start()
    {
        S.hand = transform.Find("Hand");
        R.tongueTip = transform.Find("EndPointOfTongue");
        R.lineRenderer = GetComponentInChildren<LineRenderer>();
        R.movement = GetComponent<Movement>();
    }
    void Update()
    {
        HandMovement();
        TongueUpdate();
        if (S.heldBox != null)
        {
            HeldBox();
        }
    }

    public void GrabInput(float input)
    {
        if (!S.tongueOut)
        {
            if (input == 1 && S.holdInput == 0)
            {
                ThrowTongue(R.movement.S.movementInput);
            }
        }
        if (input == 0 && S.holdInput == 1 && S.heldBox != null) // holding box and ready to throw
        {
            S.heldBox.GetComponent<Collider2D>().enabled = true;
            S.heldBox.GetComponent<Rigidbody2D>().gravityScale = 2;
            S.heldBox.GetComponent<Rigidbody2D>().AddForce(new Vector2(R.movement.S.movementInput.x, R.movement.S.movementInput.y + 1) * M.throwForce, ForceMode2D.Impulse);
            S.heldBox = null;
        }
        else if (input == 0 && S.holdInput == 1 && S.tongueBox != null) // pulling box with tongue
        {
            S.tongueBox.transform.parent = null;
            S.tongueBox.GetComponent<Collider2D>().enabled = true;
            S.tongueBox.GetComponent<Rigidbody2D>().gravityScale = 2;
            S.tongueBox = null;
        }
        S.holdInput = input;
    }

    void HandMovement()
    {
        S.hand.position = Vector2.Lerp(S.hand.position, transform.position + (Vector3)R.movement.S.movementInput, M.handLerpForce);
    }

    void Grab(GameObject box)
    {
        S.hand.GetChild(0).position = box.transform.position;
        S.hand.GetChild(0).rotation = box.transform.rotation;
        S.hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        S.heldBox = box.transform;
        S.hand.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        S.grabbableBox = null;
    }

    void HeldBox()
    {
        S.heldBox.GetComponent<Collider2D>().enabled = false;
        S.heldBox.rotation = S.hand.rotation;
        S.heldBox.position = S.hand.position;
        S.heldBox.GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    void ThrowTongue(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, R.movement.S.movementInput, M.tongueLength, R.layerBox);
        if (hit)
        {
            GameObject boxHit = hit.collider.gameObject;
            S.tongueOffset = boxHit.transform.position - transform.position;
            S.tongueOut = true;
            print("box hit : " + boxHit);
            S.tongueBox = boxHit;
        }
        else
        {
            print("box not hit");
            S.tongueOffset = direction * M.tongueLength;
            S.tongueOut = true;
        }
        StartCoroutine("TongueRetract");
    }

    void BoxToTongueTip(GameObject box)
    {
        box.GetComponent<Collider2D>().enabled = false;
        box.transform.rotation = S.hand.rotation;
        box.GetComponent<Rigidbody2D>().gravityScale = 0;
        box.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        box.transform.parent = R.tongueTip;
        box.transform.localPosition = Vector2.zero;
    }

    IEnumerator TongueRetract()
    {
        R.movement.S.state = Movement.State.Inactive;
        R.movement.S.acceleration = 0;
        R.movement.S.decceleration = 0;
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.gravityScale = 0;
        yield return new WaitForSeconds(0.15f);
        S.tongueRetracting = true;
        yield return new WaitForSeconds(0.05f);
        if (S.tongueBox != null)
            BoxToTongueTip(S.tongueBox);
        rigidbody2D.gravityScale = 1;
        R.movement.S.acceleration = R.movement.M.acceleration;
        R.movement.S.decceleration = R.movement.M.decceleration;
        R.movement.S.state = Movement.State.Base;

        S.tongueRetracting = true;
        S.tongueOut = false;
        S.tongueOffset = Vector2.zero;
    }

    void TongueUpdate()
    {
        S.tongueEndPoint = Vector3.Lerp(S.tongueEndPoint, S.tongueOffset, 0.5f);
        R.tongueTip.position = S.tongueEndPoint + (Vector2)transform.position;
        // S.lerpTime += Time.deltaTime;
        // float percent = Mathf.Clamp01(M.tongueExtensionCurve.Evaluate(S.lerpTime));
        // S.tongueEndPoint = Vector3.Lerp(transform.position, (Vector2)transform.position + S.tongueOffset, percent);
        R.lineRenderer.SetPosition(1, S.tongueEndPoint);
        if (S.tongueEndPoint == Vector2.zero)
        {
            S.tongueRetracting = false;
        }

        if (S.tongueBox != null)
        {
            if (Vector2.Distance(S.tongueBox.transform.position, transform.position) < 1 || S.tongueEndPoint == Vector2.zero)
            {
                Grab(S.tongueBox);
                S.tongueBox.transform.parent = null;
                S.tongueBox = null;
            }
        }
        /*
        if (S.tongueRetracting)
        {
            print("huh");
            Collider2D collider = Physics2D.OverlapCircle(((Vector2)transform.position + S.tongueEndPoint), 1, R.layerBox);
            if (collider != null)
            {
                Rigidbody2D rigidbody2D = collider.GetComponent<Rigidbody2D>();
                print("almost");
                if (rigidbody2D.mass > 1)
                {
                    print("called");
                    GetComponent<Rigidbody2D>().AddForce((((Vector2)transform.position + S.tongueEndPoint) - (Vector2)transform.position) * 30f * Time.deltaTime, ForceMode2D.Impulse);
                }
                else
                {
                    print("called lower");
                    rigidbody2D.AddForce(((Vector2)transform.position - ((Vector2)transform.position + S.tongueEndPoint)) * 30f * Time.deltaTime, ForceMode2D.Impulse);
                }
            }
        }
        */
    }
}
