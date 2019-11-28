using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AI : MonoBehaviour
{
    public GameObject player;
    public NavMeshAgent agent;

    public Transform playerTopPoint;
    public Transform playerBottomPoint;

    public GameObject topPoint;
    public GameObject bottomPoint;

    public List<GameObject> cover;
    public GameObject highCover;
    public GameObject lowCover;

    private GameObject target;
    private GameObject navMeshTarget;
    private float currentCoverTargetDis = 0;
    private float currentTargetDis = 0;

    private bool partialCover;
    private bool fullCover;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < highCover.transform.childCount; i++)
        {
            cover.Add(highCover.transform.GetChild(i).gameObject);
        }
        for(int j = 0; j < lowCover.transform.childCount; j++)
        {
            cover.Add(lowCover.transform.GetChild(j).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //makes the point at the top and bottom of an enemy look at the respective points on the player to later check if they are in cover in relation to the player
        topPoint.transform.LookAt(playerTopPoint);
        bottomPoint.transform.LookAt(playerBottomPoint);

        RaycastHit raycastTop;
        if(Physics.Raycast(topPoint.transform.position,topPoint.transform.forward,out raycastTop,Mathf.Infinity))
        {
            if(raycastTop.transform.tag == "Player")
            {
                fullCover = false;
            }
            else
            {
                fullCover = true;
            }
        }

        RaycastHit raycastBottom;
        if(Physics.Raycast(bottomPoint.transform.position, bottomPoint.transform.forward, out raycastBottom, Mathf.Infinity))
        {
            if (raycastBottom.transform.tag == "Player")
            {
                partialCover = false;
            }
            else
            {
                partialCover = true;
            }
        }

        
        if(partialCover && !fullCover)
        {
            agent.speed = 5;
        }
        if(!partialCover && !fullCover)
        {
            agent.speed = 10;
            FindCover();
            agent.destination = navMeshTarget.transform.position;
        }

    }

    void FindCover()
    {
        float temp;
        for (int i = 0; i < cover.Capacity; i++)
        {
            temp = Vector3.Distance(player.transform.position, cover[i].transform.position);
            if (currentCoverTargetDis == 0)
            {
                currentCoverTargetDis = temp;
            }
            if (temp >= currentCoverTargetDis)
            {
                currentCoverTargetDis = temp;
                target = cover[i];
            }
        }
        float temp2;
        for (int j = 0; j < target.transform.childCount; j++)
        {
            temp2 = Vector3.Distance(player.transform.position, target.transform.GetChild(j).position);
            if (currentTargetDis == 0)
            {
                currentTargetDis = temp2;
            }
            if (temp2 >= currentTargetDis)
            {
                currentTargetDis = temp2;
                navMeshTarget = target.transform.GetChild(j).gameObject;
            }
        }
        currentCoverTargetDis = 0;
        currentTargetDis = 0;
    }
}
