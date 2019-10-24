using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletForce;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward * bulletForce);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
