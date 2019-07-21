using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class PointIntensity : MonoBehaviour
    {
        public Vector3 pos;
        public float net_intensity;
        public SoundType loudestSound;

        private MeshRenderer meshRenderer;

        private Dictionary<SoundType, List<SoundMapPointData>> sources = new Dictionary<SoundType, List<SoundMapPointData>>();
        [SerializeField]
        private List<float> segIds = new List<float>();

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            net_intensity = 0;
            Reset(); 
        }

        private void Start()
        {
            pos = transform.position;
        }

        public void Reset()
        {
            net_intensity = 0;
            sources.Clear();
            segIds.Clear();
            PaintMap();
        }

        public void AddNewSoundPoint(float intensity, SoundSegment seg, GameObject source)
        {
            //if under the source's name there is no entry, we create one
            //else we add the intensity to the new list
            if (!sources.ContainsKey(seg._type))
            {
                sources.Add(seg._type, new List<SoundMapPointData>());

                SoundMapPointData data = new SoundMapPointData(seg._type, seg._id, source.name);
                data.UnifyPointData(intensity);
                sources[seg._type].Add(data);
            }
            else
            {
                //try to find the entry that has the same segment id
                for (int i=0; i<sources[seg._type].Count; i++)
                {
                    if (sources[seg._type][i].segmentId == seg._id && sources[seg._type][i].sourceName == source.name)
                    {
                        sources[seg._type][i].UnifyPointData(intensity);
                        return;
                    }
                }

                //no match for this id
                SoundMapPointData data = new SoundMapPointData(seg._type, seg._id, source.name);
                data.UnifyPointData(intensity);
                sources[seg._type].Add(data);
            }
        }
        
        //decide how each sound wave merge with others when collision happens
        //naive approach: add the one that has the same type, substract the one that is different
        public void MergeSoundFromDifferentSources()
        {
            foreach (SoundType t in sources.Keys)
            {
                float intensity = 0;
                
                for (int i=0; i<sources[t].Count; i++)
                {
                    segIds.Add(sources[t][i].Intensity());
                    intensity += sources[t][i].Intensity();
                }
                //Debug.Log("type: " + t + " has intensity of " + intensity);
                
                //take the loudest sound type and update its intensity
                if (intensity > net_intensity)
                {
                    net_intensity = intensity;
                    loudestSound = t;
                }
            }

            if (net_intensity > SystemController.Instance.currentHighestIntensity)
            {
                SystemController.Instance.currentHighestIntensity = net_intensity;
            }
        }

        public void PaintMap()
        {
            float value = Mathf.Clamp((net_intensity) / SystemController.Instance.currentHighestIntensity, 0f, 1f);
            meshRenderer.material.SetVector("_Color", new Vector4(value, 0, 0, value));
        }
    }
}
