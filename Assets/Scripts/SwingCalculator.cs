using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SwingCalculator : MonoBehaviour
{
    public Rigidbody Ball;
    public BallPhysics BallScript;

    private bool TrackingSwing;

    private Vector3[] SwingHistory;

    public Transform Clubface;

    private int SwingCooldownTimer = 0;

    private int SwingMultiplier = 20;
    // Start is called before the first frame update
    void Start()
    {
        TrackingSwing = false;
        SwingHistory = new Vector3[] {Vector3.zero, Vector3.zero, Vector3.zero};
    }

    void UpdateHistory()
    {
        SwingHistory[2] = SwingHistory[1];
        SwingHistory[1] = SwingHistory[0];
        SwingHistory[0] = Clubface.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (TrackingSwing) {
            UpdateHistory();
        } else if (SwingCooldownTimer >= 0) {
            // Timer in place so ball isn't hit twice
            SwingCooldownTimer--;
        } else {
            TrackingSwing = true;
            // Necessary to set after a delay so that the club does not
            // impart friction on the ball
            BallScript.ReadyToHit = false;
        }
    }

    public void UpdateTracking(bool track)
    {
        TrackingSwing = track;
    }

    public void UpdateSwingMultiplier(int multi)
    {
        SwingMultiplier = multi;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Ball") && TrackingSwing)
        {
            HitBall();
            TrackingSwing = false;
            SwingCooldownTimer = 5;
        }
    }

    public void HitBall()
    {
        // Calculate magnitude of the past two swing position changes
        print("Club collide");
        Vector3 gapOne = SwingHistory[1] - SwingHistory[0];
        Vector3 gapTwo = SwingHistory[2] - SwingHistory[1];

        float SwingLength = gapOne.magnitude + gapTwo.magnitude;
        Vector3 SwingDirection = Clubface.transform.forward;

        BallScript.UnfreezeBall();
        Ball.AddForce(SwingDirection * (SwingLength * SwingMultiplier));
    }
}
