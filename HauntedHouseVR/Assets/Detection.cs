using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter3D(Collider other)
    {
        Debug.Log("hit detected");
    }
}
