using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeFromCart : MonoBehaviour
{
    public GameObject takenObject;
    public GameObject source;

    public void TakeObject()
    {
        takenObject.transform.position = source.transform.position + (Vector3.back * 2);
    }
}
