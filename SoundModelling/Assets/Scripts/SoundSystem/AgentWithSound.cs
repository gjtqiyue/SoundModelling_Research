using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public abstract class AgentWithSound : Controllable
    {
        public bool canTrackSound;
        public float radius;

        public abstract void SearchSoundSource(List<PointIntensity> path);
    }
}
