using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;
     
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position; 
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.transform.position + offset;
        //transform.rotation = new quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, 1);
    }
}
