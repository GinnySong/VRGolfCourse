using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    private Vector2 prev_handpos;

    // public TextMeshProUGUI display;


    // Start is called before the first frame update
    void Start()
    {
        total_angle = 0.0f;
        old_angle = 0.0f;
        prev_handpos = new Vector2(0, -1);
        tracking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (tracking) {
            UpdateAngle();
            UpdateAcceleration();
        }
    }

    public void BeginTracking(SelectEnterEventArgs args)
    {
        hand_transform = args.interactorObject.transform;
        tracking = true;
    }

    public void StopTracking()
    {
        Start();
    }

    void UpdateAngle()
    {
        Vector3 hand_local_pos = transform.InverseTransformPoint(hand_transform.position);
        Vector2 hand_2d_norm = new Vector2(hand_local_pos.x, hand_local_pos.z).normalized;
        angle_turned = old_angle + Vector2.SignedAngle(prev_handpos, hand_2d_norm);
        prev_handpos = hand_2d_norm;
        old_angle = angle_turned;
        if(-30 < total_angle + angle_turned && total_angle + angle_turned < 30) {
            total_angle += angle_turned;
            transform.RotateAround(transform.position, transform.TransformDirection(0, -1, 0), angle_turned);
        }
        // SetText(total_angle, angle_turned);
    }

    void UpdateAcceleration()
    {
        acceleration = total_angle / 30.0f;
        WheelScript.SetAcceleration(acceleration);
    }

    // void SetText(float angle, float change)
    // {
    //     display.text = string.Format("Angle: {0:0.00}\nChange in angle: {1:0.00}\nPrevHandPos: {2}", angle, change, prev_handpos.ToString());
    // }
}
