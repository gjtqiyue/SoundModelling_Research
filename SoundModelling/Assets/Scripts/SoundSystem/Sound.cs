using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class Sound
    {
        public GameObject producer;
        public float volume;

        public Sound (GameObject pro, int vol)
        {
            producer = pro;
            volume = vol;
        }
    }
}
