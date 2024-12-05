using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingGhost : MonoBehaviour
{
    public bool original = true;
    float time_elapsed = 0;
    // Start is called before the first frame update
    void Start()
    {
        time_elapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!original) {
            time_elapsed += Time.deltaTime;
            if (time_elapsed > 5) {
                Destroy(gameObject);
            }
        }
    }
}
