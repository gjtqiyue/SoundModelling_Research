﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundSystem;

[RequireComponent(typeof(AgentSoundComponent))]
public class Player : AgentWithSound
{

    public float speed;

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
                soundCmpt.MakeSound(gameObject, transform.position, movingVolume, SoundType.Walk, soundAngle, movingStepDuration);
                timer = pace;
            }

            timer -= Time.deltaTime;
        }
        
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.position = transform.position + new Vector3(h * speed * Time.fixedDeltaTime, 0, v * speed * Time.fixedDeltaTime);

        if (h == 0 && v == 0)
        {
            isWalking = false;
            timer = pace / 2;
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

    private void OnGUI()
    {
        GUI.Label(new Rect (200, 10, 200, 50), "Volume");
        movingVolume = int.Parse(GUI.TextField(new Rect(200, 30, 100, 20), movingVolume.ToString()));
    }
}
