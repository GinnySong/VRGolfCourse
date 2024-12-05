using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SwingCalculator : MonoBehaviour
{
    public Rigidbody Ball;
    public BallPhysics BallScript;

    private bool TrackingSwing;

    private Vector3[] SwingHistory;
    private (Vector3 position, Quaternion rotation)[] GhostHistory;

    public Transform Clubface;
    private bool OnCooldown = false;

    private float SwingCooldownTimer = 0;

    private int SwingMultiplier = 20;
    // Start is called before the first frame update

    public GameObject ghost;
    public GameObject club;

    private int HistoryLength = 5;

    void Start()
    {
        TrackingSwing = false;
        SwingHistory = Enumerable.Repeat(Vector3.zero, HistoryLength).ToArray();
        GhostHistory = Enumerable.Repeat((club.transform.position, club.transform.rotation), HistoryLength).ToArray();
    }

    void UpdateHistory()
    {
        for (int i = HistoryLength; i > 1; i--) {
            SwingHistory[i - 1] = SwingHistory[i - 2];
            GhostHistory[i - 1] = GhostHistory[i - 2];
        }
        SwingHistory[0] = Clubface.position;
        GhostHistory[0] = (club.transform.position, club.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {

        if (TrackingSwing) {
            UpdateHistory();
        } 
        
        if (OnCooldown) {
            BallScript.UnfreezeBall();
            SwingCooldownTimer += Time.deltaTime;
            if (SwingCooldownTimer >= 1) {
                TrackingSwing = true;
                OnCooldown = false;
            }
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
        if (!OnCooldown) {
            HitBall();
            TrackingSwing = false;
            SwingCooldownTimer = 0;
            OnCooldown = true;
        }
    }

    public void HitBall()
    {
        // Calculate magnitude of the past two swing position changes
        // Put ghost copies of the club
        
        float SwingLength = 0;
        for (int i = 0; i < HistoryLength; i++) {

            GameObject new_ghost = Instantiate(ghost, GhostHistory[i].position, GhostHistory[i].rotation);
            new_ghost.GetComponent<SwingGhost>().original = false;
            new_ghost.SetActive(true);
            if (i != HistoryLength - 1) {
                // Find magnitude of gap between swing history positions
                SwingLength += (SwingHistory[i + 1] - SwingHistory[i]).magnitude;
            }
        }
        // Vector3 gapOne = SwingHistory[1] - SwingHistory[0];
        // Vector3 gapTwo = SwingHistory[2] - SwingHistory[1];

        //float SwingLength = gapOne.magnitude + gapTwo.magnitude;
        Vector3 SwingDirection = Clubface.transform.forward;

        BallScript.UnfreezeBall();
        Ball.AddForce(SwingDirection * (SwingLength * SwingMultiplier));
        print("Total power: " + (SwingDirection * SwingLength * SwingMultiplier));
    }
}
