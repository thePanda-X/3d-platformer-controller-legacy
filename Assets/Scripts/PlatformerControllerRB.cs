using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlatformerControllerRB : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    float dashResetTimer = 0;
    public float dashResetTime = 1f;
    public int midairDashCount = 1;
    int WallDashCount = 0;
    bool isDashing = false;
    bool canDash = true;
    bool reloadDash = false;
    public float[] jumpForces = { 7.5f, 11f, 16f, 8.25f, 9f, 5f };
    public enum JumpType { jumpRegular, jumpDouble, jumpTriple, jumpEnemy, jumpWall, Dive };
    public int jumpCount = 0;
    public float jumpCountResetTime = 0.5f;
    float jCResetTimer = 0;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    Rigidbody rb;
    bool isGrounded = false;
    Transform cam;
    Animator anim;
    Transform fakeShadow;
    [SerializeField] LayerMask NotPlayer;
    Vector3 CamRelative; //the players velocity relative to the camera
    [HideInInspector] public Transform playerModel;
    bool isMoving = false;
    bool canMove = true;

    [SerializeField] GameObject RunParticle;
    [SerializeField] GameObject WallSideParticle;
    float tbsp = 0;
    float timeBtwnSpawn = 0.1f;

    Vector3 CamRelativeMove;

    //wall jump related variables
    bool canWallJump;
    public float wallSlideSpeed = 0.5f;
    public float wallCheckDistance = 0.2f;
    public float timeBeforeWallSlide = 1f;
    float wallSlideTimer = 0;

    bool hasBonked = false;

    List<Transform> nearbyEnemies = new List<Transform>();
    float enemySearchRadius = 5f;
    public LayerMask enemyLayer;

    RaycastHit GroundHit;
    CapsuleCollider cc;

    Transform closestEnemy = null;

    bool prevGrounded = false; // this is used to check if the player has just landed on the ground


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        anim = GetComponent<Animator>();
        fakeShadow = GameObject.FindGameObjectWithTag("FakeShadow").transform;
        playerModel = GameObject.FindGameObjectWithTag("PlayerModel").transform;
        cc = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null)
        {
            transform.localScale = Vector3.one; //reset scale if the player is not on a moving platform
        }
        //mechanics
        checkForGrounded();     //check if the player is grounded
        HandleMovingPlatforms();    //handle moving platforms
        handleMovement();       //player Horizontal Movement Grounded
        HandleJump();           //player Jump
        HandleEnemyBounce();    //player Enemy Bounce
        HandleWallJump();       //player Wall Jump
        handleDash();           //player Dash

        //visuals
        HandleAnimations();     //handle animations
        HandleRunParticles();   //spawn particles when running
        HandleFakeShadow();     //handle fake shadow
    }

    private void HandleEnemyBounce()
    {
        //look for enemies in a radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, enemySearchRadius, enemyLayer);

        //if there are enemies in the radius
        if (hitColliders.Length > 0)
        {
            //add them to the list
            foreach (var hitCollider in hitColliders)
            {
                if (!nearbyEnemies.Contains(hitCollider.transform) && !hitCollider.GetComponent<WalkingEnemy>().GetIsMarkedForDeath())
                {
                    nearbyEnemies.Add(hitCollider.transform);
                }
            }
        }

        if (nearbyEnemies.Count == 0)
        {
            closestEnemy = null;
        }

        //if there are enemies in the list
        if (nearbyEnemies.Count > 0)
        {
            //get the closest enemy
            if (closestEnemy == null)
            {
                closestEnemy = nearbyEnemies[0];
            }

            //check if they are in the bounce radius
            foreach (var enemy in nearbyEnemies)
            {
                if (Vector3.Distance(transform.position, enemy.position) < Vector3.Distance(transform.position, closestEnemy.position) && !enemy.GetComponent<WalkingEnemy>().GetIsMarkedForDeath())
                {
                    closestEnemy = enemy;
                }

                if (Vector3.Distance(transform.position, enemy.position) < 1f && rb.linearVelocity.y < 0)
                {
                    //if they are, bounce the player
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForces[(int)JumpType.jumpEnemy], rb.linearVelocity.z);
                    //remove the enemy from the list'
                    closestEnemy.GetComponent<WalkingEnemy>().TriggerDeath();
                    nearbyEnemies.Remove(closestEnemy);
                    nearbyEnemies.Remove(enemy);
                    break;
                }
            }

            // if the player dashesh and the closest enemy is in a certain radius and the player can dash then make the 
            //player look at the enemy and dash towards it

            //NOT FINISHED CHECK HOW TO MAKE THE PLAYER LOOK AT THE ENEMY AND THEN MOVE TO IT.
            if (Input.GetButtonDown("Dash") && Vector3.Distance(transform.position, closestEnemy.position) < 5f && canDash && !isGrounded)
            {
                Debug.Log($"Dashing to closest enemy: {closestEnemy.gameObject.name}");
                rb.linearVelocity = new Vector3(0, jumpForces[(int)JumpType.jumpEnemy], 0);
                transform.position = closestEnemy.position + (Vector3.up * 0.25f);
                var forward = closestEnemy.position - transform.position;
                forward.y = 0;
                transform.rotation = Quaternion.LookRotation(forward);
                transform.rotation.eulerAngles.Set(0, transform.rotation.eulerAngles.y, 0);
                StartCoroutine(DashCooldown());
                //remove the enemy from the list
                closestEnemy.GetComponent<WalkingEnemy>().TriggerDeath();
                nearbyEnemies.Remove(closestEnemy);

                canDash = true;
                reloadDash = false;
                dashResetTimer = 0;
            }

        }
    }

    private void handleDash()
    {

        if (Input.GetButtonDown("Dash") && canDash && isMoving && (Mathf.Abs(Input.GetAxisRaw("Horizontal"))) + (Mathf.Abs(Input.GetAxisRaw("Vertical"))) > 0.1f)
        {
            isDashing = true;
            var p = Instantiate(WallSideParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            p.transform.localScale = Vector3.one * 2f;
            Destroy(p, 1f);
            canDash = false;
            canMove = false;

            if (isGrounded && !Input.GetButton("Jump"))
            {
                rb.linearVelocity = new Vector3(CamRelativeMove.x * dashSpeed, jumpForces[(int)JumpType.Dive] + jumpForces[(int)JumpType.jumpRegular], CamRelativeMove.z * dashSpeed);
            }
            else if (Input.GetButton("Jump"))
            {
                JumpType jump = (JumpType)jumpCount;
                rb.linearVelocity = new Vector3(CamRelativeMove.x * dashSpeed, rb.linearVelocity.y, CamRelativeMove.z * dashSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector3(CamRelativeMove.x * dashSpeed, rb.linearVelocity.y + jumpForces[(int)JumpType.Dive], CamRelativeMove.z * dashSpeed);
            }

            playerModel.DOLookAt(transform.position + new Vector3(CamRelativeMove.x, 0, CamRelativeMove.z), 0.1f);
            cam.GetComponent<cameraTriggerHandler>().TriggerShake(0.1f, .5f, 5, 90);
            StartCoroutine(DashCooldown());
        }

        if (!isGrounded && !canDash && WallDashCount < midairDashCount && canWallJump)
        {
            WallDashCount++;
            canDash = true;
            reloadDash = false;
            dashResetTimer = 0;
        }

        if (isGrounded && !canDash)
        {
            reloadDash = true;
            WallDashCount = 0;
        }

        if (reloadDash)
        {
            dashResetTimer += Time.deltaTime;
            if (dashResetTimer >= dashResetTime)
            {
                canDash = true;
                reloadDash = false;
                dashResetTimer = 0;
            }
        }
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
    }



    void checkForGrounded()
    {

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out GroundHit, 1.1f, NotPlayer))
        {
            isGrounded = true;
            canMove = true;
            hasBonked = false;
        }
        else
        {
            isGrounded = false;
            canMove = false;
        }

        if (isGrounded && prevGrounded != isGrounded)
        {
            StartCoroutine(landSquash());
        }
        prevGrounded = isGrounded;
    }

    void HandleMovingPlatforms()
    {
        if (isGrounded)
        {
            if (GroundHit.transform.tag == "MovingPlatform")
            {
                transform.parent = GroundHit.transform;
            }
            else
            {
                transform.parent = null;
            }
        }
        else
        {
            transform.parent = null;
        }
    }

    void HandleWallJump()
    {
        RaycastHit hit;
        bool foundwall;
        if (Physics.Raycast(transform.position + Vector3.up * cc.height / 2f, playerModel.forward, out hit, cc.radius + wallCheckDistance, NotPlayer))
        {
            if (hit.normal.y == 0)
            {
                foundwall = true;

                if (hit.transform.tag == "MovingPlatform")
                {
                    transform.parent = hit.transform;
                }
            }
            else
            {
                foundwall = false;
                transform.parent = null;
                wallSlideTimer = 0;
            }
        }
        else
        {
            foundwall = false;
            wallSlideTimer = 0;
        }

        if (!isGrounded && foundwall && !isDashing)
        {
            canWallJump = true;
            rb.linearVelocity = new Vector3(0, 0, 0);
        }
        else
        {
            foundwall = false;
            canWallJump = false;
            wallSlideTimer = 0;
        }

        if (canWallJump)
        {
            canMove = false;
            rb.useGravity = false;

            Vector3 Mforward = cam.forward * Input.GetAxisRaw("Vertical");
            Vector3 Mright = cam.right * Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                isGrounded = false;
                anim.SetTrigger("Jump");
                StartCoroutine(jumpSquash());
                jumpCount = 0;
                transform.forward = hit.normal;
                playerModel.forward = hit.normal;
                rb.linearVelocity = new Vector3(transform.forward.x * moveSpeed, jumpForces[(int)JumpType.jumpWall], transform.forward.z * moveSpeed);
                canWallJump = false;
            }

            //wait a bit before wall sliding
            if (wallSlideTimer < timeBeforeWallSlide)
            {
                wallSlideTimer += Time.deltaTime;
            }
            else
            {
                if (canWallJump)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallSlideSpeed, rb.linearVelocity.z);
                    //spawn particles when sliding on wall
                    if (tbsp > timeBtwnSpawn)
                    {
                        tbsp = 0;
                        GameObject p = Instantiate(WallSideParticle, hit.point, Quaternion.identity);
                        p.transform.forward = -hit.normal;
                        Destroy(p, 1f);
                    }
                    else
                    {
                        tbsp += Time.deltaTime;
                    }

                    if (Vector3.Distance(CamRelativeMove.normalized, hit.normal) < 0.5f)
                    {
                        //release player from wall
                        canWallJump = false;
                        rb.useGravity = true;
                        //move player away from wall a bit so that he can jump again
                        rb.linearVelocity = new Vector3(hit.normal.x * moveSpeed / 5, rb.linearVelocity.y, hit.normal.z * moveSpeed / 5);
                        transform.forward = hit.normal;
                        playerModel.forward = hit.normal;
                    }
                }
            }

        }
        else
        {
            if (!isDashing)
            {
                rb.useGravity = true;
            }
        }
    }

    void HandleJump()
    {
        JumpType jumpType = JumpType.jumpRegular;
        //jump height base on key press time
        if (Input.GetButtonDown("Jump") && isGrounded)
        {

            if (jumpCount == 2)
            {
                Vector2 horizVel = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
                horizVel.Normalize();
                if (CamRelativeMove.magnitude < 0.5f && horizVel.magnitude < 0.25f)
                {
                    jumpCount = 0;
                }
            }

            jumpType = (JumpType)jumpCount;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForces[(int)jumpType], rb.linearVelocity.z);

            StartCoroutine(jumpSquash());
            anim.SetInteger("JumpType", (int)jumpType);
            anim.SetTrigger("Jump");

            canMove = false;

            jumpCount = jumpCount < 2 ? jumpCount + 1 : 0;
            jCResetTimer = 0;
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }


        if (isGrounded)
        {
            if (jCResetTimer > jumpCountResetTime)
            {
                jumpCount = 0;
                jCResetTimer = 0;
            }
            else
            {
                jCResetTimer += Time.deltaTime;
            }
        }
    }

    void HandleAnimations()
    {
        //handle animations
        anim.SetFloat("Movement", CamRelativeMove.magnitude);
        anim.SetBool("Grounded", isGrounded);
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetBool("CanWallJump", canWallJump);
        anim.SetBool("isDiving", isDashing);
        anim.SetBool("hasBonked", hasBonked);
    }

    void HandleRunParticles()
    {
        if (isGrounded && isMoving)
        {
            if (tbsp > timeBtwnSpawn)
            {
                var clone = Instantiate(RunParticle, transform.position, Quaternion.identity);
                Destroy(clone, 1f);
                tbsp = 0;
            }
            else
            {
                tbsp += Time.deltaTime;
            }
        }
    }

    void HandleFakeShadow()
    {
        RaycastHit shadowHit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out shadowHit, 100, NotPlayer))
        {
            if (!fakeShadow.gameObject.activeSelf)
            {
                fakeShadow.gameObject.SetActive(true);  //enable the fake shadow if it's not already
            }
            fakeShadow.position = shadowHit.point + shadowHit.normal / 1000; //move the fake shadow to the surface and offset it a bit so it doesn't clip
            fakeShadow.up = shadowHit.normal; //rotate the fake shadow to the normal of the surface
        }
        else
        {
            fakeShadow.gameObject.SetActive(false);     //disable the fake shadow if it's not on the ground
        }
    }

    void handleMovement()
    {
        //player Horizontal Movement in air
        Vector3 camForwardNoY = new Vector3(cam.forward.x, 0, cam.forward.z);
        Vector3 camRightNoY = new Vector3(cam.right.x, 0, cam.right.z);
        Vector3 Mforward = camForwardNoY * Input.GetAxisRaw("Vertical");
        Vector3 Mright = camRightNoY * Input.GetAxisRaw("Horizontal");
        CamRelative = (Mforward + Mright).normalized + transform.position;
        CamRelativeMove = CamRelative - transform.position;

        if (canMove && !isDashing)
        {
            rb.linearVelocity = new Vector3(CamRelativeMove.x * moveSpeed, rb.linearVelocity.y, CamRelativeMove.z * moveSpeed);
            if (CamRelativeMove.magnitude > 0f)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }

            if (isMoving)
            {
                playerModel.DORotateQuaternion(Quaternion.LookRotation(CamRelativeMove), 0.1f);

                //rotate players y axis to the direction of the camera
                transform.rotation = Quaternion.Euler(0, cam.eulerAngles.y, 0);
            }
        }
        else if (!isGrounded)
        {
            if (Vector3.Distance(playerModel.forward, CamRelativeMove.normalized) > 1f && isMoving)
            {
                rb.linearVelocity -= playerModel.forward * Time.deltaTime * Vector3.Distance(playerModel.forward, CamRelativeMove.normalized) * 7;
            }
        }
    }




    //procedural sqaush and stretch
    IEnumerator jumpSquash()
    {
        playerModel.DOScale(new Vector3(1.2f, 1.8f, 1.2f), 0.1f);
        yield return new WaitForSeconds(0.1f);
        playerModel.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.1f);
    }
    IEnumerator landSquash()
    {
        playerModel.DOScale(new Vector3(1.8f, 1.2f, 1.8f), 0.1f);
        yield return new WaitForSeconds(0.1f);
        playerModel.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.1f);
    }


    void OnDrawGizmos()
    {
        //show the model relative position in red
        if (playerModel != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerModel.position, playerModel.forward);
        }
        //show the transform position in green
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward);
        //show the cam relativeMove position in blue
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + CamRelativeMove, 0.1f);

        if (rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2);
        }

    }

    public void doJumpPad()
    {
        anim.SetInteger("JumpType", 1);
        anim.SetTrigger("Jump");
        canMove = false;
        jumpCount = 0;
        jCResetTimer = 0;
    }
}
