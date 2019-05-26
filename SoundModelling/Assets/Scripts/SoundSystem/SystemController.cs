using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoundSystem
{
    public class SystemController : SingletonBase<SystemController>
    {
        [SerializeField]
        List<GameObject> agents = new List<GameObject>();

        [Space]
        [Header("Sound Property")]
        public float stepDistance;      //distance for each time step
        public float fadingSpeed;       //how fast the sound fades away
        public int reflectionLimit;     //how many times it reflect a surface
        public float deflectionRate;      //how much the volume get deducted every time it reflects

        private GameObject[,] map;

        [Space]
        [HideInInspector]
        public Vector3 startPoint;
        [HideInInspector]
        public int width;
        [HideInInspector]
        public int length;
        [HideInInspector]
        public float MapHeight;
        [HideInInspector]
        public Vector2 resolution;  // x is width, y is length
        [HideInInspector]
        public GameObject quadPrefab;

        [HideInInspector]
        public bool drawPointDistribution;

        [HideInInspector]
        public float scaleX;
        [HideInInspector]
        public float scaleY;
        [HideInInspector]
        public int count;
        [HideInInspector]
        public Transform mapParent;

        private Queue<AgentSoundComponent> ReceiveSoundQueue = new Queue<AgentSoundComponent>();

        [HideInInspector]
        public float currentHighestIntensity;

        public void RegisterAgent(GameObject agent)
        {
            agents.Add(agent);
        }

        public void UnregisterAgent(GameObject agent)
        {
            agents.Remove(agent);
        }

        public void RequestForSoundResolve(AgentSoundComponent a)
        {
            if (!ReceiveSoundQueue.Contains(a))
            {
                ReceiveSoundQueue.Enqueue(a);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            currentHighestIntensity = float.Epsilon;
            GenerateHeatMapLayout();
            //Debug.Log(Mathf.RoundToInt(1.34f) + " " + Mathf.RoundToInt(1.64f));
        }

        private void Update()
        {
            Initialize();

            Calculate();
            ResolveSoundCollision();
            Paint();
            ResolveSoundReceive();
        }

        private void Initialize()
        {
            currentHighestIntensity = float.Epsilon;

            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    //clear out the color
                    map[i, j].GetComponent<PointIntensity>().Reset();
                }
            }
        }

        private void Calculate()
        {
            foreach(GameObject agent in agents)
            {
                //two steps:
                //1. map all the sound point onto the map that from the same sound source
                //2. resolve sound points from different sound sources
                agent.GetComponent<AgentSoundComponent>().Calculate();
            }
        }

        private void ResolveSoundCollision()
        {
            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    map[i, j].GetComponent<PointIntensity>().MergeSoundFromDifferentSources();
                }
            }
        }

        private void ResolveSoundReceive()
        {
            while (ReceiveSoundQueue.Count > 0)
            {
                AgentSoundComponent a = ReceiveSoundQueue.Dequeue();
                List<PointIntensity> trace = TrackSoundSource(a.transform.position);
                a.DrawTrackToSoundSource(trace);
            }
        }

        private void Paint()
        {
            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    map[i, j].GetComponent<PointIntensity>().PaintMap();
                }
            }
        }

        private List<PointIntensity> TrackSoundSource(Vector3 pos)
        {
            List<PointIntensity> trace = new List<PointIntensity>();

            float x = pos.x - startPoint.x;
            float z = pos.z - startPoint.z;

            int cellX = Mathf.RoundToInt(x / scaleX);
            int cellY = Mathf.RoundToInt(z / scaleY);

            int curX = cellX;
            int curY = cellY;

            while (true) {
                //hill climbing
                //Debug.Log(trace.Count + ": " + map[curX, curY].transform.position.ToString());
                trace.Add(map[curX, curY].GetComponent<PointIntensity>());

                float maxIntensity = map[curX, curY].GetComponent<PointIntensity>().net_intensity;
                int maxX = curX;
                int maxY = curY;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) { continue; }
                        else if (curX + i > resolution.x || curX + i < 0 || curY + j > resolution.y || curY + j < 0) { continue; }
                        else
                        {
                            if (map[curX + i, curY + j].GetComponent<PointIntensity>().net_intensity > maxIntensity)
                            {
                                maxIntensity = map[curX + i, curY + j].GetComponent<PointIntensity>().net_intensity;
                                maxX = curX + i;
                                maxY = curY + j;
                            }
                        }
                    }
                }

                if (maxIntensity == map[curX, curY].GetComponent<PointIntensity>().net_intensity)
                {
                    //reach max
                    return trace;
                }
                else
                {
                    //keep going
                    curX = maxX;
                    curY = maxY;
                }
            }
        }

        public void MapSoundData(Vector3 pos, float newIntensity, SoundSegment seg, GameObject source)
        {
            float x = pos.x - startPoint.x;
            float z = pos.z - startPoint.z;

            int cellX = Mathf.RoundToInt(x / scaleX);
            int cellY = Mathf.RoundToInt(z / scaleY);
 
            if (cellX < resolution.x && cellY < resolution.y && cellX >= 0 && cellY >= 0)
            {
                //Debug.Log(cellX + ", " + cellY);
                map[cellX, cellY].GetComponent<PointIntensity>().AddNewSoundPoint(newIntensity, seg, source);

                //draw yellow line to indicate the mapping
                if (drawPointDistribution) { Debug.DrawLine(pos, map[cellX, cellY].transform.position, Color.yellow); }
            }
        }

        public void GenerateHeatMapLayout()
        {
            scaleX = width / resolution.x;
            scaleY = length / resolution.y;

            map = new GameObject[(int)resolution.x, (int)resolution.y];

            mapParent = new GameObject("SoundMap").transform;

            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    Vector3 location = startPoint + new Vector3(scaleX * i, MapHeight, scaleY * j);
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(quadPrefab);

                    obj.transform.position = location;
                    obj.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

                    obj.transform.parent = mapParent;

                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

                    var tempMaterial = new Material(renderer.sharedMaterial);

                    tempMaterial.color = Color.white;

                    renderer.sharedMaterial = tempMaterial;

                    obj.transform.localScale = new Vector3(scaleX, scaleY, 1);

                    map[i, j] = obj;
                }
            }
        }
    }


}
