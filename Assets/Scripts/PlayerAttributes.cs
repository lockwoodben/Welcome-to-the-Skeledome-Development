
using UnityEngine;

[CreateAssetMenu(menuName = "Player Attributes")] // Used to create new playerAttributes object
public class PlayerAttributes : ScriptableObject
{
    /*
    * These handle all linear movements
    */
    [Header("Run")]
    public float runMaxSpeed; // speed in which acceleration ends
    public float runAccel; // how long until max speed is reach
    [HideInInspector] public float runAccelVal; // force applied to player (multiplied by speedDiff)
    public float runDeccel; // same as runAccel but opposite
    [HideInInspector] public float runDeccelVal; // same as runAccelVal by opposite
    [Space(10)]
    [Range(0.01f, 1)] public float accelInAir; // multiplier to air acceleration
    [Range(0.01f, 1)] public float deccelInAir; // same as accelInAir but opposite
    public bool doConserveMomentum; // forfeit yourself to the rules of Yimir



    /*
    * These handle all calculations for gravity
    */
    [Header("Gravity")]
    [HideInInspector] public float gStrength; // downward force of gravity
    [HideInInspector] public float gScale; // multiplier of player gravity
    [Space(5)]
    public float fallGravMult; // gravity multiplier when falling
    public float maxFallSpeed; // downward terminal velocity
    public float maxJumpSpeed; // upward terminal velocity
    [Space(20)]



    /*
    * These handle all calculations for jump
    */
    [Header("Jump")]
    public float jHeight; // height of jump
    public float jTimeToMax; // length of time to reach max height after jumping
    [HideInInspector] public float jForce; // force applied on jump
    public float partialJumpGravMult; // gravity multiplier when player releases jump
    [Range(0f, 1)] public float jHangGravMult; // reduces gravity when near max height
    public float jHangTime; // time in which player is in hang time
    [Space(5)]
    public float jHangAccel; // acceleration in hang time
    public float jHangMaxSpeed; // max speed in hang time
    [Range(0.01f, 0.5f)] public float jBuffTime; // time before hitting ground where your jump still counts
    [Range(0.01f, 0.5f)] public float coyoteTime; // time after falling off ledge in which you can still jump



    /*
    *   These handle additional calculations for wall jumps
    */ 
    [Header("Wall Jump")]
    [Range(0f, 1.5f)]public float wallJTime; // how long since last wall jump
    public Vector2 wallJForce; // force in which you launch from wall
    public bool turnOnWall; // cause player to turn on wall jump

    [Space(20)]



    /*
    * These handle all calculations for dash
    */
    [Header("Dash")]
    public int dashUses; // how many dashes a player has available
    public int dashSpeed;  // the velocity in which a dash propells the player
    public float dashSleepTime; // time for game freeze on dash
    [Space(5)]
    public float dashAttackTime; // how long for the attack phase of the dash
    [Space(5)]
    public float dashEndTime; // time after drag to idle
    public Vector2 dashEndSpeed; // slows down player after dash
    [Range(0f,1f)] public float dashEndRunLerp; // slows player input while dashing
    [Space(5)]
    public float dashCD; // time before next use of dash
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashInputBufferTime; // how many times you can spam dash before next is qued

    [Space(20)]



    /*
    *   These functions handle the slide movements
    */
    [Header("Slides")]
    public float slideSpeed; // the speed in which the player slides down a wall
    public float slideAccel; // the speed in which the player accelerates down a wall





    
     
    private void OnValidate()
    {
        // calculate movement speeds
        runAccelVal = (50 * runAccel) / runMaxSpeed;
        runDeccelVal = (50 * runDeccel) / runMaxSpeed;
        
        // calculate strength of gravity
        gStrength = -(2 * jHeight) / (jTimeToMax * jTimeToMax);

        // scale of gravity on rigid body
        gScale = gStrength / Physics2D.gravity.y;

        // calculate upward force of jump
        jForce = Mathf.Abs(gStrength) * jTimeToMax;
         

        // Clamp takes the parameters (value, min, max) as input and returns result between min and max
        #region Variable Ranges
        runAccel = Mathf.Clamp(runAccel, 0.01f, runMaxSpeed);
        runDeccel = Mathf.Clamp(runDeccel, 0.01f, runMaxSpeed);
        #endregion
    }

}
