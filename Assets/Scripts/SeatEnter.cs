using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatEnter : MonoBehaviour
{
    public Transform seatPosition;
    public GameObject player;
    public GameObject cart;

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
        isSitting = !isSitting;

        if (isSitting)
        {
            player.transform.position = seatPosition.position;
            player.transform.rotation = seatPosition.rotation;
            playerCollider.enabled = false;
        } else
        {
            playerCollider.enabled = true;
        }
    }
}
