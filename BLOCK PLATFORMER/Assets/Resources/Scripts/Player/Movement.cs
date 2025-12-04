using UnityEngine;
using SaintsField;
using SaintsField.Playa;

public abstract class Movement : MonoBehaviour
{
    public enum State
    {
        Inactive, // NO CONTROL OVER PLAYER (use for cutscenes and other stuff)
        Base, // normal state (full control)
        Knockback, // no control (lowers acceleration and decceleration so that only the knockback controls you)
        Ledging, // diff control pattern (when the character is hanging on a ledge) AO LEDGE
        WallJumping, // similar to normal (however with lower acceleration and decceleration in order to stop repeat walljumps) AO WALL
        WallSliding,
        Dashing, // AO DASH
        Crouching
    }

    [System.Serializable]
    public struct Modifiers
    {
        [LayoutStart("Basic", ELayout.FoldoutBox)]
        [SerializeField] public float movementSpeed;
        [SerializeField] public float acceleration;
        [SerializeField] public float decceleration;
        [LayoutStart("./Air", ELayout.FoldoutBox)]
        [SerializeField][RichLabel("Diff Val w/ Air?")] public bool DIFF_AIR_VALS;
        [ShowIf(nameof(DIFF_AIR_VALS))][SerializeField] public float movementSpeedAir;
        [ShowIf(nameof(DIFF_AIR_VALS))][SerializeField] public float accelerationAir;
        [ShowIf(nameof(DIFF_AIR_VALS))][SerializeField] public float deccelerationAir;
        [LayoutEnd]
        [LayoutStart("Jump", ELayout.FoldoutBox)]
        [SerializeField][RichLabel("Can Jump?")] public bool JUMP_ABLE;
        [ShowIf(nameof(JUMP_ABLE))][SerializeField] public float jumpForce;
        [ShowIf(nameof(JUMP_ABLE))][SerializeField] public Vector2 baseGroundNormal;
        [ShowIf(nameof(JUMP_ABLE))][SerializeField] public Vector2 groundCheckLocation;
        [ShowIf(nameof(JUMP_ABLE))][SerializeField] public Vector2 groundCheckSize;
        [ShowIf(nameof(JUMP_ABLE))][SerializeField][RichLabel("Multiple Jumps?")] public bool MULT_JUMP;
        [ShowIf(nameof(JUMP_ABLE), nameof(MULT_JUMP))][SerializeField] public int maxAmountOfJumpsInAir;
        [ShowIf(nameof(JUMP_ABLE))][SerializeField][RichLabel("Variable Jump Height?")] public bool QUICK_FALL_OPTION; // the player CHOOSES quickfall when releasing jump button
        [ShowIf(nameof(JUMP_ABLE), nameof(QUICK_FALL_OPTION))][SerializeField] public float quickFallForce;
        [ShowIf(nameof(JUMP_ABLE), nameof(QUICK_FALL_OPTION))][SerializeField] public bool QUICK_FALLEN_FORCE; // the player FORCES quickfall at peak velocity
        [ShowIf(nameof(JUMP_ABLE), nameof(QUICK_FALL_OPTION))][SerializeField] public float quickFallForceForced;
        [ShowIf(nameof(JUMP_ABLE), nameof(QUICK_FALL_OPTION), nameof(QUICK_FALLEN_FORCE))][SerializeField] public float m_peakJumpVelocity; // used for FORCE

        [LayoutStart("./QOT", ELayout.FoldoutBox)]
        [ShowIf(nameof(JUMP_ABLE))][SerializeField][RichLabel("Jump QOT")] public bool JUMP_QOT;
        [ShowIf(nameof(JUMP_ABLE), nameof(JUMP_QOT))][SerializeField][RichLabel("Coyote Time")] public bool COYOTE_TIME;
        [ShowIf(nameof(JUMP_ABLE), nameof(JUMP_QOT), nameof(COYOTE_TIME))][Range(0.0f, 5.0f)][SerializeField] public float coyoteTimeSeconds;
        [ShowIf(nameof(JUMP_ABLE), nameof(JUMP_QOT))][SerializeField][RichLabel("Jump Queuing")] public bool JUMP_CUE;
        [ShowIf(nameof(JUMP_ABLE), nameof(JUMP_QOT), nameof(JUMP_CUE))][Range(0.0f, 5.0f)][SerializeField] public float jumpQueueMax;
        [ShowIf(nameof(JUMP_ABLE), nameof(JUMP_QOT), nameof(JUMP_CUE))][Range(0.0f, 5.0f)][SerializeField] public float jumpQueueClearTimeSeconds;
        [LayoutStart("Gravity Modifier", ELayout.FoldoutBox)]
        [SerializeField][RichLabel("Gravity Mods?")] public bool GRAV_MOD;
        [ShowIf(nameof(GRAV_MOD))][SerializeField] public float fallClamp; // clamp the fall speed of the player to make them fall slower
        [ShowIf(nameof(GRAV_MOD))][SerializeField] public float gravityFallingAccel;
    }
    [SerializeField][SaintsRow][RichLabel("Modifiers")] public Modifiers M;

    [System.Serializable]
    public struct States
    {
        [LayoutStart("Current Mods", ELayout.FoldoutBox)]
        [SerializeField][ReadOnly] public float movementSpeed;
        [SerializeField][ReadOnly] public float acceleration;
        [SerializeField][ReadOnly] public float decceleration;
        [LayoutEnd]
        [LayoutStart("Inputs", ELayout.FoldoutBox)]
        [ReadOnly] public Vector2 movementInput;
        [LayoutEnd]
        [LayoutStart("States", ELayout.FoldoutBox)]
        [SerializeField] public State state;
        [SerializeField][ReadOnly] public Vector2 facing;
        [SerializeField][ReadOnly] public bool canTurn;
        [SerializeField][ReadOnly] public bool isGrounded;
        [ShowIf(nameof(M.JUMP_ABLE), nameof(M.MULT_JUMP))][ReadOnly] public int amountOfJumps;
        [ShowIf(nameof(M.JUMP_ABLE), nameof(M.QUICK_FALL_OPTION))][ReadOnly] public bool canQuickFall;
        //QOL
        [ShowIf(nameof(M.JUMP_ABLE), nameof(M.JUMP_QOT), nameof(M.COYOTE_TIME))][ReadOnly] public int coyoteTimeActive; // |-1 = disabled|0 = able to change|1 = active|
        [ShowIf(nameof(M.JUMP_ABLE), nameof(M.JUMP_QOT), nameof(M.JUMP_CUE))][ReadOnly] public int jumpQueue;

    }

    [System.Serializable]
    public struct References
    {
        [LayoutStart("Components", ELayout.FoldoutBox)]
        [ReadOnly] public Rigidbody2D rb;
        [ReadOnly] public Animator anim;
        [ReadOnly] public ParticleManager particleManager;
        [LayoutEnd]
        [LayoutStart("LayerMasks", ELayout.FoldoutBox)]
        [SerializeField] public LayerMask layerGround;
    }

    [SerializeField][SaintsRow][RichLabel("References")] public References R;

    [SerializeField][SaintsRow][RichLabel("States")] public States S;

    public virtual void ModtoState()
    {
        S.acceleration = M.acceleration;
        S.decceleration = M.decceleration;
        S.movementSpeed = M.movementSpeed;
    }

    protected virtual void ComponentGrab()
    {
        R.rb = GetComponent<Rigidbody2D>();
        R.anim = GetComponent<Animator>();
        R.particleManager = GetComponent<ParticleManager>();
    }

    // Input functions
    public abstract void MoveInput(Vector2 input);

    public abstract void JumpInput(float input);
}
