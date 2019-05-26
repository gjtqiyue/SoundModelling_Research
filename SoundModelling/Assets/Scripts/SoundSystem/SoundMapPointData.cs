using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    [System.Serializable]
    public class SoundMapPointData
    {
        float net_intensity;
        float average_intensity;
        SoundType type;
        float pointCount;

        public int segmentId;
        public string sourceName;

        public SoundMapPointData(SoundType t, int identicalId, string name)
        {
            type = t;
            average_intensity = 0;
            net_intensity = 0;
            pointCount = 0;
            segmentId = identicalId;
            sourceName = name;
        }

        public void UnifyPointData(float intensity)
        {
            //calculate average of intensity on this block
            net_intensity += intensity;

            // V(k+1) = (k/(k+1))*V(k)+(1/(k+1))*R(k+1)
            average_intensity = (pointCount / (pointCount + 1)) * average_intensity + (1 / (pointCount + 1)) * intensity;

            pointCount += 1;
        }

        public float Intensity()
        {
            return average_intensity;
        }
    }
}
