using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueBall : MonoBehaviour
{

    public Rigidbody ball;
    public Transform tee;

    public void Respawn()
    {
        ball.constraints = RigidbodyConstraints.None;
        ball.velocity = Vector3.zero;
        ball.position = tee.position;
    }
   
}
