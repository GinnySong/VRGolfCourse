using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour
{
    public UnityEvent onTrigger;
    public GameObject messageText;

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

    public void DisplayMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);  // Ensure the text is visible
        }
    }
}
