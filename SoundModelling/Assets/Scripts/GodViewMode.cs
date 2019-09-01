using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodViewMode : Controllable
{
    /***************************************************************
     * God view mode
     ***************************************************************/

    public Camera cam;
    public CameraScript camScript;
    public GameObject spawnableObject;
    public float height;

    public bool ModeOn;

    private void Start()
    {
        camScript = gameObject.GetComponent<CameraScript>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.G))
        {
            ToggleModeOn();
        }

        if (ModeOn)
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
                //Debug.Log(hit.transform.name);
                if (hit.transform != null && hit.transform.tag == "Ground")
                {
                    Instantiate(spawnableObject, hit.point - new Vector3(0, hit.transform.position.y, 0), transform.rotation);
                }
            }

            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
            {
                Debug.Log("holding shift and left mouse button");
                //toggle mode off
                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (hit.transform.GetComponent<Controllable>())
                {
                    if (hit.transform.gameObject.GetComponent<Controllable>().AcquireControl())
                    {
                        ToggleModeOff();
                    }
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void ToggleModeOn()
    {
        ModeOn = true;

        cam.transform.rotation = Quaternion.LookRotation(Vector3.down);
        cam.transform.position = new Vector3(cam.transform.position.x, height, cam.transform.position.z);
        AcquireControl();
    }

    private void ToggleModeOff()
    {
        camScript.enabled = true;
        ModeOn = false;
    }

    public override bool AcquireControl()
    {
        bool res = base.AcquireControl();
        if (res)
        {
            camScript.enabled = false;

            return true;
        }
        return false;
    }

    public override bool ReleaseControl()
    {
        return base.ReleaseControl();
    }

    private void OnGUI()
    {
        if (ModeOn)
        {
            GUI.Label(new Rect(Screen.width - 30, 0, 30, 10), "God view mode");
        }
    }
}
