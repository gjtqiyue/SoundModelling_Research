using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundSystem;

public class Player : AgentWithSound
{
    public float speed;

    [Header("Moving sound settings")]
    public float pace;  //how long it takes to make one step
    public int movingVolume;

    [SerializeField]
    bool isWalking;
    [SerializeField]
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<AgentSoundComponent>().MakeSound(gameObject, transform.position, movingVolume, SoundType.Walk);
        if (isWalking)
        {
            if (timer <= 0.05)
            {
                //make a move sound
                
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
}
