using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform playerTransform;

    public float followRange;  // start follow player after exceed this distance
    public float followSpeed;
    public Vector3 angle;
    public float distance;
    public float scale;

    public bool following = true;

    private void Start()
    {
        Vector3 direction = new Vector3(Mathf.Cos(AngleToRadian(angle.y)), Mathf.Sin(AngleToRadian(angle.x)) + 1f, Mathf.Sin(AngleToRadian(angle.y)));
        transform.rotation = Quaternion.LookRotation(-direction.normalized);
    }

    private void Update()
    {
        distance += Input.mouseScrollDelta.y * scale;
        distance = Mathf.Clamp(distance, 75, 150);
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(Mathf.Cos(AngleToRadian(angle.y)), Mathf.Sin(AngleToRadian(angle.x)) + 1f, Mathf.Sin(AngleToRadian(angle.y)));
        Vector3 dest = playerTransform.position + direction.normalized * distance;

        //if (Vector3.SqrMagnitude(transform.position - dest) > followRange)
        //{
        //    following = true;
        //}

        //if (following)
        //{
            transform.position = Vector3.MoveTowards(transform.position , dest, followSpeed * Time.fixedDeltaTime);

            if (Vector3.SqrMagnitude(dest - transform.position) < 0.5)
            {
                following = false;
            }
        //}
    }

    public float AngleToRadian(float angle)
    {
        return angle / 360 * 2 * Mathf.PI;
    }
}
