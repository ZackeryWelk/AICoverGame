using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public GameObject player;

    public Transform playerTopPoint;
    public Transform playerBottomPoint;

    public GameObject topPoint;
    public GameObject bottomPoint;

    private bool partialCover;
    private bool fullCover;

    // Start is called before the first frame update
    void Start()
    {
        
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
        if (Physics.Raycast(bottomPoint.transform.position, topPoint.transform.forward, out raycastBottom, Mathf.Infinity))
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



    }
}
