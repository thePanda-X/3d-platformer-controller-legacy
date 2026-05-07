using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class controllerVisual : MonoBehaviour
{

    public RectTransform leftStick;
    public RectTransform rightStick;

    public Image Rtrigger;
    public Image Ltrigger;

    public Image Rbumper;
    public Image Lbumper;

    public Image jumpButton;
    public Image dashButton;

    // Update is called once per frame
    void Update()
    {
        rightStick.localPosition = new Vector3(Input.GetAxisRaw("Horizontal") * 20, Input.GetAxisRaw("Vertical") * 20, 0);
        leftStick.localPosition = new Vector3(Input.GetAxisRaw("Mouse X") * 20, Input.GetAxisRaw("Mouse Y") * -20, 0);

        if (Input.GetButton("Jump"))
        {
            jumpButton.color = Color.green;
        }
        else
        {
            jumpButton.color = Color.white;
        }

        if (Input.GetButton("Dash"))
        {
            dashButton.color = Color.green;
        }
        else
        {
            dashButton.color = Color.white;
        }

        if(Input.GetAxisRaw("RTrigger") > 0.2)
        {
            Rtrigger.color = Color.green;
        }
        else
        {
            Rtrigger.color = Color.white;
        }

        if (Input.GetAxisRaw("LTrigger") > 0.2)
        {
            Ltrigger.color = Color.green;
        }
        else
        {
            Ltrigger.color = Color.white;
        }

        if (Input.GetButton("Rbumper"))
        {
            Rbumper.color = Color.green;
        }
        else
        {
            Rbumper.color = Color.white;
        }

        if (Input.GetButton("Lbumper"))
        {
            Lbumper.color = Color.green;
        }
        else
        {
            Lbumper.color = Color.white;
        }
    }
}
