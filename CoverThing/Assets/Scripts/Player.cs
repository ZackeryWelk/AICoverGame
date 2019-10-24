using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    //basic things
    public int moveSpeed;
    public GameObject player;
    public GameObject gun;
    public GameObject cameraHolder;

    //cover
    public float coverRunRange;
    bool inCover;
    NavMeshAgent agent;


    //camera looking stuff
    public float sensitivityX = 10f;
    public float sensitivityY = 10f;
    public float minimumX = -360f;
    public float maximumX = 360f;
    public float minimumY = -60f;
    public float maximumY = 60f;
    float rotationX = 0f;
    float rotationY = 0f;

    Quaternion originalRotation;


    // Start is called before the first frame update
    void Start()
    {
        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb)
        //    rb.freezeRotation = true;
        originalRotation = transform.localRotation;


        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (!inCover)
        {
            playerControls();
        }
        if(inCover)
        {
            playerCoverControls();
        }
        checkIfOnGround();
        //transform.position +=  Vector3.down * Time.deltaTime;

        
    }
    void playerControls()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * moveSpeed * Time.deltaTime;
            agent.enabled = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * moveSpeed * Time.deltaTime;
        }

        //cover
        RaycastHit hit;
        if (Input.GetKey(KeyCode.Q))
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, coverRunRange))
            {
                if (hit.transform.tag == "Cover")
                {
                    agent.enabled = true;
                    agent.destination = hit.transform.position;
                    inCover = true;

                    //head to cover
                }
            }
        }



        //camera looking stuff

        //Gets rotational input from the mouse
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;

        //Clamp the rotation average to be within a specific value range
        rotationY = ClampAngle(rotationY, minimumY, maximumY);
        rotationX = ClampAngle(rotationX, minimumX, maximumX);

        //Get the rotation you will be at next as a Quaternion
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);

        //Rotate
        cameraHolder.transform.rotation = originalRotation * xQuaternion * yQuaternion;
        //player.transform.localRotation = Quaternion.Euler(0, cameraHolder.transform.localRotation.y, 0);
        player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0,cameraHolder.transform.forward.z);
    }

    void playerCoverControls()
    {
        if (Input.GetKey(KeyCode.S))
        {
            agent.enabled = false;
            inCover = false;
        }
    }

    void checkIfOnGround()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position - new Vector3(0,-1,0),Vector3.down,out hit,100))
        {
            float distance = Vector3.Distance(transform.position - new Vector3(0, -1, 0), hit.transform.position);
            if(hit.transform.tag == "ground" && distance > 0.5f)
            {
                transform.position = new Vector3(0, hit.transform.position.y ,0);
            }
        }
    }


    float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }
}
