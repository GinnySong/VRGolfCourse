using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Throttle : MonoBehaviour
{
    public SteeringWheel WheelScript;
    public Transform hand_transform;
    public bool tracking;
    private float angle_turned;
    private float old_angle;
    private float total_angle;
    public float acceleration;

    private Vector2 normal_handpos;
    private Vector2 prev_handpos;
    public GameObject knob;

    private float MAX_ANGLE = 15f;
    private float BRAKE_ANGLE = -10f;
    private float MIN_ANGLE = -15f;
    private Vector2 reset_hand = new Vector2(-0.2f, -0.9f).normalized;

    // public TextMeshProUGUI display;


    // Start is called before the first frame update
    void Start()
    {
        total_angle = 0.0f;
        old_angle = 0.0f;
        prev_handpos = Vector2.down;
        StopTracking();
    }

    // Update is called once per frame
    void Update()
    {
        if (tracking) {
            Vector3 local_handpos = transform.InverseTransformPoint(hand_transform.position);
            normal_handpos = new Vector2(local_handpos.x, local_handpos.z).normalized;
            UpdateAngle();
            UpdateAcceleration();
            if (acceleration > 0) {
                knob.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            } else {
                knob.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
        }
    }

    public void BeginTracking(SelectEnterEventArgs args)
    {
        hand_transform = args.interactorObject.transform;
        tracking = true;
    }

    public void StopTracking()
    {
        tracking = false;

        // Find the total angle needed to put the throttle in brake, convert to radians
        float angle_needed = MIN_ANGLE + 0.01f - total_angle;
        angle_needed = angle_needed * Mathf.PI / 180;
        // Set the "hand position" to turn it back by the needed angle
        normal_handpos = new Vector2(Mathf.Sin(angle_needed), -Mathf.Cos(angle_needed));
        UpdateAngle();
        UpdateAcceleration();
        knob.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    void UpdateAngle()
    {
        angle_turned = old_angle + Vector2.SignedAngle(prev_handpos, normal_handpos);
        prev_handpos = normal_handpos;
        old_angle = angle_turned;
        if(MIN_ANGLE < total_angle + angle_turned && total_angle + angle_turned < MAX_ANGLE) {
            total_angle += angle_turned;
            transform.RotateAround(transform.position, transform.TransformDirection(0, -1, 0), angle_turned);
        }
        // SetText(total_angle, angle_turned);
    }

    void UpdateAcceleration()
    {
        if (total_angle > BRAKE_ANGLE) {
            acceleration = Mathf.InverseLerp(BRAKE_ANGLE, MAX_ANGLE, total_angle);
        } else {
            acceleration = -1f;
        }
        WheelScript.SetAcceleration(acceleration);
    }

    // void SetText(float angle, float change)
    // {
    //     display.text = string.Format("Angle: {0:0.00}\nChange in angle: {1:0.00}\nPrevHandPos: {2}", angle, change, prev_handpos.ToString());
    // }
}
