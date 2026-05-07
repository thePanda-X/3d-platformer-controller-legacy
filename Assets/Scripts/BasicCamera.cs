using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BasicCamera : MonoBehaviour
{

    public Transform Target; //the target object
    public float xSensivity; //the sensitivity of the mouse on the x axis
    public float ySensivity; //the sensitivity of the mouse on the y axis
    float xrot = 0;         //the rotation around the x axis
    float yrot = 0;         //the rotation around the y axis
    public Vector3 CamOffset = new Vector3(0, 0.3f, 0); // custom angle offset to aim the where i want

    Vector3 camStartPos;

    public Transform Cam;   //the transform of the camera

    bool RTreleased = true;
    bool LTreleased = true;

    bool snapping = false;
    bool bumperCam = false;
    public float camSnapSpeed = 0.5f;   // time it takes for the camera to snap in 60 degree increments
    public LayerMask mask;



    // Start is called before the first frame update
    void Start()
    {
        camStartPos = Cam.localPosition;
    }

    void Update()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Target.position;
        if (!snapping)
        {
            xrot += xSensivity * Input.GetAxis("Mouse X");
            yrot += ySensivity * Input.GetAxis("Mouse Y");
        }


        if (Input.GetAxisRaw("RTrigger") > 0.2)
        {
            if (RTreleased)
            {
                RTreleased = false;
                //add 45 degrees to the camera x rotation
                transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y - 45, transform.localEulerAngles.z), camSnapSpeed);
                StartCoroutine(BlockCamSnap());
            }
        }
        else
        {
            RTreleased = true;
        }

        if (Input.GetAxisRaw("LTrigger") > 0.2)
        {
            if (LTreleased)
            {
                LTreleased = false;
                //subtract 45 degrees from the camera x rotation
                transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + 45, transform.localEulerAngles.z), camSnapSpeed);
                StartCoroutine(BlockCamSnap());
            }
        }
        else
        {
            LTreleased = true;
        }

        if (Input.GetButtonDown("Lbumper"))
        {
            var playerModel = GameObject.FindGameObjectWithTag("PlayerModel").transform;
            if (playerModel != null)
            {
                transform.DORotate(new Vector3(0, playerModel.eulerAngles.y, 0), camSnapSpeed);
                bumperCam = true;
                StartCoroutine(BlockCamSnap());
            }
        }

        yrot = Mathf.Clamp(yrot, -27.5f, 35f);
        Cam.LookAt(transform.position + CamOffset);
        if (!snapping)
        {
            transform.eulerAngles = new Vector3(yrot, xrot, 0);
        }

        
        
        WallclipHandler();

    }

    IEnumerator BlockCamSnap()
    {
        snapping = true;
        yield return new WaitForSeconds(camSnapSpeed);
        xrot = transform.localEulerAngles.y;

        if (bumperCam)
        {
            yrot = -5;
            bumperCam = false;
        }

        snapping = false;
    }

void WallclipHandler()
{
    RaycastHit hit;
    Vector3 rayDirection = Cam.position - transform.position;
    Vector3 smoothVelocity = Vector3.zero;
    float smoothTime = 0.1f;

    if (Physics.Raycast(transform.position, rayDirection, out hit, rayDirection.magnitude, mask))
    {
        Cam.position = hit.point;
    }
    else // No hit detected
    {
        // Return the camera to its original position smoothly
        Cam.localPosition = Vector3.SmoothDamp(Cam.localPosition, camStartPos, ref smoothVelocity, smoothTime);
    }
}




}
