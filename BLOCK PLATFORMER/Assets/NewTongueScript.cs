using UnityEngine;
using UnityEngine.InputSystem;
using SaintsField;
using System.Collections;
using Unity.VisualScripting;
using SaintsField.Playa;
using UnityEngine.UIElements;
using Unity.Mathematics;

public class NewTongueScript : MonoBehaviour
{
    public enum PlayerState
    {
        Base, //no box
        PullRecoil, //box pulled to player (little recoil anim and prep for powerthrow)
        PullToo, //player is pulling themselves to the box (after tap during tongue touch)
        BoxHold, //player is holding box
        TongueTouch, //tongue is out (repeat press pulls)
        TongueHeld, //tongue swing (on hold)
    }

    [System.Serializable]
    public struct Modifiers
    {
        [SerializeField][ReadOnly] public float tongueLength;
        [SerializeField][ReadOnly] public float tongueShootTime;
        [SerializeField][ReadOnly] public float tongueLerpSpeed;
    }

    [System.Serializable]
    public struct States
    {
        [SerializeField] public PlayerState stateMachine;
        [LayoutStart("Box Holding", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public float holdInput;
        [SerializeField][ReadOnly] public float holdInputTime;
        [SerializeField][ReadOnly] public float holdInputTimeThreshold;
        [SerializeField][ReadOnly] public Vector2 lastFacingDir;
        [SerializeField][ReadOnly] public float lastFacingDirLR;
        [SerializeField][ReadOnly] public bool canTurn;
        [SerializeField][ReadOnly] public Transform heldBox;
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
        [LayoutStart("IMPORTANT", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public GameObject interactedObject;
        [SerializeField][ReadOnly] public Transform tongueTip;
        [SerializeField][ReadOnly] public Transform goalTongueTip;
        [LayoutStart("Components", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public Movement movement;
        [SerializeField][ReadOnly] public Animator anim;
        [LayoutStart("Visuals", ELayout.FoldoutBox)]
        [SerializeField] public LineRenderer lineRenderer;
        [SerializeField] public LineRenderer lineRendererVisualLayer1;
        [SerializeField] public LineRenderer lineRendererVisualLayer2;
        [LayoutStart("Layers", ELayout.FoldoutBox)]
        [SerializeField] public LayerMask layerBox;
        [SerializeField] public LayerMask layerGround;
    }

    [SerializeField][SaintsRow][RichLabel("Modifiers")] public Modifiers M;
    [SerializeField][SaintsRow][RichLabel("References")] public References R;
    [SerializeField][SaintsRow][RichLabel("States")] public States S;

    public void GrabInput(float input)
    {
        switch (S.stateMachine)
        {
            case PlayerState.Base:
                if (S.holdInput == 0 && input == 1)
                {
                    ThrowTongue(S.lastFacingDir);
                }
                break;
            case PlayerState.TongueTouch:
                if (S.holdInput == 0 && input == 1)
                {
                    TonguePull();
                }
                //else if (S.holdInput == 0 && input == 1)
                break;
        }
        S.holdInput = input;

        if (S.holdInput == 1)
        {
            S.holdInputTime += Time.deltaTime;
        }
        else
        {
            S.holdInputTime = 0;
        }
    }

    private void TurnUpdate()
    {
        S.lastFacingDir = R.movement.S.facing;
    }

    private void ThrowTongue(Vector2 dir)
    {
        S.stateMachine = PlayerState.TongueTouch;
    }

    IEnumerator ThrowTongueExt()
    {
        R.movement.S.state = Movement.State.Inactive;
        R.movement.S.acceleration = 0;
        R.movement.S.decceleration = 0;
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.gravityScale = 0;
        //Start
        yield return new WaitForSeconds((float)(M.tongueShootTime / 4f));
        R.anim.SetBool("lookNEUTRAL", true);

        //END
        rigidbody2D.gravityScale = 1;
        R.movement.S.acceleration = R.movement.M.acceleration;
        R.movement.S.decceleration = R.movement.M.decceleration;
        R.movement.S.state = Movement.State.Base;
        R.anim.SetBool("lookNEUTRAL", false);
    }


    private void TonguePull()
    {

    }

    public void UpdateTonguePoint(bool localPos, Vector2 pos)
    {
        if (localPos)
        {
            R.goalTongueTip.position = transform.position + (Vector3)pos;
        }
        else
        {
            R.goalTongueTip.position = pos;
        }
    }

    public void TongueUpdate()
    {
        R.tongueTip.position = Vector2.Lerp(R.tongueTip.position, R.goalTongueTip.position, M.tongueLerpSpeed);
    }

    public void FaceUpdate()
    {

    }
}
