using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //public float speed=5.0f;
    //public float turnSpeed;
   // public float horizontalInput;
    //public float forwardInput;
    private float speed=5.0f;
    private float turnSpeed=25.0f;
    private float horizontalInput;
    private float forwardInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {//this is where we get player input
        horizontalInput=Input.GetAxis("Horizontal");
        forwardInput=Input.GetAxis("Vertical");
        //we'll move the vehicle forward
        transform.Translate(Vector3.forward * Time.deltaTime*speed*forwardInput);
        //transform.Translate(Vector3.right * Time.deltaTime*turnSpeed*horizontalInput);
        //we turn the vehicle
        transform.Rotate(Vector3.up , turnSpeed * horizontalInput * Time.deltaTime);
    }
}
