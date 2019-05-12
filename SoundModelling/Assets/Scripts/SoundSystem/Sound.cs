using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class Sound
    {
        public int sid;
        public GameObject producer;
        public Vector3 producedPos;
        public float volume;

        public Sound (GameObject pro, Vector3 pos, int vol)
        {
            sid = Utility.GetUniqueId();
            producer = pro;
            producedPos = pos;
            volume = vol;
        }
    }
}
