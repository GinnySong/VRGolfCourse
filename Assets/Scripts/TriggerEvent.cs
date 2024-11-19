using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour
{
    public UnityEvent onTrigger;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Ball"))
        {
            if (onTrigger != null)
            {
                onTrigger.Invoke();
            }
        }
    }

    public void ToggleObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(true);
        }
    }
}
