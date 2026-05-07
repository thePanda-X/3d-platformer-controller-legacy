using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class JumpPad : MonoBehaviour
{

    public float jumpForce = 10f;
    public GameObject JumpPadEffect;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.up * jumpForce;
            other.gameObject.GetComponent<PlatformerControllerRB>().doJumpPad();
            StartCoroutine(turnPlayer(other));
            var p = Instantiate(JumpPadEffect, transform.position, Quaternion.identity);
            Destroy(p, 1f);
        }
    }

    IEnumerator turnPlayer(Collider other)
    {
        yield return new WaitForSeconds(0.1f);
        Vector3 currentRotation = other.gameObject.GetComponent<PlatformerControllerRB>().playerModel.transform.eulerAngles;
        other.gameObject.GetComponent<PlatformerControllerRB>().playerModel.transform.DORotate(new Vector3(0, currentRotation.y + 720, 0), 0.5f, RotateMode.FastBeyond360);
    }
}
