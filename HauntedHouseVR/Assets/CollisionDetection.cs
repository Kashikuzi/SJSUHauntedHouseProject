using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    private void OnTriggerEnter3D(Collider other)
    {
        Debug.Log("hit detected");
    }
}
