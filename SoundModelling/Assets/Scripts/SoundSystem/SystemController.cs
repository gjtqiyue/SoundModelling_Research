using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace SoundSystem
{
    public class SystemController : SingletonBase<SystemController>
    {
        [SerializeField]
        List<GameObject> agents = new List<GameObject>();

        [Space]
        [Header("Sound Property")]
        public float stepDistance;                              //distance for each time step
        public float fadingSpeed;                               //how fast the sound fades away
        public int rayFrequency;                                //how many rays
        public int reflectionLimit;                             //how many times it reflect a surface
        [Range(0.5f, 1f)] public float diffractionAngleRatio;   //determine the angle of the diffraction of the sound
        public float diffractionRate;
        public float deflectionRate;                            //how much the volume get deducted every time it reflects
        public LayerMask layerOfSoundObstacle;

        public int RayFrequency                                 //control the ray number for all agents
        {
            set
            {
                if (value == rayFrequency) return;
                else
                {
                    rayFrequency = value;
                    foreach (GameObject a in agents)
                    {
                        a.GetComponent<AgentSoundComponent>().rayFrequency = value;
                    }
                }
            }
        }

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
        public float scale;
        [HideInInspector]
        public float unitLength;
        [HideInInspector]
        public GameObject quadPrefab;

        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    RegenerateSoundMap();
                }
            }
        }

        [HideInInspector]
        public bool drawPointDistribution;

        [HideInInspector]
        public Vector2 resolution;
        [HideInInspector]
        public int count;
        [HideInInspector]
        public Transform mapParent;

        private float soundMapTileSize;

        private Queue<AgentSoundComponent> ReceiveSoundQueue = new Queue<AgentSoundComponent>();

        [SerializeField]
        private bool generatingSoundMap;    //true if is regenerating sound map
        [SerializeField]
        private bool simulationON = false;

        [HideInInspector]
        public float currentHighestIntensity;

        private Dictionary<int, int> modifiedMapGrids = new Dictionary<int, int>();

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
            if (!generatingSoundMap && simulationON)
            {
                Initialize();

                Calculate();
                ResolveSoundCollision();
                Paint();
                ResolveSoundReceive();
            }
        }

        private void RegenerateSoundMap()
        {
            generatingSoundMap = true;
            if (mapParent) { Destroy(mapParent.gameObject); }
            modifiedMapGrids.Clear();
            GenerateHeatMapLayout();
        }

        private void Initialize()
        {
            currentHighestIntensity = float.Epsilon;

            if (modifiedMapGrids.Count > 0 && map != null)
            {
                foreach (int key in modifiedMapGrids.Keys)
                {
                    int num = modifiedMapGrids[key];
                    int x = num / (int)resolution.y;
                    int y = num % (int)resolution.y;
                    map[x, y].GetComponent<PointIntensity>().Reset();
                }
            }
            modifiedMapGrids.Clear();
            //for (int i = 0; i < resolution.x; i++)
            //{
            //    for (int j = 0; j < resolution.y; j++)
            //    {
            //        //clear out the color
            //        map[i, j].GetComponent<PointIntensity>().Reset();
            //    }
            //}
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
            foreach (int key in modifiedMapGrids.Keys)
            {
                int num = modifiedMapGrids[key];
                int x = num / (int)resolution.y;
                int y = num % (int)resolution.y;
                map[x, y].GetComponent<PointIntensity>().MergeSoundFromDifferentSources();
            }
            //for (int i = 0; i < resolution.x; i++)
            //{
            //    for (int j = 0; j < resolution.y; j++)
            //    {
            //        map[i, j].GetComponent<PointIntensity>().MergeSoundFromDifferentSources();
            //    }
            //}
        }

        private void ResolveSoundReceive()
        {
            while (ReceiveSoundQueue.Count > 0)
            {
                AgentSoundComponent a = ReceiveSoundQueue.Dequeue();
                List<PointIntensity> trace = TrackSoundSource(a.transform.position, a.agent.radius);
                a.DrawTrackToSoundSource(trace);
            }
        }

        private void Paint()
        {
            foreach (int key in modifiedMapGrids.Keys)
            {
                int num = modifiedMapGrids[key];
                int x = num / (int)resolution.y;
                int y = num % (int)resolution.y;
                map[x, y].GetComponent<PointIntensity>().PaintMap();
            }
            //for (int i = 0; i < resolution.x; i++)
            //{
            //    for (int j = 0; j < resolution.y; j++)
            //    {
            //        map[i, j].GetComponent<PointIntensity>().PaintMap();
            //    }
            //}
        }

        private List<PointIntensity> TrackSoundSource(Vector3 pos, float agentRadius)
        {
            List<PointIntensity> trace = new List<PointIntensity>();

            float x = pos.x - startPoint.x;
            float z = pos.z - startPoint.z;

            int cellX = Mathf.RoundToInt(x / soundMapTileSize);
            int cellY = Mathf.RoundToInt(z / soundMapTileSize);

            int curX = cellX;
            int curY = cellY;

            while (true) {
                //hill climbing
                //Debug.Log(trace.Count + ": " + map[curX, curY].transform.position.ToString());
                PointIntensity pointIntensity = map[curX, curY].GetComponent<PointIntensity>();
                trace.Add(pointIntensity);

                float maxIntensity = pointIntensity.net_intensity;
                int maxX = curX;
                int maxY = curY;

                Vector3 curPos = map[curX, curY].GetComponent<PointIntensity>().pos;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        Vector3 destPos = map[curX + i, curY + j].GetComponent<PointIntensity>().pos;

                        if (i == 0 && j == 0) { continue; }
                        else if (curX + i > resolution.x || curX + i < 0 || curY + j > resolution.y || curY + j < 0) { continue; }
                        else if (Physics.Raycast(curPos, destPos-curPos, (destPos-curPos).magnitude + agentRadius, layerOfSoundObstacle, QueryTriggerInteraction.Ignore)) { continue; } //if hit a wall or obstacle
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

                if (maxIntensity == pointIntensity.net_intensity)
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

            int cellX = Mathf.RoundToInt(x / soundMapTileSize);
            int cellY = Mathf.RoundToInt(z / soundMapTileSize);
 
            if (cellX < resolution.x && cellY < resolution.y && cellX >= 0 && cellY >= 0)
            {
                //Debug.Log(cellX + ", " + cellY);
                map[cellX, cellY].GetComponent<PointIntensity>().AddNewSoundPoint(newIntensity, seg, source);

                //add this point to modified list
                int key = cellX * (int)resolution.y + cellY;
                int value;
                if (!modifiedMapGrids.TryGetValue(key, out value))
                {
                    //test if the value is already exist in the dictionary, if not we add this value as both key and value
                    modifiedMapGrids.Add(key, key);
                }

                //draw yellow line to indicate the mapping
                if (drawPointDistribution) { Debug.DrawLine(pos, map[cellX, cellY].transform.position, Color.yellow); }
            }
        }

        public void GenerateHeatMapLayout()
        {
            generatingSoundMap = true;
            soundMapTileSize = scale * unitLength;

            resolution.x = width / (soundMapTileSize);
            resolution.y = length / (soundMapTileSize);

            map = new GameObject[(int)resolution.x, (int)resolution.y];

            mapParent = new GameObject("SoundMap").transform;

            for (int i = 0; i < resolution.x; i++)
            {
                for (int j = 0; j < resolution.y; j++)
                {
                    Vector3 location = startPoint + new Vector3(soundMapTileSize * i, MapHeight, soundMapTileSize * j);
                    GameObject obj = (GameObject)Instantiate(quadPrefab);

                    obj.transform.position = location;
                    obj.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

                    obj.transform.parent = mapParent;

                    obj.transform.localScale = new Vector3(soundMapTileSize, soundMapTileSize, 1);

                    map[i, j] = obj;
                }
            }

            generatingSoundMap = false;
        }

        public bool IsSimulationOn()
        {
            return simulationON;
        }

        void OnGUI()
        {
            GUILayout.Label("Control");
            if (GUILayout.Button("Start / Stop Sim"))
            {
                simulationON = !simulationON;
            }

            GUILayout.Label("System Property");
            GUILayout.Label("Reflection");
            reflectionLimit = int.Parse(GUILayout.TextField(reflectionLimit.ToString()));
            reflectionLimit = (int)GUILayout.HorizontalSlider(reflectionLimit, 0f, 10f);
            GUILayout.Label("Ray");
            RayFrequency = int.Parse(GUILayout.TextField(rayFrequency.ToString()));
            GUILayout.Label("Scale");
            Scale = float.Parse(GUILayout.TextField(Scale.ToString()));
            Scale = GUILayout.HorizontalSlider(Scale, 0.1f, 10f);
            GUILayout.Label("Fading");
            fadingSpeed = float.Parse(GUILayout.TextField(fadingSpeed.ToString()));
            fadingSpeed = GUILayout.HorizontalSlider(fadingSpeed, 0.1f, 5f);
            GUILayout.Label("Step distance");
            stepDistance = float.Parse(GUILayout.TextField(stepDistance.ToString()));
        }
    }
}
