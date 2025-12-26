using UnityEngine;
using UnityEngine.InputSystem;
using SaintsField;
using System.Collections;
using Unity.VisualScripting;
using SaintsField.Playa;
using UnityEngine.UIElements;
using Unity.Mathematics;
using UnityEditor;

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
        [SerializeField] public float tongueLength;
        [SerializeField] public float tonguePullForce;
        [SerializeField] public float tongueShootTime;
        [SerializeField] public float tongueLerpSpeed;
        [SerializeField] public bool AIR_PULL;
        [SerializeField] public float tongueAirPullForce;
        [LayoutStart("Tongue Swing", ELayout.FoldoutBox)]
        [SerializeField] public float swingSpeed;
        [SerializeField] public float swingAcceleration;
        [SerializeField] public float swingDecceleration;
        [SerializeField] public float swingStartForce;
        [LayoutStart("Timers", ELayout.FoldoutBox)]
        [SerializeField] public float holdTimeThreshold;
        [SerializeField] public float tongueTouchTimeThreshold;
    }

    [System.Serializable]
    public struct States
    {
        [SerializeField] public PlayerState stateMachine;

        [LayoutStart("Timers", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public float holdInputTime;
        [SerializeField][ReadOnly] public float tongueTouchTime;
        [LayoutEnd]

        [LayoutStart("Box Holding", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public float holdInput;
        [SerializeField][ReadOnly] public Vector2 lastFacingDir;
        [SerializeField][ReadOnly] public float lastFacingDirLR;
        [SerializeField][ReadOnly] public bool canTurn;
        [SerializeField][ReadOnly] public Transform heldBox;
        [SerializeField][ReadOnly] public Transform hand;
        [SerializeField][ReadOnly] public Transform grabbableBox;

        [LayoutEnd]
        [LayoutStart("Tongue States", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public bool tongueOut;
        [SerializeField][ReadOnly] public bool tongueHit;
        [SerializeField][ReadOnly] public float tongueLerpSpeed;
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
        [SerializeField] public Transform tongueTip;
        [SerializeField] public Transform goalTongueTip;
        [LayoutStart("Components", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public MovementPlatformer2D movement;
        [SerializeField][ReadOnly] public Animator anim;
        [SerializeField][ReadOnly] public DistanceJoint2D distanceJoint;
        [LayoutStart("Visuals", ELayout.FoldoutBox)]
        [SerializeField] public LineRenderer tongueVisual1;
        [SerializeField] public LineRenderer tongueVisual2;
        [SerializeField] public LineRenderer tongueVisual3;
        [LayoutStart("Layers", ELayout.FoldoutBox)]
        [SerializeField] public LayerMask hitableTongueLayer;
        [SerializeField] public LayerMask layerBox;
        [SerializeField] public LayerMask layerGround;
    }

    [SerializeField][SaintsRow][RichLabel("Modifiers")] public Modifiers M;
    [SerializeField][SaintsRow][RichLabel("References")] public References R;
    [SerializeField][SaintsRow][RichLabel("States")] public States S;

    void Start()
    {
        S.tongueLerpSpeed = M.tongueLerpSpeed;
        ComponentGrab();
    }

    void ComponentGrab()
    {
        R.movement = GetComponent<MovementPlatformer2D>();
        R.anim = GetComponent<Animator>();
        R.distanceJoint = GetComponent<DistanceJoint2D>();
    }
    void Update()
    {
        TongueUpdate();
        FaceUpdate();
        TimerUpdate();
    }

    void TimerUpdate()
    {
        S.tongueTouchTime += Time.deltaTime;
        if (S.stateMachine == PlayerState.TongueTouch && S.tongueTouchTime > M.tongueTouchTimeThreshold)
        {
            PhysicsUnfreeze();
            S.stateMachine = PlayerState.Base;
            TongueRetract();
        }

        if (S.stateMachine == PlayerState.TongueTouch && S.holdInputTime > M.holdTimeThreshold && R.interactedObject != null) // holding down button > swing
        {
            PhysicsUnfreeze();
            TongueSwing();
            S.stateMachine = PlayerState.TongueHeld;
        }

        if (S.holdInput == 1)
        {
            S.holdInputTime += Time.deltaTime;
        }
        else
        {
            S.holdInputTime = 0;
        }
    }

    public void GrabInput(float input)
    {
        switch (S.stateMachine)
        {
            case PlayerState.Base:
                if (S.holdInput == 0 && input == 1)
                {
                    ThrowTongue(S.lastFacingDir);
                    S.tongueTouchTime = 0;
                    PhysicsFreeze();
                    S.stateMachine = PlayerState.TongueTouch;
                }
                break;
            case PlayerState.TongueTouch:
                if (S.holdInput == 0 && input == 1) // tap again > pull
                {
                    if (R.interactedObject == null)
                    {
                        if (M.AIR_PULL)
                        {
                            PhysicsUnfreeze();
                            AccelerationZero();
                            TonguePull(false, M.tongueAirPullForce);
                            S.stateMachine = PlayerState.Base;
                            break;
                        }
                    }
                    else
                    {
                        PhysicsUnfreeze();
                        AccelerationZero();
                        TonguePull(false, M.tonguePullForce);
                        S.stateMachine = PlayerState.Base;
                        break;
                    }
                }
                break;
            case PlayerState.TongueHeld: // tongue swing
                if (input == 0)
                {
                    TongueSwingEnd();
                    TongueRetract();
                    PhysicsUnfreeze();
                    S.stateMachine = PlayerState.Base;
                }
                break;
        }

        //hold checker
        S.holdInput = input;
    }

    private void AccelerationZero()
    {
        R.movement.S.acceleration = 2;
        R.movement.S.decceleration = 2;
        R.movement.S.movementSpeed = 7;
    }
    private void PhysicsFreeze()
    {
        R.movement.S.state = Movement.State.Inactive;
        R.movement.S.acceleration = 0;
        R.movement.S.decceleration = 0;
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.gravityScale = 0;
    }

    private void PhysicsUnfreeze()
    {
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = 1;
        if (rigidbody2D.linearVelocity.magnitude < 2)
        {
            R.movement.S.acceleration = R.movement.M.acceleration;
            R.movement.S.decceleration = R.movement.M.decceleration;
        }
        else
        {
            R.movement.S.acceleration = 0;
            R.movement.S.decceleration = 0;
        }
        R.movement.S.state = Movement.State.Base;
    }
    private void TongueSwing()
    {
        // change acceleration
        R.movement.S.acceleration = M.swingAcceleration;
        R.movement.S.decceleration = M.swingDecceleration;
        R.movement.S.movementSpeed = M.swingSpeed;

        // change anchor
        R.distanceJoint.connectedAnchor = R.tongueTip.position;

        // unparent goaltip
        R.goalTongueTip.parent = null;
        R.goalTongueTip.position = R.distanceJoint.connectedAnchor;
        S.tongueLerpSpeed = 1;

        // enable distance joint
        R.distanceJoint.enabled = true;
        R.distanceJoint.distance = Vector2.Distance(transform.position, R.tongueTip.position);

        // apply force
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.AddForce(M.swingStartForce * Vector2.down, ForceMode2D.Impulse);
    }

    private void TongueSwingEnd()
    {
        S.tongueLerpSpeed = M.tongueLerpSpeed;
        R.goalTongueTip.parent = this.transform;
        R.distanceJoint.enabled = false;
    }

    private void ThrowTongue(Vector2 dir)
    {
        S.canTurn = false;
        S.tongueOut = true;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, M.tongueLength, R.hitableTongueLayer);
        if (hit)
        {
            UpdateTonguePoint(false, hit.point + dir);
        }
        else
        {
            UpdateTonguePoint(true, dir * M.tongueLength);
        }
    }


    private void TonguePull(bool locked, float power)
    {
        if (locked)
        {

        }
        else
        {
            R.movement.Launch(S.lastFacingDir, power, true);
            TongueRetract();
        }
    }

    private void TongueRetract()
    {
        R.interactedObject = null;
        UpdateTonguePoint(true, Vector2.zero);
        S.tongueOut = false;
        S.canTurn = true;
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
        //constant lerp
        R.tongueTip.position = Vector2.Lerp(R.tongueTip.position, R.goalTongueTip.position, S.tongueLerpSpeed);

        //when throwing tongue, check to see if it hits something
        if (S.tongueOut && R.interactedObject == null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, R.tongueTip.localPosition, Vector2.Distance(transform.position, R.tongueTip.position), R.hitableTongueLayer);
            if (hit)
            {
                R.interactedObject = hit.collider.gameObject;
                if (R.interactedObject.GetComponent<Pickupable>() != null)
                {
                    UpdateTonguePoint(false, hit.collider.transform.position);
                }
                else
                {
                    UpdateTonguePoint(false, hit.point);
                }
            }
        }

        //update visuals
        R.tongueVisual1.SetPosition(1, R.tongueTip.parent == null ? R.tongueTip.position : R.tongueTip.localPosition);
        R.tongueVisual2.SetPosition(1, R.tongueTip.parent == null ? R.tongueTip.position : R.tongueTip.localPosition);
        R.tongueVisual3.SetPosition(1, R.tongueTip.parent == null ? R.tongueTip.position : R.tongueTip.localPosition);
    }

    public void FaceUpdate()
    {
        if (S.canTurn && R.movement.S.movementInput != Vector2.zero)
        {
            S.lastFacingDir = R.movement.S.movementInput;
            //S.lastFacingDirLR = R
        }
    }
}
