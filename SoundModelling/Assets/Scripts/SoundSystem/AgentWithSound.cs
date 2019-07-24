using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public abstract class AgentWithSound : MonoBehaviour
    {
        public float radius;

        public abstract void SearchSoundSource(List<PointIntensity> path);
    }
}
