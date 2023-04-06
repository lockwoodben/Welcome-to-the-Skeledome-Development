using System.Collections;
using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(CharacterController))]
public class PlayerPhysics : MonoBehaviour
{

    // holds all movement attributes for player
    public PlayerAttributes Attrib;


    /*
    * This is where I'd put my player animations... IF I HAD ANY!!
    */
    #region COMPONENTS

    public Rigidbody2D rigid { get; private set; } // rigid 2D body that represents the player

    #endregion

    /*
    * These control what actions the player(s) have available to them
    */
    #region STATE PARAMETERS

    public float lastOnGroundTimer { get; private set; } // when player was last on the ground
    public float lastOnWallTimer { get; private set; } // when player had last touched a wall
    public float lastOnWallRightTimer { get; private set; } // when player had last touched right wall
    public float lastOnWallLeftTimer {get; private set;} // when player had last touched left wall
    public bool isJumping {get; private set;} // is player jumping?
    public bool isFacingRight {get; private set;}  // is player facing right?
    public bool isWallJumping {get; private set;} // is player wall jumping?
    public bool isDashing {get; private set;} // is player dashing?
    public bool isSlide {get; private set;} // is player sliding?

    // jump
    private bool isPartialJump; // is player performing a partial jump (letting go of jump before achieving max height)
    private bool isFalling; // is player falling?
    private int jumpCount = 0; // how many times has player jumped before touching ground?
    private int wallJumpCount = 0; // how many times has player wall jumped before touching ground?
    private bool onGround; // is player on the ground?
    private bool inLava; // is player in the lava?
    
    // wall jump
    private float wallJumpTimer;  // when player last wall jumped
    private int lastWallJumpD; // direction of last wall jump

    // Dash
    private bool dashOnCD; // is player's dash on cooldown?
    private Vector2 lastDashDir; // what was the direction of player's last dash?
    private bool isDashAttacking; // is player commiting a dash attack?
    private int dashLeft; // how many dashes does the player have left?
    #endregion

    /*
    * creates variables used to control input at a later stage
    */ 
    #region INPUT PARAMETERS

    private Vector2 moveInput;

    // keeps track of jump and dash times
    public float lastJumpTime {get; private set;}
    public float lastDashTime {get; private set;}

    #endregion

    /*
    *   This creates check parameters to compare hitboxes with
    */ 
    #region CHECK PARAMETERS

    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f,0.3f);
    [Space(5)]
    [SerializeField] private Transform frontWallCheckPoint;
	[SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f,1f);
    [Space(5)]
    //[SerializeField] private Transform lavaCheckPoint;
    //[SerializeField] private Vector2 lavaCheckSize = new (0.5f,1f);

    #endregion

    /*
    * These initialize the program. First Awake() then Start()
    */
    #region LAYERS AND TAGS

    [Header("Layers and Tags")]
    [SerializeField] private LayerMask groundLayer;

    #endregion

    // activates rigid object
    private void Start() {
        SetGScale(Attrib.gravityScale);
        isFacingRight = true;
    }

    private void Awake() {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update() {
            
            /*
            *   These keep track of the timers necessary to create fluid motion
            */
            #region TIMERS

            lastOnGroundTimer -= Time.deltaTime;
            lastJumpTime -= Time.deltaTime;
            lastDashTime -= Time.deltaTime;
            lastOnWallLeftTimer -= Time.deltaTime;
            lastOnWallTimer -= Time.deltaTime; 

            #endregion


            /*
            * This controls the control schemes for various movements
            */
            #region INPUT HANDLER 

            moveInput.x = Input.GetAxisRaw("Horizontal"); // gets direction of movement
            moveInput.y = Input.GetAxisRaw("Vertical"); // gets the jump

            // move input
            if (moveInput.x != 0)
                CheckDirectionToFace(moveInput.x > 0);

            // jump input
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
                OnJumpInput();
            
            // partial jump input
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
                OnJumpUpInput();

            // dash input
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxis("Fire1") != 0)
                OnDashInput();
            
            
            #endregion


            /*
            * This checks if the player is in contact with objects
            */
            #region COLLISION CHECKS

            // ground check
            if (!isDashing && !isJumping) {
                if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping) { // check if hitbox is colliding with ground
                    jumpCount = 0;
                    wallJumpCount = 0;
                    lastOnGroundTimer = Attrib.coyoteTime;
                    onGround = false;
                }

                // right wall check
                if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)
                    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)) && !isWallJumping) {
                        lastOnWallRightTimer = Attrib.coyoteTime;
                        onGround = true;
                    }
                // left wall check
                if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)
                    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)) && !isWallJumping) {
                        lastOnWallLeftTimer = Attrib.coyoteTime;
                        onGround = true;
                    }

                lastOnWallTimer = Mathf.Max(lastOnWallLeftTimer, lastOnWallRightTimer);
            }
            //if(Physics2D.OverlapBox(lavaCheckPoint.position, lavaCheckSize, 0, groundLayer)) {
              //  inLava = true;
            //}

            #endregion


            /*
            * This checks if a jump is legitimate, if so, which one and what other actions surround it
            */
            #region JUMP CHECKS

            // Checks if the player is jumping 
            if (isJumping && rigid.velocity.y < 0) {
                isJumping = false;
                
                if (!isWallJumping)
                    isFalling = true;
            }

            // wall jump check
            if (isWallJumping && Time.time - wallJumpTimer > Attrib.wallJumpTime)
                isWallJumping = false;

            // Checks 
            if (lastOnGroundTimer > 0 && !isJumping && !isWallJumping) {
                isPartialJump = false;

                if (!isJumping)
                    isFalling = false;
            }

            if (!isDashing) {
                // call jump
                if (CanJump() && lastJumpTime > 0) {
                    isJumping = true;
                    isPartialJump = false;
                    isFalling = false;
                    Jump();
                } else if (CanWallJump() && lastJumpTime > 0) {
                    // call walljump
                    isWallJumping = true;
                    isJumping = false;
                    isPartialJump = false;
                    isFalling = false;
                    
                    
                    wallJumpTimer = Time.time;
                    if (lastOnWallRightTimer > 0) 
                        lastWallJumpD = -1;
                    else
                        lastWallJumpD = 1;

                    WallJump(lastWallJumpD);
                }   

            }

            #endregion


            /*
            * This checks if a slide is legitimate
            */
            #region SLIDE CHECKS
            
            // set it slide to true or false based on if the player
            // is moving into the left or right wall
            isSlide = (CanSlide() && (lastOnWallLeftTimer > 0 && moveInput.x < 0) 
                || lastOnWallRightTimer > 0 && moveInput.x > 0);
            #endregion


            /*
            * This checks if a dash is legitimate, if so, dash
            */
            #region DASH CHECKS
            if (CanDash() && lastDashTime > 0) {
                Sleep(Attrib.dashSleepTime);  //freeze game for moment before dash

                if (moveInput != Vector2.zero) // if player is moving
                    lastDashDir = moveInput; // lastDirection = movement direction
                else {
                    if (isFacingRight) // if player is facing right, set last direction right and vice versa
                        lastDashDir = Vector2.right;
                    else
                        lastDashDir = Vector2.left;
                }
                    
                isDashing = true;
                isJumping = false;
                isWallJumping = false;
                isPartialJump = false;

                StartCoroutine(nameof(StartDash), lastDashDir);
                
            }

            #endregion


           /*
           * This controls the force of gravity on the player(s)
           */ 
            #region GRAVITY

            if(!isDashAttacking) {
                if (isPartialJump) {
                    // higher gravity if jump is released
                    // set new gravity scale = gravityScale * partial Jump gravity multiplier
                    SetGScale(Attrib.gravityScale * Attrib.partialJumpGravityMult);
                    // update velocity, keep y velocity lte max fall speed
                    rigid.velocity = new Vector2(rigid.velocity.x, Mathf.Max(rigid.velocity.y, -Attrib.maxFallSpeed));  
                }
                else if (isSlide) 
                    SetGScale(0); // set the gravity scale to 0

                 else if ((isJumping || isWallJumping || isFalling) && Mathf.Abs(rigid.velocity.y) < Attrib.jumpHangTime)
                    SetGScale(Attrib.gravityScale * Attrib.jumpHangGravityMult);

                 else if (rigid.velocity.y < 0) {
                    // higher gravity when falling
                    SetGScale(Attrib.gravityScale * Attrib.fallGravityMult);
                    // caps fall speed
                    rigid.velocity = new Vector2(rigid.velocity.x, 
                        Mathf.Max(rigid.velocity.y, -Attrib.maxFallSpeed));
                }
                 else
                    SetGScale(Attrib.gravityScale); // default gravity
            } else
                SetGScale(0);

            #endregion
        }

    private void FixedUpdate() {
        if (!isDashing)
            Run(1);
        else if (isDashAttacking)
            Run(Attrib.dashEndRunLerp);

        if (isSlide)
            Slide();
    }


    /*
    * These functions determine all basic movement paterns
    */
    #region RUN METHODS
    private void Run(float lerpNum)
    {
        
        float targetSpeed = moveInput.x * Attrib.runMaxSpeed; // calculate direction and velocity

        /*
        * This calculates the rate of acceleration
        */
        #region Calculate AccelRate
        float accelRate;

        // calculates acceleration based on if we're accelerating or turning or deccelerating
        // apply airborne multiplier if applicable
        if (lastOnGroundTimer > 0) {
            if (Mathf.Abs(targetSpeed) > 0.01f)
                accelRate = Attrib.runAccelerationVal;
            else
                accelRate = Attrib.runDeccelerationVal;
        }
        else {
            if (Mathf.Abs(targetSpeed) > 0.01f)
                accelRate = Attrib.runAccelerationVal * Attrib.accelerationInAir;
            else
                accelRate = Attrib.runDeccelerationVal * Attrib.deccelerationInAir;
        }

        #endregion


        /*
        *  This allows for a more fluid feeling movement rather than linear movement
        */
        #region Conserve Momentum
        if (Attrib.doConserveMomentum && Mathf.Abs(rigid.velocity.x) > Mathf.Abs(targetSpeed) && 
            Mathf.Sign(rigid.velocity.x) == Mathf.Sign(targetSpeed) && 
            Mathf.Abs(targetSpeed) > 0.01f && lastOnGroundTimer < 0) 
        {
            // stop deccelRate from changing in order to preserve momentum
            accelRate = 0;
        }
        #endregion

        // calculate the diff between max and current horrizontal speed
        float speed = targetSpeed - rigid.velocity.x;

        // calculate the force to add to rigid
        float force = speed * accelRate;

        // apply the horrizontal force
        rigid.AddForce(force * Vector2.right, ForceMode2D.Force);
    }

    #endregion


    /*
    *   General methods that didn't find a place amongst a specific movement
    */
    #region GEN METHODS
    
    // set gravity with new scale
    public void SetGScale(float scale) {
        rigid.gravityScale = scale;
    }

    // call perform sleep for given time
    private void Sleep(float time) {
        StartCoroutine(nameof(PerformSleep), time);
    }

    // pause player for brief period before dash
    private IEnumerator PerformSleep(float time) {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time); 
        Time.timeScale = 1;
    }

    #endregion


    /*
    * These are where the checks occur that determine the legitimacy of actions
    */
    #region  CHECK FUNCTIONS

    // This checks the direction of the player
    public void CheckDirectionToFace(bool isMovingRight) {

        if (isMovingRight != isFacingRight)
            turn();
    }

    // check if player can jump
    private bool CanJump() {
        return (lastOnGroundTimer > 0 && !isJumping) || jumpCount < 2;
    }

    // check if player can do partial jump
    private bool CanPartialJump() {
        return isJumping && rigid.velocity.y > 0;
    }

    // check if player can wall partial jump
    private bool CanWallPJump() {
        return isWallJumping && rigid.velocity.y > 0;
    }

    // check if player can dash
    private bool CanDash() {
        if (!isDashing && dashLeft < Attrib.dashUses &&lastOnGroundTimer > 0 && !dashOnCD)
            StartCoroutine(nameof(RefillDash), 1);

        return dashLeft > 0;
    }
    
    // check if player can wall jump
     private bool CanWallJump() {
            return onGround && lastOnGroundTimer <= 0 && lastOnWallTimer > 0 &&
            lastJumpTime > 0 && wallJumpCount < 1 && (!isWallJumping ||
            (lastOnWallRightTimer > 0 && lastWallJumpD == 1) || 
            (lastOnWallLeftTimer > 0 && lastWallJumpD == -1));
    }
    
    // check if player can slide
    public bool CanSlide() {
        return (lastOnWallTimer > 0 && !isJumping && !isWallJumping && !isDashing && lastOnGroundTimer <= 0);
    }

    #endregion

    // This will flip the player around
    public void turn() {
        // the scale to flip
        Vector3 scale = transform.localScale;
 
        scale.x *= -1;
        transform.localScale = scale;
        isFacingRight = !isFacingRight;
    }


    /*
    * All of the functions that controls jump
    */
    #region JUMP FUNCTIONS

    private void Jump() {

        // make sure jump can only be called once 
        // by resetting its timers
        lastJumpTime = 0;
        lastOnGroundTimer = 0;
        

        /*
        * This is where jump is calculated
        */
        #region Jump

        // calculate the the jump force
        float force = Attrib.jumpForce;
        
        // if you are falling reset the y velocity
        if (rigid.velocity.y < 0) {
            Vector3 vel = rigid.velocity;
            vel.y = 0f;
            rigid.velocity = vel;
        }

        if(jumpCount == 1)
            rigid.AddForce(Vector2.up * Mathf.Min(force, Attrib.maxJumpSpeed), ForceMode2D.Impulse);
        else
            rigid.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        jumpCount++;

        // stops from wall jumping in air
        if (jumpCount == 2)
            onGround = false;
    }

    #endregion

    private void WallJump(int direction) {
        // make sure you cant wall jump multiple times'
        lastJumpTime = 0;
        lastOnGroundTimer = 1;
        lastOnWallLeftTimer = 0;
        lastOnWallRightTimer = 0;


        /*
        * This is where the wall jump occures
        */
        #region WALL JUMP

        Vector2 force = new Vector2(Attrib.wallJumpForce.x, Attrib.wallJumpForce.y);
        
        if (moveInput.x > 0)
            force.x *= direction; // apply the force in the oppsite direction
        else if (moveInput.x < 0)
            turn();

        if (Mathf.Sign(rigid.velocity.x) != Mathf.Sign(force.x))
			force.x -= rigid.velocity.x;

        if (rigid.velocity.y < 0)
            force.y -= rigid.velocity.y;

        rigid.AddForce(force, ForceMode2D.Impulse);
        wallJumpCount++;

        #endregion
    }

    #endregion


    /*
    * This is where slide is calculated
    */
    #region SLIDE METHOD

    private void Slide() {
        // like run but on the y axis 
        float speed = Attrib.slideSpeed - rigid.velocity.y;
        float force = speed * Attrib.slideAcceleration;

        // clamp the force 
        force = Mathf.Clamp(force, -Mathf.Abs(speed) * 
            (1 / Time.fixedDeltaTime), Mathf.Abs(speed) * 
            (1 / Time.fixedDeltaTime));

        rigid.AddForce(force * Vector2.up);
    }

    #endregion


    /*
    * These are all the methods that calculate dash
    */ 
    #region DASH METHODS

    // dash coroutine
    private IEnumerator StartDash(Vector2 direction) {
       lastOnGroundTimer = 0; // set necessary values to 0
       lastDashTime = 0;
       

       float startTime = Time.time; // set start time

       dashLeft--; // remove dash
       isDashAttacking = true; // activate dash attack
       SetGScale(0); // remove gravity

       while (Time.time - startTime <= Attrib.dashAttackTime) { // during attack phase
            rigid.velocity = direction.normalized * Attrib.dashSpeed;  // set velocity
            // pauses loop until next frame
            yield return null;
       }

       startTime = Time.time;  // set new start time
       isDashAttacking = false; // end attack phase

       // begin end of dash
       SetGScale(Attrib.gravityScale); // return to normal gravity
       rigid.velocity = Attrib.dashEndSpeed * direction.normalized; // set new velocity

       while (Time.time - startTime <= Attrib.dashEndTime) // during end phase
            yield return null; // do nothing

       // dash over
       isDashing = false;
    }

    private IEnumerator RefillDash(int dashes) {
        dashOnCD = true; // set cooldown
        yield return new WaitForSeconds(Attrib.dashCD); // wait for cooldown
        dashOnCD = false; // finish cooldown
        dashLeft++; // award dash
    }

    #endregion


    /*
    * These check major inputs
    */
    #region INPUT CHECKS

    public void OnJumpInput() { lastJumpTime = Attrib.jumpBufferTime; }


    // check if partial jump and sey isPartialJump if so
    public void OnJumpUpInput() { if (CanPartialJump()) isPartialJump = true; }


    public void OnDashInput() { lastDashTime = Attrib.dashInputBufferTime; }
    
    #endregion
}
    
