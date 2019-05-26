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
        public SoundType type;

        public Sound (GameObject pro, Vector3 pos, float vol, SoundType t)
        {
            sid = Utility.GetUniqueId();
            producer = pro;
            producedPos = pos;
            volume = vol;
            type = t;
        }
    }

    public enum SoundType
    {
        Walk,
        Run,
        Hit
    }
}
