using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    public Rigidbody ball;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        float friction = other.material.dynamicFriction;
        Vector3 friction_force = ball.velocity * friction * -0.2f;
        friction_force.y = friction_force.y / 2;
        ball.AddForce(friction_force);
        ball.AddTorque(ball.angularVelocity * friction * -0.2f);
        if (ball.velocity.magnitude < 0.3) {
            FreezeBall();
        }
    }
    
    public void FreezeBall()
    {
        ball.constraints = RigidbodyConstraints.FreezeAll;
        ball.maxAngularVelocity = 0;
    }

    public void UnfreezeBall()
    {
        ball.maxAngularVelocity = 7;
        ball.constraints = RigidbodyConstraints.None;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Clubface") {
            UnfreezeBall();
        }
    }
}
