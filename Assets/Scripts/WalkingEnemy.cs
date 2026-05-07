using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WalkingEnemy : MonoBehaviour
{
    // The player character
    public GameObject player;

    // The distance at which the enemy can see the player
    public float sightRadius = 10f;

    // The distance at which the enemy can attack the player
    public float attackRange = 1f;

    // The speed at which the enemy moves towards the player
    public float moveSpeed = 3f;

    // The amount of time the enemy must wait between attacks
    public float attackDelay = 1f;

    // The time when the enemy can attack again
    private float nextAttackTime;

    // The distance the enemy will wander from its starting position
    public float wanderRadius = 5f;

    // The point the enemy will wander towards
    private Vector3 wanderPoint;

    // The enemy's rigidbody
    private Rigidbody rb;

    // home of the enemy (where it will wander back to)
    Vector3 home;

    // if the is too far from home, it will wander back to home.
    public float homeReturnDistance = 5f;

    bool isFrozen = false;

    bool wasChasing = false;

    public GameObject DeathFx;
    bool isMarkedForDeath = false;

    void Start()
    {
        // Get the enemy's rigidbody
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        home = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFrozen) 
        {
            return;
        }
        // Get the distance between the enemy and the player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (Vector3.Distance(transform.position, home) > homeReturnDistance)
        {
            StartCoroutine(ReturnHome(1f));
        }

        // If the player is within the enemy's sight radius
        if (distance < sightRadius)
        {
            wasChasing = true;
            // Face the player
            var lookAtPos = player.transform.position;
            lookAtPos.y = transform.position.y;
            transform.LookAt(lookAtPos);

            // Move towards the player
            Vector3 moveDirection = (lookAtPos - transform.position).normalized;
            
            //make moveDirection unitary
            moveDirection = moveDirection / moveDirection.magnitude;

            rb.linearVelocity = moveDirection * moveSpeed;

            // If the player is within the enemy's attack range and it's time to attack again
            if (distance < attackRange && Time.time > nextAttackTime)
            {
                // Attack the player
                Debug.Log("Attacking player!");

                // Set the time when the enemy can attack again
                nextAttackTime = Time.time + attackDelay;
            }
        }
        // If the player is not within the enemy's sight radius
        else
        {
            // If the enemy was chasing the player, set the wander point to home and set wasChasing to false
            if (wasChasing)
            {
                RaycastHit homeGroundHit;
                if (Physics.Raycast(home, Vector3.down, out homeGroundHit))
                {
                    wanderPoint = homeGroundHit.point;
                }
                wasChasing = false;
            }
            // If the enemy doesn't have a wander point, choose a new one
            if (wanderPoint == Vector3.zero)
            {
                wanderPoint = ChooseWanderPoint();
            }

            // Move towards the wander point
            Vector3 moveDirection = (wanderPoint - transform.position).normalized;
            rb.linearVelocity = moveDirection * moveSpeed;

            // If the enemy has reached the wander point, reset it to zero
            if (Vector3.Distance(transform.position, wanderPoint) < 0.5f)
            {
                wanderPoint = Vector3.zero;
            }
        }
    }

    IEnumerator ReturnHome(float freezeTime)
    {
        isFrozen = true;
        transform.DOScale(0, freezeTime/2);
        yield return new WaitForSeconds(freezeTime);
        transform.position = home;
        transform.DOScale(1, freezeTime/2);
        isFrozen = false;
    }

    // Choose a random point within the wander radius to wander towards
    private Vector3 ChooseWanderPoint()
    {
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(0f, wanderRadius);
        return transform.position + new Vector3(Mathf.Sin(randomAngle), 0, Mathf.Cos(randomAngle)) * randomDistance;
    }

    // Draw the sight radius and attack range in the scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        //show the wander point
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wanderPoint, 0.5f);
        // show home radius
        Gizmos.color = Color.yellow;
        if (home != Vector3.zero)
        { Gizmos.DrawWireSphere(home, homeReturnDistance); }
        else
        { Gizmos.DrawWireSphere(transform.position, homeReturnDistance); }

    }

    public void TriggerDeath()
    {
        isMarkedForDeath = true;
        isFrozen = true;
        //reset x and z rotation
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.DOScale(new Vector3(1, 0.2f, 1), 0.25f);

        var clone = Instantiate(DeathFx, transform.position, Quaternion.identity);
        Destroy(clone, 1f);
        Destroy(gameObject, 0.3f);
    }

    public bool GetIsMarkedForDeath()
    {
        return isMarkedForDeath;
    }
}
