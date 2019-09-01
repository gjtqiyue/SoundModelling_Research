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
        private List<string> segIds = new List<string>();
        private Dictionary<SoundType, float> accumulatedIntensity = new Dictionary<SoundType, float>();

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
            accumulatedIntensity.Clear();
            PaintMap();
        }

        public void AddNewSoundPoint(float intensity, SoundSegment seg, string sourceName)
        {
            //filter the sound by type
            //if under the source's name there is no entry, we create one
            //else we add the intensity to the new list

            //unifypointdata averages the intensity from the same source and segment
            if (!sources.ContainsKey(seg._type))
            {
                sources.Add(seg._type, new List<SoundMapPointData>());

                SoundMapPointData data = new SoundMapPointData(seg._type, seg._id, sourceName);
                data.UnifyPointData(intensity);
                sources[seg._type].Add(data);
            }
            else
            {
                //try to find the entry that has the same segment id
                for (int i=0; i<sources[seg._type].Count; i++)
                {
                    if (sources[seg._type][i].segmentId == seg._id && sources[seg._type][i].sourceName == sourceName)
                    {
                        sources[seg._type][i].UnifyPointData(intensity);
                        return;
                    }
                }

                //no match for this id
                SoundMapPointData data = new SoundMapPointData(seg._type, seg._id, sourceName);
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
                    segIds.Add(sources[t][i].sourceName + sources[t][i].segmentId + sources[t][i].Intensity());
                    intensity += sources[t][i].Intensity();
                }

                //Debug.Log("type: " + t + " has intensity of " + intensity);
                accumulatedIntensity[t] = intensity;
                
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
            float highest = SystemController.Instance.currentHighestIntensity;
            float value = Mathf.Clamp((net_intensity) / highest, 0f, 1f);
            //if add if statement, agent will leave a trace of sound intensity, interesting
            if (value > 0)
            {
                meshRenderer.material.SetInt("_AnimationSwitch", SystemController.Instance.animationSwitch);
                meshRenderer.material.SetVector("_Color", new Vector4(value, 0.2f, 1-value, 0.9f));
                meshRenderer.material.SetFloat("_Intensity", Mathf.Clamp(highest - net_intensity, 0, highest));
            }
            else
            {
                meshRenderer.material.SetVector("_Color", new Vector4(1f, 1f, 1f, 0f));
            }
        }

        public float RetrieveSoundIntensityAtType(SoundType s)
        {
            if (accumulatedIntensity != null && accumulatedIntensity.ContainsKey(s))
                return accumulatedIntensity[s];
            else
                return 0;
        }
    }
}
