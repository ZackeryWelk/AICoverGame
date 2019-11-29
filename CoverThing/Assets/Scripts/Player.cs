using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //basic things
    public int moveSpeed;
    public GameObject player;
    public GameObject gun;
    public GameObject bulletSpawn;
    public GameObject cameraHolder;
    public GameObject cam;
    private bool moving;

    //cover
    public float coverRunRange;
    public float inCoverRange;
    bool inCover;
    bool inHighCover;
    bool inLowCover;
    NavMeshAgent agent;
    private Vector3 savedCoverPos;
    public GameObject currentCover;
    private Vector3 lookDirection;
    public GameObject leftPoint;
    public GameObject rightPoint;
    public Transform coverCamPos;
    private float coverLerpIn;
    private float coverLerpOut;
    private float coverLerpSpeed = 5;
    private bool transferringCover;


    private float peekOutTimer = 1;
    private float peekInTimer = 1;
    private Vector3 temp;
    //left peek
    RaycastHit leftHit;
    bool leftEdge;
    public Transform leftPeekCamPos;

    //right peek
    RaycastHit rightHit;
    bool rightEdge;
    public Transform rightPeekCamPos;

    //aiming
    bool aiming;
    public Transform normalCamPos;
    public Transform AimCamPos;
    private float lerpInProgress = 0;
    private float lerpOutProgress = 0;
    public float lerpSpeed = 5;
    public GameObject laserSightObj;

    //shooting
    public float fireRate = 0.5f;
    private float initialFireRate;
    public int gunRange;
    public bool hitEnemy;
    public GameObject impact;
    public int ammo;
    private int currentAmmo;
    public Text ammoText;
    public Slider reloadSlider;
    private float reloadProgress;
    public float reloadTime;
    private float originalReloadTime;
    private bool reloading;


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


    //animation
    public Animator animator;



    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.localRotation;


        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        initialFireRate = fireRate;
        currentAmmo = ammo;
        originalReloadTime = reloadTime;
    }

    // Update is called once per frame
    void Update()
    {

        if (agent.enabled)
        {
            if(Input.GetKey(KeyCode.S))
            {
                agent.enabled = false;
                transferringCover = false;
            }
            if (Vector3.Distance(transform.position, agent.destination) < inCoverRange)
            {
                if (currentCover.transform.name == "LowCover")
                {
                    animator.SetBool("emergeRight", false);
                    animator.SetBool("emergeLeft", false);

                    animator.SetBool("movingCover", false);
                    animator.SetBool("running", false);
                    inLowCover = true;
                    transferringCover = false;
                }
                else if (currentCover.transform.name == "HighCover")
                {
                    animator.SetBool("emergeRight", false);
                    animator.SetBool("emergeLeft", false);

                    animator.SetBool("movingCover", false);
                    animator.SetBool("running", false);
                    inHighCover = true;
                    transferringCover = false;
                }
                inCover = true;
            }

        }

        if (!inCover)
        {
            playerControls();
        }
        if(inLowCover && !transferringCover)
        {
            playerLowCoverControls();
        }
        if(inHighCover && !transferringCover)
        {
            playerHighCoverControls();
        }
        checkIfOnGround();
        //transform.position +=  Vector3.down * Time.deltaTime;

        ammoText.text = currentAmmo.ToString() + " / " + ammo.ToString();
    }
    void playerControls()
    {
        fireRate -= Time.deltaTime;

        if(!aiming)
        {
            coverLerpIn = 0;
            if (coverLerpOut < 1)
            {
                coverLerpOut += Time.deltaTime * coverLerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, normalCamPos.position, coverLerpOut);
            }
        }

        //movement
        if (Input.GetKey(KeyCode.W))
        {
            moving = true;
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
            if(!aiming)
                animator.SetBool("running", true);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moving = true;
            transform.position += -transform.forward * moveSpeed * Time.deltaTime;
            agent.enabled = false;
            if(!aiming)
                animator.SetBool("running", true);
        }
        if (Input.GetKey(KeyCode.A))
        {
            moving = true;
            transform.position += -transform.right * moveSpeed * Time.deltaTime;
            if(!aiming)
                animator.SetBool("running", true);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moving = true;
            transform.position += transform.right * moveSpeed * Time.deltaTime;
            if(!aiming)
                animator.SetBool("running", true);
        }

        if(!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !agent.enabled)
        {
            moving = false;
            animator.SetBool("running", false);
        }
        



        //cover
        RaycastHit hit;
        if (Input.GetKey(KeyCode.Q))
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, coverRunRange))
            {
                if (hit.transform.tag == "Cover")
                {
                    lookDirection = hit.normal;
                    currentCover = hit.transform.gameObject;
                    agent.enabled = true;
                    agent.destination = hit.transform.position;
                    animator.speed = agent.speed/7;
                    animator.SetBool("running", true);
                    transferringCover = true;
                    //head to cover
                }
            }
        }
        if (agent.enabled)
        {
            if (Vector3.Distance(transform.position, agent.destination) < inCoverRange)
            {
                if(currentCover.transform.name == "LowCover")
                {
                    animator.SetBool("running", false);
                    inLowCover = true;
                    transferringCover = false;
                }
                else if(currentCover.transform.name == "HighCover")
                {
                    animator.SetBool("running", false);
                    inHighCover = true;
                    transferringCover = false;
                }
                inCover = true;
            }
        }


        //aiming stuff
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if(moving)
            {
                animator.SetBool("aimRunning", true);
                animator.SetBool("idleAim", false);
            }
            else
            {
                animator.SetBool("idleAim", true);
                animator.SetBool("aimRunning", false);
            }
            lerpOutProgress = 0;
            aiming = true;
            if (lerpInProgress < 1)
            {
                lerpInProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, AimCamPos.position, lerpInProgress);
            }
            RaycastHit laserSight;
            if(Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out laserSight, gunRange))
            {
                Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
            }
        }
        else
        {
            animator.SetBool("aimRunning", false);
            animator.SetBool("idleAim", false);
            
            lerpInProgress = 0;
            aiming = false;
            if (lerpOutProgress < 1)
            {
                lerpOutProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, normalCamPos.position, lerpOutProgress);
            }
        }

        //shooting

        if (aiming && Input.GetKey(KeyCode.Mouse0) && currentAmmo > 0 && !reloading)
        {
            Debug.DrawRay(bulletSpawn.transform.position, bulletSpawn.transform.forward, Color.red, gunRange);

            if (fireRate < 0)
            {
                RaycastHit gunHit;
                if(Physics.Raycast(bulletSpawn.transform.position,bulletSpawn.transform.forward,out gunHit, gunRange))
                {
                    if(gunHit.transform.tag == "Enemy")
                    {
                        
                        hitEnemy = true;
                    }
                    else
                    {
                        Instantiate(impact, gunHit.point, Quaternion.Euler(gunHit.normal));
                    }
                }
                fireRate = initialFireRate;
                currentAmmo--;
            }
        }

        if(Input.GetKey(KeyCode.R))
        {
            reloading = true;
        }
        if(reloading)
        {
            reloadTime -= Time.deltaTime;
            reloadProgress = 1 - (reloadTime/originalReloadTime);
            reloadSlider.value = reloadProgress;

            if (reloadTime < 0)
            {
                currentAmmo = ammo;
                reloadTime = originalReloadTime;
                reloading = false;
                reloadSlider.value = 0;
            }
        }

        //free camera looking stuff

        //Gets rotational input from the mouse
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;

        //Clamp the rotation average to be within a specific value range
        rotationY = ClampAngle(rotationY, minimumY, maximumY);
        rotationX = ClampAngle(rotationX, minimumX, maximumX);

        //Get the rotation you will be at next as a Quaternion
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);

        //if (!aiming)
        //{
            //Rotate
            cameraHolder.transform.rotation = originalRotation * xQuaternion * yQuaternion;
            //turns the player
            player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0, cameraHolder.transform.forward.z);
        bulletSpawn.transform.forward = new Vector3(cameraHolder.transform.forward.x, cameraHolder.transform.forward.y, cameraHolder.transform.forward.z);
        //}
    }

    void playerLowCoverControls()
    {
        fireRate -= Time.deltaTime;

        coverLerpOut = 0;
        if (coverLerpIn < 1)
        {
            coverLerpIn += Time.deltaTime * coverLerpSpeed;
            cam.transform.position = Vector3.Lerp(cam.transform.position, coverCamPos.position, coverLerpIn);
        }

        if (agent.enabled)
        {

            if (Vector3.Distance(transform.position, agent.destination) < inCoverRange)
            {
                animator.Play("lowCoverIdle");
                
                inLowCover = true;
                agent.enabled = false;
                transferringCover = false;
            }
        }


        //backs out of cover
        if (Input.GetKey(KeyCode.S))
        {
            animator.Play("idle");
            inCover = false;
            inLowCover = false;
        }
        //moving in cover(limits you so you cant walk sideways out of cover 
        if (!aiming)
        {

            //look direction makes player look into cover
            transform.forward = lookDirection;
            //moving right
           if(Physics.Raycast(leftPoint.transform.position,-leftPoint.transform.forward,out leftHit,Mathf.Infinity))
           {
                if(leftHit.transform.gameObject == currentCover && Input.GetKey(KeyCode.D))
                {
                    animator.SetBool("lowCoverLeft", true);
                    transform.position += -transform.right * moveSpeed * Time.deltaTime;
                    leftEdge = false;
                }
                else if(leftHit.transform.gameObject != currentCover)
                {
                    animator.SetBool("lowCoverLeft", false);
                    leftEdge = true;
                }
                else
                {
                    animator.SetBool("lowCoverLeft", false);
                    leftEdge = false;
                }
           }
           //moving left
           if (Physics.Raycast(rightPoint.transform.position, -rightPoint.transform.forward, out rightHit, Mathf.Infinity))
           {
               if (rightHit.transform.gameObject == currentCover && Input.GetKey(KeyCode.A))
               {
                    animator.SetBool("lowCoverRight", true);
                    transform.position += transform.right * moveSpeed * Time.deltaTime;
                   rightEdge = false;
               }
               else if(rightHit.transform.gameObject != currentCover)
               {
                    animator.SetBool("lowCoverRight", false);
                    rightEdge = true;
               }
               else
               {
                    animator.SetBool("lowCoverRight", false);
                    rightEdge = false;
               }
           }

        }

        //aiming out of left side of cover
        if (leftEdge)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                animator.SetBool("emergeRight",true);
                peekOutTimer -= Time.deltaTime;
                peekInTimer = 1;
                //agent.isStopped = true;

                lerpOutProgress = 0;
                aiming = true;
                if (lerpInProgress < 1)
                {
                    lerpInProgress += Time.deltaTime * lerpSpeed;
                    cam.transform.position = Vector3.Lerp(cam.transform.position, leftPeekCamPos.position, lerpInProgress);
                    transform.position = Vector3.Lerp(transform.position, currentCover.transform.GetChild(1).position, lerpInProgress);
                }
                RaycastHit laserSight;
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out laserSight, gunRange))
                {
                    Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
                }

                player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0, cameraHolder.transform.forward.z);
                bulletSpawn.transform.forward = new Vector3(cameraHolder.transform.forward.x, cameraHolder.transform.forward.y, cameraHolder.transform.forward.z);

                RaycastHit hit;
                if (Input.GetKey(KeyCode.Q))
                {
                    if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out hit, coverRunRange))
                    {
                        if (hit.transform.tag == "Cover")
                        {
                            lookDirection = hit.normal;
                            currentCover = hit.transform.gameObject;
                            agent.enabled = true;
                            agent.destination = hit.transform.position;
                            animator.SetBool("movingCover", true);
                            transferringCover = true;
                            aiming = false;
                            //head to cover
                        }
                    }
                }
                RaycastHit particleHit;
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out particleHit, coverRunRange))
                {
                    if (particleHit.transform.tag == "Cover" && particleHit.transform.gameObject != currentCover)
                    {
                        particleHit.transform.gameObject.GetComponent<ParticleSystem>().Play();
                    }
                }
            }
            else
            {
                animator.SetFloat("Direction", -1);
                animator.SetBool("emergeRight",false);
                //agent.isStopped = false;
                peekInTimer -= Time.deltaTime;
                peekOutTimer = 1;
                lerpInProgress = 0;
                //aiming = false;
                if (lerpOutProgress < 1)
                {
                    lerpOutProgress += Time.deltaTime * lerpSpeed;
                    cam.transform.position = Vector3.Lerp(cam.transform.position, coverCamPos.position, lerpOutProgress);
                    transform.position = Vector3.Lerp(transform.position, currentCover.transform.GetChild(3).position, lerpOutProgress);

                }
                if (lerpOutProgress >= 0.99f)
                {
                    aiming = false;
                }
            }
        }

        //aiming out of right side of cover
        if (rightEdge)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                //agent.isStopped = true;
                animator.SetBool("emergeLeft", true);
                lerpOutProgress = 0;
                aiming = true;
                if (lerpInProgress < 1)
                {
                    lerpInProgress += Time.deltaTime * lerpSpeed;
                    cam.transform.position = Vector3.Lerp(cam.transform.position, rightPeekCamPos.position, lerpInProgress);
                    transform.position = Vector3.Lerp(transform.position, currentCover.transform.GetChild(0).position, lerpInProgress);
                }
                RaycastHit laserSight;
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out laserSight, gunRange))
                {
                    Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
                }

                player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0, cameraHolder.transform.forward.z);
                bulletSpawn.transform.forward = new Vector3(cameraHolder.transform.forward.x, cameraHolder.transform.forward.y, cameraHolder.transform.forward.z);

                RaycastHit hit;
                if (Input.GetKey(KeyCode.Q))
                {
                    if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out hit, coverRunRange))
                    {
                        if (hit.transform.tag == "Cover")
                        {
                            lookDirection = hit.normal;
                            currentCover = hit.transform.gameObject;
                            agent.enabled = true;
                            agent.destination = hit.transform.position;
                            animator.SetBool("movingCover", true);
                            transferringCover = true;
                            aiming = false;
                            animator.SetBool("emergeRight", false);

                            //head to cover
                        }
                    }
                }
                RaycastHit particleHit;
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out particleHit, coverRunRange))
                {
                    if (particleHit.transform.tag == "Cover" && particleHit.transform.gameObject != currentCover)
                    {
                        particleHit.transform.gameObject.GetComponent<ParticleSystem>().Play();
                    }
                }
            }
            else
            {
                //agent.isStopped = false;
                animator.SetFloat("Direction", -1);
                animator.SetBool("emergeLeft", false);
                lerpInProgress = 0;
                aiming = false;
                if (lerpOutProgress < 1)
                {
                    lerpOutProgress += Time.deltaTime * lerpSpeed;
                    cam.transform.position = Vector3.Lerp(cam.transform.position, coverCamPos.position, lerpOutProgress);
                    transform.position = Vector3.Lerp(transform.position, currentCover.transform.GetChild(2).position, lerpOutProgress);
                }
                if (lerpOutProgress >= 0.99f)
                {
                    aiming = false;
                }

            }
        }

        //aiming stuff
        if (Input.GetKey(KeyCode.Mouse1) && !leftEdge && !rightEdge)
        {
            //agent.isStopped = true;
            
            lerpOutProgress = 0;
            aiming = true;
            if (lerpInProgress < 1)
            {
                lerpInProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, AimCamPos.position, lerpInProgress);
            }
            RaycastHit laserSight;
            if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out laserSight, gunRange))
            {
                Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
            }

            player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0, cameraHolder.transform.forward.z);
            bulletSpawn.transform.forward = new Vector3(cameraHolder.transform.forward.x, cameraHolder.transform.forward.y, cameraHolder.transform.forward.z);

            RaycastHit hit;
            if (Input.GetKey(KeyCode.Q))
            {
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out hit, coverRunRange))
                {
                    if (hit.transform.tag == "Cover")
                    {
                        lookDirection = hit.normal;
                        currentCover = hit.transform.gameObject;
                        agent.enabled = true;
                        agent.destination = hit.transform.position;
                        animator.SetBool("movingCover", true);

                        transferringCover = true;
                        aiming = false;
                        animator.SetBool("emergeLeft", false);

                        //head to cover
                    }
                }
            }
            RaycastHit particleHit;
            if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out particleHit, coverRunRange))
            {
                if (particleHit.transform.tag == "Cover" && particleHit.transform.gameObject != currentCover)
                {
                    particleHit.transform.gameObject.GetComponent<ParticleSystem>().Play();
                }
            }
        }
        else if(!leftEdge && !rightEdge)
        {
            //agent.isStopped = false;

            lerpInProgress = 0;
            aiming = false;
            if (lerpOutProgress < 1)
            {
                lerpOutProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, coverCamPos.position, lerpOutProgress);
            }
        }

        //shooting
        if (aiming && Input.GetKey(KeyCode.Mouse0) && currentAmmo > 0 && !reloading)
        {
            Debug.DrawRay(bulletSpawn.transform.position, bulletSpawn.transform.forward, Color.red, gunRange);

            if (fireRate < 0)
            {
                RaycastHit gunHit;
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out gunHit, gunRange))
                {
                    if (gunHit.transform.tag == "Enemy")
                    {

                        hitEnemy = true;
                    }
                    else
                    {
                        Instantiate(impact, gunHit.point, Quaternion.Euler(gunHit.normal));
                    }
                }
                fireRate = initialFireRate;
                currentAmmo--;
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            reloading = true;
        }
        if (reloading)
        {
            reloadTime -= Time.deltaTime;
            reloadProgress = 1 - (reloadTime / originalReloadTime);
            reloadSlider.value = reloadProgress;

            if (reloadTime < 0)
            {
                currentAmmo = ammo;
                reloadTime = originalReloadTime;
                reloading = false;
                reloadSlider.value = 0;
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
    }

    void playerHighCoverControls()
    {

        fireRate -= Time.deltaTime;

        coverLerpOut = 0;
        if (coverLerpIn < 1)
        {
            coverLerpIn += Time.deltaTime * coverLerpSpeed;
            cam.transform.position = Vector3.Lerp(cam.transform.position, coverCamPos.position, coverLerpIn);
        }

        if (agent.enabled)
        {
            if (Vector3.Distance(transform.position, agent.destination) < inCoverRange)
            {
                inHighCover = true;
                agent.enabled = false;
            }
        }


        //backs out of cover
        if (Input.GetKey(KeyCode.S))
        {
            inHighCover = false;
            inCover = false;
        }
        //moving in cover(limits you so you cant walk sideways out of cover 
        if (!aiming)
        {

            //look direction makes player look into cover
            transform.forward = -lookDirection;
            //RaycastHit leftHit;
            if (Physics.Raycast(leftPoint.transform.position, leftPoint.transform.forward, out leftHit, Mathf.Infinity))
            {
                if (leftHit.transform.gameObject == currentCover && Input.GetKey(KeyCode.A))
                {
                    transform.position += -transform.right * moveSpeed * Time.deltaTime;
                }
            }
            //RaycastHit rightHit;
            if (Physics.Raycast(rightPoint.transform.position, rightPoint.transform.forward, out rightHit, Mathf.Infinity))
            {
                if (rightHit.transform.gameObject == currentCover && Input.GetKey(KeyCode.D))
                {
                    transform.position += transform.right * moveSpeed * Time.deltaTime;
                }
            }

        }

        //aiming stuff
        if (Input.GetKey(KeyCode.Mouse1))
        {
            //agent.isStopped = true;

            lerpOutProgress = 0;
            aiming = true;
            if (lerpInProgress < 1)
            {
                lerpInProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, AimCamPos.position, lerpInProgress);
            }
            RaycastHit laserSight;
            if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out laserSight, gunRange))
            {
                Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
            }

            player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0, cameraHolder.transform.forward.z);
            bulletSpawn.transform.forward = new Vector3(cameraHolder.transform.forward.x, cameraHolder.transform.forward.y, cameraHolder.transform.forward.z);

            RaycastHit hit;
            if (Input.GetKey(KeyCode.Q))
            {
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out hit, coverRunRange))
                {
                    if (hit.transform.tag == "Cover")
                    {
                        lookDirection = hit.normal;
                        currentCover = hit.transform.gameObject;
                        agent.enabled = true;
                        agent.destination = hit.transform.position;
                        //head to cover
                    }
                }
            }
            RaycastHit particleHit;
            if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out particleHit, coverRunRange))
            {
                if (particleHit.transform.tag == "Cover" && particleHit.transform.gameObject != currentCover)
                {
                    particleHit.transform.gameObject.GetComponent<ParticleSystem>().Play();
                }
            }
        }
        else
        {
            //agent.isStopped = false;

            lerpInProgress = 0;
            aiming = false;
            if (lerpOutProgress < 1)
            {
                lerpOutProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(cam.transform.position, coverCamPos.position, lerpOutProgress);
            }
        }

        //shooting
        if (aiming && Input.GetKey(KeyCode.Mouse0) && currentAmmo > 0 && !reloading)
        {
            Debug.DrawRay(bulletSpawn.transform.position, bulletSpawn.transform.forward, Color.red, gunRange);

            if (fireRate < 0)
            {
                RaycastHit gunHit;
                if (Physics.Raycast(bulletSpawn.transform.position, bulletSpawn.transform.forward, out gunHit, gunRange))
                {
                    if (gunHit.transform.tag == "Enemy")
                    {

                        hitEnemy = true;
                    }
                    else
                    {
                        Instantiate(impact, gunHit.point, Quaternion.Euler(gunHit.normal));
                    }
                }
                fireRate = initialFireRate;
                currentAmmo--;
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            reloading = true;
        }
        if (reloading)
        {
            reloadTime -= Time.deltaTime;
            reloadProgress = 1 - (reloadTime / originalReloadTime);
            reloadSlider.value = reloadProgress;

            if (reloadTime < 0)
            {
                currentAmmo = ammo;
                reloadTime = originalReloadTime;
                reloading = false;
                reloadSlider.value = 0;
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
