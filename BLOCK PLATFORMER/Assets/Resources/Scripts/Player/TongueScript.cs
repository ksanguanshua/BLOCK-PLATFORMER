using UnityEngine;
using UnityEngine.InputSystem;
using SaintsField;
using System.Collections;
using Unity.VisualScripting;

public class TongueScript : MonoBehaviour
{
    [System.Serializable]
    public struct Modifiers
    {
        [SerializeField] public float tongueLength;
        [SerializeField] public AnimationCurve tongueExtensionCurve;



    }

    [System.Serializable]
    public struct States
    {
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
        [SerializeField] public LayerMask layerBox;
    }

    [SerializeField][SaintsRow][RichLabel("Modifiers")] public Modifiers M;
    [SerializeField][SaintsRow][RichLabel("References")] public References R;
    [SerializeField][SaintsRow][RichLabel("States")] public States S;

    void Start()
    {
        R.lineRenderer = GetComponentInChildren<LineRenderer>();
        R.movement = GetComponent<Movement>();
    }
    void Update()
    {
        TongueUpdate();
    }
    void OnAttack(InputValue value)
    {
        if (!S.tongueOut)
            ThrowTongue(R.movement.S.movementInput);
    }

    void ThrowTongue(Vector2 direction)
    {
        S.tongueOffset = direction * M.tongueLength;
        S.tongueOut = true;
        StartCoroutine("TongueRetract");
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
        // S.lerpTime += Time.deltaTime;
        // float percent = Mathf.Clamp01(M.tongueExtensionCurve.Evaluate(S.lerpTime));
        // S.tongueEndPoint = Vector3.Lerp(transform.position, (Vector2)transform.position + S.tongueOffset, percent);
        R.lineRenderer.SetPosition(1, S.tongueEndPoint);
        if (S.tongueEndPoint == Vector2.zero)
        {
            S.tongueRetracting = false;
        }
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
    }
}
