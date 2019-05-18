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

        [Header("SoundAttribute")]
        public int rayFrequency;        //how many rays
        public float stepDistance;      //distance for each time step
        public float fadingSpeed;       //how fast the sound fades away
        public int reflectionLimit;     //how many times it reflect a surface
        public float deflectionRate;      //how much the volume get deducted every time it reflects

        public int volume;

        [SerializeField]
        private List<MapPointData>[] soundRayData;
        [SerializeField]
        private List<MapPointData> mapKeyPoints;  //point that is important
        private Queue<Sound> soundQueue;

        private Ray r;

        private ArrayList blocks;

        private void Start()
        {
            soundQueue = new Queue<Sound>();
            blocks = new ArrayList();
            SystemController.Instance.RegisterAgent(gameObject);
        }

        private void Update()
        {
            Calculate();
        }

        // called when produce a sound
        public void MakeSound(GameObject agent, Vector3 pos, int volume)
        {
            soundQueue.Enqueue(new Sound(agent, pos, volume));
        }

        // called when a sound can reach this object
        public void ReceiveSound(GameObject from, float intensity)
        {
            Debug.Log("Receive sound from " + from.name + " At intensity " + intensity);
        }

        public void Calculate()
        {
            if (soundQueue.Count <= 0) return;

            mapKeyPoints = new List<MapPointData>();

            while (soundQueue.Count > 0)
            {
                Sound sound = soundQueue.Dequeue();

                soundRayData = new List<MapPointData>[rayFrequency];


                float steps = sound.volume / fadingSpeed;
                float spreadDistance = steps * stepDistance;

                mapKeyPoints.Add(new MapPointData(sound.sid, sound.producedPos, sound.volume, fadingSpeed, stepDistance));

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
                    List<MapPointData> list = new List<MapPointData>();

                    //raycast
                    float angle = delta * i;
                    Ray ray = new Ray
                    {
                        origin = sound.producer.transform.position,
                        direction = new Vector3(Mathf.Sin(Utility.AngleToRadian(angle)), 0, Mathf.Cos(Utility.AngleToRadian(angle)))
                    };

                    sound.producer.GetComponent<Collider>().enabled = false;
                    Physics.Raycast(ray, out RaycastHit hit, spreadDistance);
                    sound.producer.GetComponent<Collider>().enabled = true;

                    //Debug.Log("Ray " + i + ": " + ray.origin + " " + angle + " " + ray.direction);

                    //hit check
                    if (hit.transform == null)
                    {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * spreadDistance, Color.red, 2);
                        //if hit nothing
                        //a straight line, we want to record the point along the ray
                        UpdateMapPoint(sound.volume, sound, ray.origin + ray.direction * spreadDistance, steps, list, ray, true);
                    }
                    else
                    {
                        float distance = hit.distance;
                        float nextSteps = distance / stepDistance;

                        float remainVol = volume - nextSteps * fadingSpeed - deflectionRate;

                        CheckHitTarget(hit.transform.gameObject, remainVol);

                        UpdateMapPoint(sound.volume, sound, hit.point, nextSteps, list, ray, false);
                        //sline.SetPosition(1, hit.point);
                        Debug.DrawLine(ray.origin, hit.point, Color.red, 2);
                        RayCastFromPoint(sound, ray, hit, remainVol, 0, list);
                    }

                    soundRayData[i] = list;
                }
            }

            //heatMap.UpdateData(mapKeyPoints);
        }

        private void RayCastFromPoint(Sound sound, Ray last_ray, RaycastHit last_hit, float remainVol, int reflectCount, List<MapPointData> list)
        {
            if (remainVol < 0.1f || reflectCount == reflectionLimit)
            {
                return;
            }

            mapKeyPoints.Add(new MapPointData(sound.sid, last_hit.point, remainVol, fadingSpeed, stepDistance));

            Vector3 reflectVec = Vector3.Reflect(last_ray.direction, last_hit.normal);
            //Debug.Log(reflectVec);
            //calculate the reflection and start volume
            //shoot ray again
            float steps = remainVol / fadingSpeed;
            float spreadDist = steps * stepDistance;

            Ray ray = new Ray()
            {
                origin = last_hit.point,
                direction = reflectVec
            };

            Physics.Raycast(ray, out RaycastHit hit, spreadDist);

            if (hit.transform == null)
            {
                //nothing
                Debug.DrawRay(ray.origin, reflectVec * spreadDist, Color.blue, 2);
                UpdateMapPoint(remainVol, sound, ray.origin + reflectVec * spreadDist, steps, list, ray, true);
            }
            else
            {
                float distance = hit.distance;
                float nextSteps = distance / stepDistance;

                float remain = remainVol - nextSteps * fadingSpeed - deflectionRate;

                CheckHitTarget(hit.transform.gameObject, remainVol);

                //Debug.Log("ray " + ray.origin + " " + hit.point);
                Debug.DrawRay(ray.origin, reflectVec * hit.distance, Color.blue, 2);
                steps = hit.distance / stepDistance;
                UpdateMapPoint(remainVol, sound, hit.point, nextSteps, list, ray, false);
                //keep reflecting
                RayCastFromPoint(sound, ray, hit, remain, ++reflectCount, list);
            }
        }

        private void UpdateMapPoint(float volume, Sound sound, Vector3 destination, float steps, List<MapPointData> list, Ray ray, bool includeDestination)
        {
            for (int k = 0; k < steps; k++)
            {
                float length = k * stepDistance;
                float intensity = (volume - fadingSpeed * k) / volume;
                Vector3 position = ray.origin + ray.direction * length;

                list.Add(new MapPointData(sound.sid, position, volume, fadingSpeed, intensity));
                DrawIndicator(volume, ray, length, intensity);
            }

            float intensityAtDestination = (volume - fadingSpeed * steps) / volume + 0.1f;
            if (includeDestination)
            {
                float length = steps * stepDistance;
                list.Add(new MapPointData(sound.sid, destination, volume, fadingSpeed, intensityAtDestination));
                DrawIndicator(volume, ray, length, intensityAtDestination);
            }
        }

        private Ray DrawIndicator(float volume, Ray ray, float length, float intensity)
        {
            GameObject obj = Instantiate(indicator, ray.origin + ray.direction * length, Quaternion.identity);
            obj.GetComponent<PointIntensity>().intensity = intensity;
            float f = intensity;
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
