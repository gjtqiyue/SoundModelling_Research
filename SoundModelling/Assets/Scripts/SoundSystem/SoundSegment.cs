using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class SoundSegment
    {
        public int _id;
        public Ray _ray;
        public RaycastHit _hit;
        public float _volume;
        public SoundType _type;
        public int _reflectNum;

        public SoundSegment(int id, Vector3 ogn, Vector3 dir, float volume, SoundType type, int reflectionIndex)
        {
            _id = id;
            _ray = new Ray
            {
                origin = ogn,
                direction = dir
            };
            _volume = volume;
            _type = type;
            _reflectNum = reflectionIndex;
        }

        public SoundSegment(int id, Vector3 ogn, Vector3 dir, RaycastHit hit, float volume, SoundType type)
        {
            _id = id;
            _ray = new Ray
            {
                origin = ogn,
                direction = dir
            };
            _hit = hit;
            _volume = volume;
            _type = type;
            _reflectNum = 0;
        }
    }
}
