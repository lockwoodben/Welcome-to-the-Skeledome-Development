
using UnityEngine;

[CreateAssetMenu(menuName = "Player Attributes")] // Used to create new playerAttributes object
public class PlayerAttributes : ScriptableObject
{
    /*
    * These handle all linear movements
    */
    [Header("Run")]
    public float runMaxSpeed; // speed in which accelerationeration ends
    public float runAcceleration; // how long until max speed is reach
    [HideInInspector] public float runAccelerationVal; // force applied to player (multiplied by speedDiff)
    public float runDecceleration; // same as runAccelerationeration but opposite
    [HideInInspector] public float runDeccelerationVal; // same as runAccelerationerationVal by opposite
    [Space(10)]
    [Range(0.01f, 1)] public float accelerationInAir; // multiplier to air accelerationeration
    [Range(0.01f, 1)] public float deccelerationInAir; // same as accelerationInAir but opposite
    public bool doConserveMomentum; // forfeit yourself to the rules of Yimir



    /*
    * These handle all calculations for gravity
    */
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength; // downward force of gravity
    [HideInInspector] public float gravityScale; // multiplier of player gravity
    [Space(5)]
    public float fallGravityMult; // gravity multiplier when falling
    public float maxFallSpeed; // downward terminal velocity
    public float maxJumpSpeed; // upward terminal velocity
    [Space(20)]



    /*
    * These handle all calculations for jump
    */
    [Header("Jump")]
    public float jumpHeight; // height of jump
    public float jumpTimeToMax; // length of time to reach max height after jumping
    [HideInInspector] public float jumpForce; // force applied on jump
    public float partialJumpGravityMult; // gravity multiplier when player releases jump
    [Range(0f, 1)] public float jumpHangGravityMult; // reduces gravity when near max height
    public float jumpHangTime; // time in which player is in hang time
    [Space(5)]
    public float jumpHangAcceleration; // accelerationeration in hang time
    public float jumpHangMaxSpeed; // max speed in hang time
    [Range(0.01f, 0.5f)] public float jumpBufferTime; // time before hitting ground where your jump still counts
    [Range(0.01f, 0.5f)] public float coyoteTime; // time after falling off ledge in which you can still jump



    /*
    *   These handle additional calculations for wall jumps
    */ 
    [Header("Wall Jump")]
    [Range(0f, 1.5f)]public float wallJumpTime; // how long since last wall jump
    public Vector2 wallJumpForce; // force in which you launch from wall
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
    public float slideAcceleration; // the speed in which the player accelerationerates down a wall





    
     
    private void OnValidate()
    {
        // calculate movement speeds
        runAccelerationVal = (50 * runAcceleration) / runMaxSpeed;
        runDeccelerationVal = (50 * runDecceleration) / runMaxSpeed;
        
        // calculate strength of gravity
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToMax * jumpTimeToMax);

        // scale of gravity on rigid body
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // calculate upward force of jump
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToMax;
         

        // Clamp takes the parameters (Value, min, max) as input and returns result between min and max
        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }

}
