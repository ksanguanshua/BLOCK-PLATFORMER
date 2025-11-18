using UnityEngine;
using UnityEngine.InputSystem;
using SaintsField;
using System.Collections;

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
        [SerializeField][ReadOnly] public Vector2 tongueEndPoint;
        [SerializeField][ReadOnly] public float lerpTime;
    }

    [System.Serializable]
    public struct References
    {
        [SerializeField][ReadOnly] public Movement movement;
        [SerializeField][ReadOnly] public LineRenderer lineRenderer;
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
        yield return new WaitForSeconds(0.15f);
        S.tongueOut = false;
        S.tongueOffset = Vector2.zero;
    }

    void TongueUpdate()
    {
        S.tongueEndPoint = Vector3.Lerp(S.tongueEndPoint, S.tongueOffset, 0.9f);
        // S.lerpTime += Time.deltaTime;
        // float percent = Mathf.Clamp01(M.tongueExtensionCurve.Evaluate(S.lerpTime));
        // S.tongueEndPoint = Vector3.Lerp(transform.position, (Vector2)transform.position + S.tongueOffset, percent);
        R.lineRenderer.SetPosition(1, S.tongueEndPoint);
    }
}
