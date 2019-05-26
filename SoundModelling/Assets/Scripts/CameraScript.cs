using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform playerTransform;

    public float followRange;  // start follow player after exceed this distance
    public float followSpeed;
    public float height;

    public bool following = true;

    private void FixedUpdate()
    {
        Vector3 dest = playerTransform.position + new Vector3(0, height, 0);
        if (Vector3.Distance(transform.position, dest) > followRange)
        {
            following = true;
        }

        if (following)
        {
            transform.position = Vector3.MoveTowards(transform.position , dest, followSpeed * Time.deltaTime);

            if (Vector3.Distance(dest, transform.position) < 0.5)
            {
                following = false;
            }
        }
    }
}
