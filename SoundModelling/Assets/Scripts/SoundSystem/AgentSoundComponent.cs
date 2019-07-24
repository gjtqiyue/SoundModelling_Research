﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a game component for gameobject
namespace SoundSystem
{
    public class AgentSoundComponent : MonoBehaviour
    {
        public GameObject indicator;
        public Heatmap heatMap;
        public AgentWithSound agent;
        public LayerMask raycastLayer;

        [Header("SoundAttribute")]
        public int rayFrequency;        //how many rays
        public int reflectionLimit;    //reflection limit

        [Space]
        public bool drawIndicator = false;
        public bool drawDebugLine = false;

        [SerializeField]
        private List<MapPointData> mapKeyPoints;  //point that is important
        private Queue<Sound> soundQueue;
        private Queue<SoundSegment> soundSegmentQueue;

        private Ray r;
        private int segmentId;

        private ArrayList blocks;

        SystemController system;
        Collider _collider;

        private void Start()
        {
            soundQueue = new Queue<Sound>();
            soundSegmentQueue = new Queue<SoundSegment>();
            blocks = new ArrayList();
            system = SystemController.Instance;
            _collider = GetComponent<Collider>();
            agent = gameObject.GetComponent<AgentWithSound>();
            SystemController.Instance.RegisterAgent(gameObject);
        }

        // called when produce a sound
        public void MakeSound(GameObject agent, Vector3 pos, float volume, SoundType type, float range, float duration)
        {
            if (system.IsSimulationOn())
                soundQueue.Enqueue(new Sound(agent, pos, volume, type, range, duration));
        }

        // called when a sound can reach this object
        public void ReceiveSound(GameObject from, float intensity)
        {
            //Debug.Log("Receive sound from " + from.name + " At intensity " + intensity);
            //hill climb to find where the sound come from
            //Question: how often should we do it?
            system.RequestForSoundResolve(this);
        }

        public void DrawTrackToSoundSource(List<PointIntensity> track)
        {
            if (gameObject.tag == "Player")
            {
                for (int i = 0; i < track.Count - 1; i++)
                {
                    Debug.DrawLine(track[i].pos, track[i + 1].pos, Color.green, 2);
                }
            }
            else
            {
                for (int i = 0; i < track.Count - 1; i++)
                {
                    Debug.DrawLine(track[i].pos, track[i + 1].pos, Color.black, 2);
                }
            }

            if (agent)
                agent.SearchSoundSource(track);
        }

        public void Calculate()
        {
            if (soundQueue.Count <= 0) return;

            segmentId = 0;
            mapKeyPoints = new List<MapPointData>();

            while (soundQueue.Count > 0)
            {
                Sound sound = soundQueue.Dequeue();
                if (true)
                {
                    float volume = sound.volume; // rayFrequency;

                    float steps = sound.volume / (system.fadingSpeed + float.Epsilon);
                    float spreadDistance = steps * system.stepDistance;

                    //mapKeyPoints.Add(new MapPointData(sound.sid, sound.producedPos, sound.volume, fadingSpeed, stepDistance));

                    //if (blocks.Count > 0)
                    //{
                    //    foreach (GameObject obj in blocks)
                    //    {
                    //        Destroy(obj);
                    //    }
                    //    blocks.Clear();
                    //}

                    Transform target = sound.producer.transform;
                    Vector3 origin = target.position;
                    Vector3 forward = target.forward;

                    //rotate clockwise and counter-clockwise to find the bondary vectors
                    float halfRange = sound.range / 2;
                    Vector3 bound_CCW = UtilityMethod.RotateAroundY(halfRange, forward);
                    Vector3 bound_CW = UtilityMethod.RotateAroundY(-halfRange, forward);
                    Debug.DrawRay(agent.transform.position, bound_CCW * 10, Color.cyan);
                    Debug.DrawRay(agent.transform.position, bound_CW * 10, Color.green);

                    float delta = 360f / rayFrequency;
                    int rayNum = (int)(sound.range / delta);
                    Vector3 dir = bound_CW;
                    for (int i = 0; i <= rayNum; i++)   // <= because we enqueue the segement first then increment the angle, so we need extra 1 time for the last direction
                    {
                        //raycast
                        soundSegmentQueue.Enqueue(new SoundSegment(segmentId, origin, dir, sound.volume, sound.type, 0));
                        dir = UtilityMethod.RotateAroundY(delta, dir);
                    }

                    while (soundSegmentQueue.Count > 0)
                    {
                        SoundSegment segment = soundSegmentQueue.Dequeue();
                        RayCastFromPoint(segment, sound);
                    }
                }
            }
        }

        private void RayCastFromPoint(SoundSegment segment, Sound sound)
        {
            float remainVol = segment._volume;
            if (remainVol < 0.1f || segment._reflectNum > system.reflectionLimit)
            {
                return;
            }

            //mapKeyPoints.Add(new MapPointData(sound.sid, last_hit.point, remainVol, fadingSpeed, stepDistance));

            Ray ray = segment._ray;
            Vector3 direction = ray.direction;

            //calculate the reflection and start volume
            //shoot ray again
            float steps = remainVol / (system.fadingSpeed + float.Epsilon);
            float spreadDist = steps * system.stepDistance;

            _collider.enabled = false;
            Physics.Raycast(ray, out RaycastHit hit, spreadDist, raycastLayer, QueryTriggerInteraction.Collide);
            _collider.enabled = true;

            if (hit.transform == null)
            {
                //nothing
                if (drawDebugLine) { Debug.DrawRay(ray.origin, direction * spreadDist, Color.red); }

                UpdateMapPoint(segment, sound, steps, true);
            }
            else if (hit.transform.gameObject.GetComponent<AgentSoundComponent>())
            {
                if (drawDebugLine) { Debug.DrawRay(ray.origin, direction * spreadDist, Color.red); }

                //calculate the remained volume
                float distance = hit.distance;
                float nextSteps = distance / system.stepDistance;              //the new number of steps based on the current hit point of the ray
                float remain = (remainVol - nextSteps * system.fadingSpeed);   //remained volume at the hit point

                UpdateMapPoint(segment, sound, nextSteps, false);

                //if the hit target is an Agent we notify them
                CheckHitTarget(hit.transform.gameObject, remainVol);
            }
            else if (hit.transform.tag == "DiffractionPoint")
            {
                if (drawDebugLine) { Debug.DrawRay(ray.origin, direction * spreadDist, Color.red); }

                //calculate the remained volume
                float distance = hit.distance;
                float nextSteps = distance / system.stepDistance;                                                           //the new number of steps based on the current hit point of the ray
                float remain = (remainVol - nextSteps * system.fadingSpeed) * SystemController.Instance.diffractionRate;    //remained volume at the hit point

                UpdateMapPoint(segment, sound, nextSteps, false);

                //handle diffraction
                Vector3 edge = Vector3.zero;
                float angle = 0;

                //find signed triangle with direction and two edge vectors
                DiffractionPoint diffracPoint = hit.transform.gameObject.GetComponent<DiffractionPoint>();
                Vector3 dir = ray.direction;
                float dotProduct_1 = Vector3.Dot(dir, diffracPoint.edgeVector_1);
                float dotProduct_2 = Vector3.Dot(dir, diffracPoint.edgeVector_2);

                if (dotProduct_1 > 0 && dotProduct_2 > 0)
                {
                    //hit into the wall, just reflection or do nothing
                    ReflectSoundRay(segment, sound, remainVol, ray, direction, hit);
                }
                else if (dotProduct_1 < 0 && dotProduct_2 < 0)
                {
                    //nothing for now
                }
                else if (dotProduct_1 > dotProduct_2)
                {
                    edge = diffracPoint.edgeVector_1;
                    angle = Vector3.SignedAngle(edge, ray.direction, Vector3.up);
                    Debug.DrawRay(hit.transform.position, edge * 10);
                }
                else if (dotProduct_2 > dotProduct_1)
                {
                    edge = diffracPoint.edgeVector_2;
                    angle = Vector3.SignedAngle(edge, ray.direction, Vector3.up);
                    Debug.DrawRay(hit.transform.position, edge * 10);
                }
                else
                {
                    Debug.Log("impossible happened, both less than 0");
                }

                //after find the correct edge vector, we calculate a diffraction angle in between
                if (edge != Vector3.zero)
                {
                    DiffractSoundRay(diffracPoint.transform.position, ray.direction, edge, angle, segment, remain, diffracPoint.TriggerRadius());
                }
            }
            else
            {
                ReflectSoundRay(segment, sound, remainVol, ray, direction, hit);
            }
        }

        private void DiffractSoundRay(Vector3 diffractionPoint, Vector3 mainDir, Vector3 edgeDir, float angle, SoundSegment segment, float remainVol, float triggerRadius)
        {
            float diffractionAngle = angle * SystemController.Instance.diffractionAngleRatio;
            int numOfRays = Mathf.RoundToInt((float)rayFrequency / 360 * Mathf.Abs(diffractionAngle));
            float delta = diffractionAngle / numOfRays;

            Vector3 dir = mainDir;
            Vector3 origin = new Vector3(diffractionPoint.x, transform.position.y, diffractionPoint.z);
            for (int i = 0; i <= numOfRays; i++)
            {
                //if raycast inside the diffraction trigger, the ray will first hit the trigger from inside, so to prevent that we need to manually set the origin outside the trigger radius
                soundSegmentQueue.Enqueue(new SoundSegment(segment._id, origin + dir * triggerRadius, dir, remainVol, segment._type, segment._reflectNum));
                dir = UtilityMethod.RotateAroundY(delta, dir);
            }
        }

        private void ReflectSoundRay(SoundSegment segment, Sound sound, float remainVol, Ray ray, Vector3 direction, RaycastHit hit)
        {
            Vector3 reflectVec = Vector3.Reflect(ray.direction, hit.normal);

            //calculate remain volume at this point
            float distance = hit.distance;
            float nextSteps = distance / system.stepDistance;

            float remain = (remainVol - nextSteps * system.fadingSpeed) * system.deflectionRate;

            //Debug.Log("ray " + ray.origin + " " + hit.point);
            if (drawDebugLine) { Debug.DrawRay(ray.origin, direction * hit.distance, Color.blue); }

            UpdateMapPoint(segment, sound, nextSteps, false);

            //keep reflecting, treat it as a new sound
            soundSegmentQueue.Enqueue(new SoundSegment(segment._id + 1, hit.point, reflectVec, remain, segment._type, segment._reflectNum + 1));
        }

        //Calculate the line from start to destination and divide into several intensity points based on fading speed and travelling speed
        private void UpdateMapPoint(SoundSegment segment, Sound sound, float steps, bool includeDestination)
        {
            for (int k = 0; k < steps; k++)
            {
                // operation for each in between sound points
                float length = k * system.stepDistance;
                float intensity = (segment._volume - system.fadingSpeed * k);
                Vector3 position = segment._ray.origin + segment._ray.direction * length;

                if (drawIndicator)
                    DrawIndicator(segment._volume, segment._ray, length, intensity);

                //map it to the map in the system controller
                system.MapSoundData(position, intensity, segment, sound.producer);
            }

            float intensityAtDestination = (segment._volume - system.fadingSpeed * steps) + 0.1f;
            if (includeDestination)
            {
                float length = steps * system.stepDistance;
                
                if (drawIndicator)
                    DrawIndicator(segment._volume, segment._ray, length, intensityAtDestination);
            }
        }

        private Ray DrawIndicator(float volume, Ray ray, float length, float intensity)
        {
            GameObject obj = Instantiate(indicator, ray.origin + ray.direction * length, Quaternion.identity);
            obj.GetComponent<SoundPoint>().source = ray.origin;
            obj.GetComponent<SoundPoint>().intensity = intensity;
            float f = intensity / volume;
            obj.GetComponent<MeshRenderer>().material.color = new Color(f, 1 - f, 1 - f, 1);
            blocks.Add(obj);

            return ray;
        }

        private void CheckHitTarget(GameObject obj, float intensity)
        {
            AgentSoundComponent cpnt = obj.GetComponent<AgentSoundComponent>();
            if (cpnt)
            {
                cpnt.ReceiveSound(gameObject, intensity);
            }
        }
    }
}
