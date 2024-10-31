using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SteeringWheel : MonoBehaviour
{
    public Transform hand_transform;
    private Boolean tracking;
    private float angle_turned;
    private float angle;
    private Vector2 prev_handpos;


    // Start is called before the first frame update
    void Start()
    {
        angle = 0.0f;
        prev_handpos = new Vector2(0, -1);
        tracking = false;
        UpdateAngle();
    }

    // Update is called once per frame
    void Update()
    {
        if (tracking) {
            UpdateAngle();
            transform.RotateAround(transform.position, transform.TransformDirection(0, 1, 0), angle_turned);
        }
    }

    public void BeginTracking(ActivateEventArgs args)
    {
        hand_transform = args.interactorObject.transform;
        tracking = true;
    }

    public void StopTracking()
    {
        tracking = false;
    }

    void UpdateAngle()
    {
        Vector3 hand_local_pos = transform.InverseTransformPoint(hand_transform.position);
        Vector2 hand_2d_norm = new Vector2(hand_local_pos.x, hand_local_pos.z).normalized;
        float angle_turned = Vector2.SignedAngle(prev_handpos, hand_2d_norm);
        prev_handpos = hand_2d_norm;
        angle += angle_turned;

        print(angle_turned);
        
    }
}
