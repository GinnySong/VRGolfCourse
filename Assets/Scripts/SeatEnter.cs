using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatEnter : MonoBehaviour
{
    public Transform seatPosition;
    public GameObject player;

    private bool isSitting;
    private Collider playerCollider;

    void Start()
    {
        isSitting = false;
        playerCollider = player.GetComponent<Collider>();
    }

    void Update()
    {
        if (isSitting)
        {
            player.transform.position = seatPosition.position;
            player.transform.rotation = seatPosition.rotation;
        }
    }

    public void SitDown()
    {
        isSitting = true;
        player.transform.position = seatPosition.position;
        player.transform.rotation = seatPosition.rotation;
        playerCollider.enabled = false;
    }

    public void StandUp()
    {
        isSitting = false;
        print("in Stand Up");
        player.transform.position = seatPosition.position + (Vector3.left * 2);
        player.transform.rotation = seatPosition.rotation;
        playerCollider.enabled = true;
    }
}
