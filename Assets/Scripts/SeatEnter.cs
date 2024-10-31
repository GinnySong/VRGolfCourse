using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatEnter : MonoBehaviour
{
    public Transform seatPosition;
    public GameObject player;
    public GameObject cart;

    private bool isSitting;
    private Vector3 positionOffset = new Vector3(0, 1, 0);
    private Vector3 rotationOffset = new Vector3(-70, 0, 0);

    void Start()
    {
        isSitting = false;
    }

    void Update()
    {
        if (isSitting)
        {
            player.transform.position = seatPosition.position + positionOffset;
            player.transform.rotation = seatPosition.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    public void SitDown()
    {
        isSitting = true;
        player.transform.position = seatPosition.position + positionOffset;
        player.transform.rotation = seatPosition.rotation * Quaternion.Euler(rotationOffset);
    }
}
