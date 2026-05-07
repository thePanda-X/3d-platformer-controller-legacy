using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cubelet : MonoBehaviour
{

    public float speed = 1f;
    public float rotationSpeed = 5f;
    public float pickUpDistance = 2f;
    Transform player;

    bool pickedUp = false;
    float timer = 0;

    public bool DoDebug = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.position) < pickUpDistance) //if the player is close enough to the cubelet pick it up
        {
            transform.position = Vector3.Lerp(transform.position, player.position + Vector3.up * 0.3f, speed * Time.deltaTime);
            //make it smaller over time
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, speed * Time.deltaTime);
            pickedUp = true;
        }

        if (Vector3.Distance(transform.position, player.position + Vector3.up * 0.3f) < 0.5f ) //if the player is close enough to the cubelet pick it up
        {
            Destroy(gameObject);
        }

        if(pickedUp)
        {
            timer += Time.deltaTime;
            if(timer > speed)
            {
               Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (DoDebug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, pickUpDistance);
        }
    }
}
