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

    public GameObject club_ghost;
    public GameObject arrow_ghost;
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
        // Put ghost copies of the club to show swing arc
        Transform closest_face = club_ghost.transform.GetChild(0).transform;
        Vector3 SwingLength = Vector3.zero;
        float closest_distance = 99999f;
        for (int i = 0; i < HistoryLength; i++) {
            
            GameObject club_trail = Instantiate(club_ghost, GhostHistory[i].position, GhostHistory[i].rotation);
            club_trail.GetComponent<SwingGhost>().original = false;
            club_trail.SetActive(true);
            // Find closest clubface to the ball for most accurate tracjectory at contact
            float distance_to_ball = (club_trail.transform.position - Ball.transform.position).magnitude;
            if (distance_to_ball < closest_distance) {
                closest_face = club_trail.transform.GetChild(0).transform;
                closest_distance = distance_to_ball;
            }
            
            // Find magnitude of gap between swing history positions
            // (One less gap than history length)
            if (i != HistoryLength - 1) {
                SwingLength -= SwingHistory[i + 1] - SwingHistory[i];
            }
        }
        // Add force to the ball in the direction of the clubface with the magnitude of the swing parallel to the closest clubface
        BallScript.UnfreezeBall();
        BallScript.MakeGhost();
        Vector3 ball_trajectory = closest_face.forward * (Vector3.Dot(SwingLength, closest_face.forward) * SwingMultiplier);
        Ball.AddForce(ball_trajectory);
        GameObject trajectory_ghost = Instantiate(arrow_ghost, Ball.position, Quaternion.LookRotation(ball_trajectory));
        trajectory_ghost.GetComponent<SwingGhost>().original = false;
        trajectory_ghost.SetActive(true);
    }
}
