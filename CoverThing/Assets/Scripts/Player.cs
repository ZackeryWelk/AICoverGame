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

    //cover
    public float coverRunRange;
    public float inCoverRange;
    bool inCover;
    NavMeshAgent agent;
    private Vector3 savedCoverPos;

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
    private Vector3 bulletSpawnPos;
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

        ammoText.text = currentAmmo.ToString() + " / " + ammo.ToString();
    }
    void playerControls()
    {
        //movement
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
                    //head to cover
                }
            }
        }
        if (agent.enabled)
        {
            if (Vector3.Distance(transform.position, agent.destination) < inCoverRange)
            {
                inCover = true;
            }
        }


        //aiming stuff
        if (Input.GetKey(KeyCode.Mouse1))
        {
            lerpOutProgress = 0;
            aiming = true;
            if (lerpInProgress < 1)
            {
                lerpInProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(normalCamPos.position, AimCamPos.position, lerpInProgress);
            }
            RaycastHit laserSight;
            if(Physics.Raycast(bulletSpawnPos, bulletSpawn.transform.forward, out laserSight, gunRange))
            {
                Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
            }
        }
        else
        {
            lerpInProgress = 0;
            aiming = false;
            if (lerpOutProgress < 1)
            {
                lerpOutProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(AimCamPos.position, normalCamPos.position, lerpOutProgress);
            }
        }

        //shooting
        bulletSpawnPos = bulletSpawn.transform.position;

        if (aiming && Input.GetKey(KeyCode.Mouse0) && currentAmmo > 0 && !reloading)
        {
            Debug.DrawRay(bulletSpawnPos, bulletSpawn.transform.forward, Color.red, gunRange);

            if (fireRate < 0)
            {
                RaycastHit gunHit;
                if(Physics.Raycast(bulletSpawnPos,bulletSpawn.transform.forward,out gunHit, gunRange))
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
        fireRate -= Time.deltaTime;

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

    void playerCoverControls()
    {
        if (Input.GetKey(KeyCode.S))
        {
            agent.enabled = false;
            inCover = false;
        }
        if (!aiming)
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += -transform.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * moveSpeed * Time.deltaTime;
            }
        }

        //aiming stuff
        if (Input.GetKey(KeyCode.Mouse1))
        {
            agent.isStopped = true;
            
            lerpOutProgress = 0;
            aiming = true;
            if (lerpInProgress < 1)
            {
                lerpInProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(normalCamPos.position, AimCamPos.position, lerpInProgress);
            }
            RaycastHit laserSight;
            if (Physics.Raycast(bulletSpawnPos, bulletSpawn.transform.forward, out laserSight, gunRange))
            {
                Instantiate(laserSightObj, laserSight.point, Quaternion.Euler(laserSight.normal));
            }

            player.transform.forward = new Vector3(cameraHolder.transform.forward.x, 0, cameraHolder.transform.forward.z);
            bulletSpawn.transform.forward = new Vector3(cameraHolder.transform.forward.x, cameraHolder.transform.forward.y, cameraHolder.transform.forward.z);
        }
        else
        {
            agent.isStopped = false;

            lerpInProgress = 0;
            aiming = false;
            if (lerpOutProgress < 1)
            {
                lerpOutProgress += Time.deltaTime * lerpSpeed;
                cam.transform.position = Vector3.Lerp(AimCamPos.position, normalCamPos.position, lerpOutProgress);
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
