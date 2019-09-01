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
        public float range;             //angle represents the openning angle of this sound

        private float duration;
        private float timeStart;

        public Sound (GameObject pro, Vector3 pos, float vol, SoundType t, float r, float dur)
        {
            sid = Utility.GetUniqueId();
            producer = pro;
            producedPos = pos;
            volume = vol;
            type = t;
            range = r;
            duration = dur;
            timeStart = Time.time;
        }

        public bool IsOver()
        {
            return Time.time >= timeStart + duration;
        }
    }

    public enum SoundType
    {
        Walk,
        Run,
        Hit,
        Talk
    }
}
