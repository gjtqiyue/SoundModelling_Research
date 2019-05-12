using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class Controller : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> agents = new List<GameObject>();

        public GameObject indicator;
        public Heatmap heatMap;

        [Header("SoundAttribute")]
        public int rayFrequency;        //how many rays
        public float stepDistance;      //distance for each time step
        public float fadingSpeed;       //how fast the sound fades away
        public int reflectionLimit;     //how many times it reflect a surface
        public float deflectionRate;      //how much the volume get deducted every time it reflects

        [Header("HeatMapAttribute")]
        public float heatMapRadius;

        public int volume;

        [SerializeField]
        private List<MapPointData>[] soundRayData;
        [SerializeField]
        private List<MapPointData> mapKeyPoints;  //point that is important
        private Queue<Sound> soundQueue;

        private Ray r;
        private LineRenderer sline;

        private ArrayList blocks;

        private void Start()
        {
            heatMapRadius = fadingSpeed;
            sline = GetComponent<LineRenderer>();
            //sline.enabled = true;
            soundQueue = new Queue<Sound>();
            //soundQueue.Enqueue(new Sound(agents[0], volume));
            blocks = new ArrayList();
            //soundQueue.Enqueue(new Sound(agents[0], volume));
            
        }

        private void Update()
        {
            soundQueue.Enqueue(new Sound(agents[0], agents[0].transform.position, volume));
            soundQueue.Enqueue(new Sound(agents[1], agents[1].transform.position, volume));
            Calculate();
            
        }

        public void Calculate()
        {
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
                        direction = new Vector3(Mathf.Sin(AngleToRadian(angle)), 0, Mathf.Cos(AngleToRadian(angle)))
                    };

                    sound.producer.GetComponent<Collider>().enabled = false;
                    Physics.Raycast(ray, out RaycastHit hit, spreadDistance);
                    sound.producer.GetComponent<Collider>().enabled = true;

                    //Debug.Log("Ray " + i + ": " + ray.origin + " " + angle + " " + ray.direction);

                    //hit check
                    if (hit.transform == null)
                    {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * spreadDistance, Color.red);
                        //if hit nothing
                        //a straight line, we want to record the point along the ray
                        UpdateMapPoint(sound.volume, sound, ray.origin + ray.direction * spreadDistance, steps, list, ray, true);
                    }
                    else
                    {
                        float nextSteps = hit.distance / stepDistance;
                        UpdateMapPoint(sound.volume, sound, hit.point, nextSteps, list, ray, false);
                        //sline.SetPosition(1, hit.point);
                        Debug.DrawLine(ray.origin, hit.point, Color.red);
                        RayCastFromPoint(sound, ray, hit, sound.volume, 0, list);
                    }

                    soundRayData[i] = list;
                }
            }

            heatMap.UpdateData(mapKeyPoints);

            //TODO: remove the points related to this sound when finish
            //RemovePointsOfSound(sound.sid);
        }

        private void RemovePointsOfSound(int id)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < mapKeyPoints.Count; i++)
            {
                if (mapKeyPoints[i].pid == id)
                {
                    indices.Add(i);
                }
            }

            foreach (int index in indices)
            {
                mapKeyPoints.RemoveAt(index);
            }
        }

        private void RayCastFromPoint(Sound sound, Ray last_ray, RaycastHit last_hit, float volume, int reflectCount, List<MapPointData> list)
        {
            float distance = last_hit.distance;
            float steps = distance / stepDistance;

            float remainVol = volume - steps * fadingSpeed - deflectionRate;

            if (remainVol < 0.1f || reflectCount == reflectionLimit)
            {
                return;
            }
            
            mapKeyPoints.Add(new MapPointData(sound.sid, last_hit.point, remainVol, fadingSpeed, stepDistance));

            Vector3 reflectVec = Vector3.Reflect(last_ray.direction, last_hit.normal);
            //Debug.Log(reflectVec);
            //calculate the reflection and start volume
            //shoot ray again
            steps = remainVol / fadingSpeed;
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
                Debug.DrawRay(ray.origin, reflectVec * spreadDist, Color.blue);
                UpdateMapPoint(remainVol, sound, ray.origin + reflectVec * spreadDist, steps, list, ray, true);
            }
            else
            {
                //Debug.Log("ray " + ray.origin + " " + hit.point);
                Debug.DrawRay(ray.origin, reflectVec * hit.distance, Color.blue);
                steps = hit.distance / stepDistance;
                UpdateMapPoint(remainVol, sound, hit.point, steps, list, ray, false);
                //keep reflecting
                RayCastFromPoint(sound, ray, hit, remainVol, ++reflectCount, list);
            }
        }

        private void UpdateMapPoint(float volume, Sound sound, Vector3 destination, float steps, List<MapPointData> list, Ray ray, bool includeDestination)
        {
            for (int k = 0; k < steps; k++)
            {
                float length = k * stepDistance;
                float intensity = (volume - fadingSpeed * k) / volume;
                float radius = heatMapRadius;
                Vector3 position = ray.origin + ray.direction * length;

                list.Add(new MapPointData(sound.sid, position, volume, radius, intensity));
                //DrawIndicator(volume, ray, length, intensity);
            }

            float intensityAtDestination = (volume - fadingSpeed * steps) / volume + 0.1f;
            if (includeDestination)
            {
                float length = steps * stepDistance;
                list.Add(new MapPointData(sound.sid, destination, volume, heatMapRadius, intensityAtDestination));
                //DrawIndicator(volume, ray, length, intensityAtDestination);
            }
        }

        private Ray DrawIndicator(float volume, Ray ray, float length, float intensity)
        {
            GameObject obj = Instantiate(indicator, ray.origin + ray.direction * length, Quaternion.identity);
            obj.GetComponent<PointIntensity>().intensity = intensity;
            float f = intensity / volume;
            obj.GetComponent<MeshRenderer>().material.color = new Color(f, 1 - f, 1 - f, 1);
            blocks.Add(obj);
            return ray;
        }

        private static float AngleToRadian(float angle)
        {
            return angle / 360 * 2 * Mathf.PI;
        }

        private void OnDrawGizmos()
        {
            //Gizmos.color = Color.red;
            //Gizmos.DrawRay(r);
        }
    }
}
