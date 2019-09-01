using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controllable : MonoBehaviour
{
    public float speed = 7;
    public bool isBeingControlled;

    private void Start()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        if (isBeingControlled)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            transform.position = transform.position + new Vector3(h * speed * Time.fixedDeltaTime, 0, v * speed * Time.fixedDeltaTime);
        }
    }

    public virtual bool AcquireControl()
    {
        if (isBeingControlled == false)
        {
            isBeingControlled = !isBeingControlled;
            // this could be changed to suit different game contests
            GameManager.Instance.UpdateControllingTarget(gameObject);
            return true;
        }
        return false;
    }

    public virtual bool ReleaseControl()
    {
        isBeingControlled = false;

        return true;
    }


}
