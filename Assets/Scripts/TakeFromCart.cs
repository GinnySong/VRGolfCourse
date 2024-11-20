using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeFromCart : MonoBehaviour
{
    public GameObject objectTaken;
    public GameObject cart;


    public void TakeObject()
    {
        objectTaken.transform.position = cart.transform.position + (Vector3.back * 2);
    }
}
