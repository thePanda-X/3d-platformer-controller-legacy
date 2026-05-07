using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovinPlatform : MonoBehaviour
{
    public Transform[] Waypoints;
    Rigidbody rb;
    int currentWaypoint = 0;
    public float timeToNextWaypoint = 5f;
    public float moveDelay = 1f;
    public bool allowDebug;
    LineRenderer Lr;

    //enum with the 5 most common tweening ease types
    public enum EaseType
    {
        Linear, inoutSine, inoutQuad, inoutCubic, inoutQuart, inoutQuint
    };

    //the ease type to use for the tween
    public EaseType easeType = EaseType.Linear;

    private void Start()
    {
        transform.position = Waypoints[currentWaypoint].position;
        rb = GetComponent<Rigidbody>();
        Lr = transform.parent.GetComponent<LineRenderer>();
        if(Lr != null)
        {
            Lr.positionCount = Waypoints.Length;
            for (int i = 0; i < Waypoints.Length; i++)
            {
                Lr.SetPosition(i, Waypoints[i].position);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, Waypoints[currentWaypoint].position) < 0.1f)
        {
            currentWaypoint++;
            
            if(currentWaypoint >= Waypoints.Length)
            {
                currentWaypoint = 0;
            }

            StartCoroutine(updatePlatform());
        }
    } 

    IEnumerator updatePlatform()
    {
        
        yield return new WaitForSeconds(moveDelay);
        transform.DOMove(Waypoints[currentWaypoint].position, timeToNextWaypoint).SetEase(GetEaseType());
    }

    //returns the ease type based on the enum
    Ease GetEaseType()
    {
        switch (easeType)
        {
            case EaseType.Linear:
                return Ease.Linear;
            case EaseType.inoutSine:
                return Ease.InOutSine;
            case EaseType.inoutQuad:
                return Ease.InOutQuad;
            case EaseType.inoutCubic:
                return Ease.InOutCubic;
            case EaseType.inoutQuart:
                return Ease.InOutQuart;
            case EaseType.inoutQuint:
                return Ease.InOutQuint;
            default:
                return Ease.Linear;
        }
    }


    private void OnDrawGizmos()
    {
        if (allowDebug)
        {
            for (int i = 0; i < Waypoints.Length; i++)
            {
                Gizmos.color = Color.red;
                if (i == 0)
                {
                    Gizmos.DrawLine(Waypoints[0].position, Waypoints[Waypoints.Length - 1].position);
                }
                else
                {
                    Gizmos.DrawLine(Waypoints[i].position, Waypoints[i - 1].position);
                }

                Gizmos.color = Color.blue;
                if(transform.parent != null)
                    Gizmos.DrawWireCube(Waypoints[i].position, transform.parent.GetChild(0).localScale);

            }
        }
    }
}
