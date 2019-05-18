using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public static class Utility
    {
        static int nextAvailId = -1;

        public static int GetUniqueId()
        {
            nextAvailId++;
            return nextAvailId;
        }

        public static float AngleToRadian(float angle)
        {
            return angle / 360 * 2 * Mathf.PI;
        }
    }
}
