using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem {
    public class SoundPoint : MonoBehaviour
    {
        public Vector3 source = Vector3.zero;
        public float intensity = -1;
        public SoundType type;
    }
}
