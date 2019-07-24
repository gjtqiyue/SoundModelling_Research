using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class DiffractionPoint : MonoBehaviour
    {
        public Vector3 edgeVector_1, edgeVector_2;
        
        public float TriggerRadius()
        {
            return gameObject.GetComponent<CapsuleCollider>().radius;
        }
    }
}
