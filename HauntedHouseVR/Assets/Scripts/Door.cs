using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator anim;
    public Transform player;
    public Transform door;

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(player.position, door.position);
        Debug.Log("hello");
        if (distance <= 25)
        {
            anim.SetBool("Near", true);
            Debug.Log("you see me");
        }
        else
        {
            anim.SetBool("Near", false);
            Debug.Log("bad");

        }
    }
}
