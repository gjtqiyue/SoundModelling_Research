using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundSystem;

[RequireComponent(typeof(AgentSoundComponent))]
public class Player : AgentWithSound
{

    [Header("Moving sound settings")]
    public float pace;  //how long it takes to make one step
    public int movingVolume;
    public float movingStepDuration;
    public float soundAngle;

    [SerializeField]
    bool isWalking;
    [SerializeField]
    float timer;

    AgentSoundComponent soundCmpt;

    // Start is called before the first frame update
    void Start()
    {
        soundCmpt = GetComponent<AgentSoundComponent>();
        if (gameObject.tag == "Player")
        {
            AcquireControl();
        }
    }

    // Update is called once per frame
    void Update()
    {

        //soundCmpt.MakeSound(gameObject, transform.position, movingVolume, SoundType.Walk, soundAngle, movingStepDuration);
        if (isWalking)
        {
            if (timer <= 0.05)
            {
                //make a move sound
                //soundCmpt.MakeSound(gameObject, transform.position, movingVolume, SoundType.Walk, soundAngle, movingStepDuration);
                timer = pace;
            }

            timer -= Time.deltaTime;
        }

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h == 0 && v == 0)
        {
            isWalking = false;
            //timer = 0.051f;
        }
        else
        {
            isWalking = true;
        }
    }

    public override void SearchSoundSource(List<PointIntensity> path)
    {
        throw new System.NotImplementedException();
    }

    private int volume;
    private float duration;

    private void OnGUI()
    {
        GUI.Label(new Rect(200, 10, 200, 50), "Volume");
        volume = int.Parse(GUI.TextField(new Rect(200, 40, 100, 20), volume.ToString()));
        GUI.Label(new Rect(200, 70, 200, 50), "Duration");
        duration = float.Parse(GUI.TextField(new Rect(200, 100, 100, 20), duration.ToString()));

        if (GUI.Button(new Rect(200, 140, 200, 50), "Make Sound"))
        {
            soundCmpt.MakeSound(gameObject, transform.position, volume, SoundType.Walk, 360, duration);
        }
    }
}
