using System.Collections;
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

        [Header("SoundAttribute")]
        public int rayFrequency;        //how many rays


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

        private void Start()
        {
            soundQueue = new Queue<Sound>();
            soundSegmentQueue = new Queue<SoundSegment>();
            blocks = new ArrayList();
            SystemController.Instance.RegisterAgent(gameObject);
        }

        // called when produce a sound
        public void MakeSound(GameObject agent, Vector3 pos, float volume, SoundType type)
        {
            soundQueue.Enqueue(new Sound(agent, pos, volume, type));
        }

        // called when a sound can reach this object
        public void ReceiveSound(GameObject from, float intensity)
        {
            //Debug.Log("Receive sound from " + from.name + " At intensity " + intensity);
            //hill climb to find where the sound come from
            //Question: how often should we do it?
            SystemController.Instance.RequestForSoundResolve(this);
        }

        public void DrawTrackToSoundSource(List<PointIntensity> track)
        {
            for (int i = 0; i < track.Count - 1; i++)
            {
                Debug.DrawLine(track[i].pos, track[i+1].pos, Color.black, 2);
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

                float volume = sound.volume; // rayFrequency;

                float steps = sound.volume / SystemController.Instance.fadingSpeed;
                float spreadDistance = steps * SystemController.Instance.stepDistance;

                //mapKeyPoints.Add(new MapPointData(sound.sid, sound.producedPos, sound.volume, fadingSpeed, stepDistance));

                if (blocks.Count > 0)
                {
                    foreach (GameObject obj in blocks)
                    {
                        Destroy(obj);
                    }
                    blocks.Clear();
                }

                float delta = 360 / rayFrequency;
                for (int i = 0; i < rayFrequency; i++)
                {
                    //raycast
                    float angle = delta * i;
                    Vector3 origin = sound.producer.transform.position;
                    Vector3 direction = new Vector3(Mathf.Sin(Utility.AngleToRadian(angle)), 0, Mathf.Cos(Utility.AngleToRadian(angle)));
                    soundSegmentQueue.Enqueue(new SoundSegment(segmentId, origin, direction, sound.volume, sound.type));
                }

                while (soundSegmentQueue.Count > 0)
                {
                    SoundSegment segment = soundSegmentQueue.Dequeue();
                    RayCastFromPoint(segment, sound);
                }
            }
        }

        private void RayCastFromPoint(SoundSegment segment, Sound sound)
        {
            float remainVol = segment._volume;
            if (remainVol < 0.1f || segment._reflectNum == SystemController.Instance.reflectionLimit)
            {
                return;
            }
            
            //increment the reflection count
            segment.IncrementReflectionCount();

            //mapKeyPoints.Add(new MapPointData(sound.sid, last_hit.point, remainVol, fadingSpeed, stepDistance));

            Ray ray = segment._ray;
            Vector3 direction = ray.direction;

            //calculate the reflection and start volume
            //shoot ray again
            float steps = remainVol / SystemController.Instance.fadingSpeed;
            float spreadDist = steps * SystemController.Instance.stepDistance;

            gameObject.GetComponent<Collider>().enabled = false;
            Physics.Raycast(ray, out RaycastHit hit, spreadDist);
            gameObject.GetComponent<Collider>().enabled = true;

            if (hit.transform == null)
            {
                //nothing
                if (drawDebugLine) { Debug.DrawRay(ray.origin, direction * spreadDist, Color.red); }

                UpdateMapPoint(segment, sound, steps, true);
            }
            else
            {
                Vector3 reflectVec = Vector3.Reflect(ray.direction, hit.normal);

                //calculate remain volume at this point
                float distance = hit.distance;
                float nextSteps = distance / SystemController.Instance.stepDistance;

                float remain = (remainVol - nextSteps * SystemController.Instance.fadingSpeed) * SystemController.Instance.deflectionRate;

                //if the hit target is an Agent we notify them
                CheckHitTarget(hit.transform.gameObject, remainVol);

                //Debug.Log("ray " + ray.origin + " " + hit.point);
                if (drawDebugLine) { Debug.DrawRay(ray.origin, direction * hit.distance, Color.blue); }

                steps = hit.distance / SystemController.Instance.stepDistance;

                UpdateMapPoint(segment, sound, nextSteps, false);

                //keep reflecting, treat it as a new sound
                soundSegmentQueue.Enqueue(new SoundSegment(segmentId++, hit.point, reflectVec, remain, segment._type));
                //RayCastFromPoint(sound, ray, hit, remain, ++reflectCount);
            }
        }

        //Calculate the line from start to destination and divide into several intensity points based on fading speed and travelling speed
        private void UpdateMapPoint(SoundSegment segment, Sound sound, float steps, bool includeDestination)
        {
            for (int k = 0; k < steps; k++)
            {
                // operation for each in between sound points
                float length = k * SystemController.Instance.stepDistance;
                float intensity = (segment._volume - SystemController.Instance.fadingSpeed * k);
                Vector3 position = segment._ray.origin + segment._ray.direction * length;

                if (drawIndicator)
                    DrawIndicator(segment._volume, segment._ray, length, intensity);

                //map it to the map in the system controller
                SystemController.Instance.MapSoundData(position, intensity, segment, sound.producer);

            }

            float intensityAtDestination = (segment._volume - SystemController.Instance.fadingSpeed * steps) + 0.1f;
            if (includeDestination)
            {
                float length = steps * SystemController.Instance.stepDistance;
                
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
