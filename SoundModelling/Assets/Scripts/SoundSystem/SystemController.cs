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
        List<AgentSoundComponent> agents = new List<AgentSoundComponent>();

        [Space]
        [Header("Sound Property")]
        public float stepDistance;                              //distance for each time step
        public float fadingSpeed;                               //how fast the sound fades away
        public int rayFrequency;                                //how many rays
        public int reflectionLimit;                             //how many times it reflect a surface
        [Range(0.5f, 1f)] public float diffractionAngleRatio;   //determine the angle of the diffraction of the sound
        public float diffractionRate;
        public float absorbtionRate;                            //how much the volume get deducted every time it reflects
        public LayerMask layerOfSoundObstacle;
        [Space]
        public int animationSwitch = 0;
        public bool paint = true;

        public int RayFrequency                                 //control the ray number for all agents
        {
            set
            {
                if (value == rayFrequency) return;
                else
                {
                    rayFrequency = value;
                    foreach (AgentSoundComponent a in agents)
                    {
                        a.rayFrequency = value;
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

        private int executionBreakTime = 1;

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

        private Queue<AgentSoundComponent> ReceivedSoundQueue = new Queue<AgentSoundComponent>();
        private Queue<SoundType> ReceivedSoundType = new Queue<SoundType>();

        [SerializeField]
        private bool generatingSoundMap;    //true if is regenerating sound map
        [SerializeField]
        private bool simulationON = false;

        [HideInInspector]
        public float currentHighestIntensity;

        private Dictionary<int, int> modifiedMapGrids = new Dictionary<int, int>();

        public void RegisterAgent(AgentSoundComponent agent)
        {
            agents.Add(agent);
        }

        public void UnregisterAgent(AgentSoundComponent agent)
        {
            agents.Remove(agent);
        }

        public void RequestForSoundResolve(AgentSoundComponent a, SoundType t)
        {
            if (!ReceivedSoundQueue.Contains(a))
            {
                ReceivedSoundQueue.Enqueue(a);
                ReceivedSoundType.Enqueue(t);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            currentHighestIntensity = float.Epsilon;
            GenerateHeatMapLayout();
            //Debug.Log(Mathf.RoundToInt(1.34f) + " " + Mathf.RoundToInt(1.64f));
        }

        private void Start()
        {
            //StartCoroutine(SoundCalculationUpdate());
        }

        //private IEnumerator SoundCalculationUpdate()
        //{
        //    while (true)
        //    {
        //        if (!generatingSoundMap && simulationON)
        //        {
        //            CheckInput();   //animation switch
        //            Initialize();

        //            Calculate();
        //            ResolveSoundCollision();
        //            Paint();
        //            ResolveSoundReceive();
        //            yield return new WaitForSeconds((float)executionBreakTime / 10);
        //        }
        //        yield return new WaitForEndOfFrame();
        //    }
        //}
        private float lastTime;

        private void Update()
        {
            if (!generatingSoundMap && simulationON)
            {
                if (Time.time - lastTime >= (float)executionBreakTime / 10)
                {
                    CheckInput();   //animation switch
                    Initialize();

                    Calculate();
                    ResolveSoundCollision();
                    Paint();
                    ResolveSoundReceive();

                    lastTime = Time.time;
                }
            }
        }

        private void RegenerateSoundMap()
        {
            generatingSoundMap = true;
            if (mapParent) { Destroy(mapParent.gameObject); }
            modifiedMapGrids.Clear();
            GenerateHeatMapLayout();
        }

        private void CheckInput()
        {
            if (Input.GetKey(KeyCode.R))
            {
                animationSwitch = 1;
            }
            else
            {
                animationSwitch = 0;
            }
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
            foreach(AgentSoundComponent agent in agents)
            {
                //two steps:
                //1. map all the sound point onto the map that from the same sound source
                //2. resolve sound points from different sound sources
                agent.Calculate();
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
            while (ReceivedSoundQueue.Count > 0)
            {
                AgentSoundComponent a = ReceivedSoundQueue.Dequeue();
                SoundType t = ReceivedSoundType.Dequeue();
                List<PointIntensity> trace = TrackSoundSource(a.transform.position, a.agent.radius, t);
                a.DrawTrackToSoundSource(trace);
            }
            if (ReceivedSoundType.Count > 0) Debug.Log("received sound type queue is not cleared");
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

        private List<PointIntensity> TrackSoundSource(Vector3 pos, float agentRadius, SoundType s)
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

                float maxIntensity = pointIntensity.RetrieveSoundIntensityAtType(s);
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
                            float curIntensity = map[curX + i, curY + j].GetComponent<PointIntensity>().RetrieveSoundIntensityAtType(s);
                            if (curIntensity > maxIntensity)
                            {
                                maxIntensity = curIntensity;
                                maxX = curX + i;
                                maxY = curY + j;
                            }
                        }
                    }
                }

                if (maxIntensity == pointIntensity.RetrieveSoundIntensityAtType(s))
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

        public void MapSoundData(List<Vector3> positions, List<float> newIntensities, SoundSegment seg, GameObject source)
        {
            count = positions.Count;
            for (int i = 0; i < count; i++)
            {
                float x = positions[i].x - startPoint.x;
                float z = positions[i].z - startPoint.z;

                int cellX = Mathf.RoundToInt(x / soundMapTileSize);
                int cellY = Mathf.RoundToInt(z / soundMapTileSize);

                if (cellX < resolution.x && cellY < resolution.y && cellX >= 0 && cellY >= 0)
                {
                    //Debug.Log(cellX + ", " + cellY);
                    map[cellX, cellY].GetComponent<PointIntensity>().AddNewSoundPoint(newIntensities[i], seg, source.name);

                    //add this point to modified list
                    int key = cellX * (int)resolution.y + cellY;
                    int value;
                    if (!modifiedMapGrids.TryGetValue(key, out value))
                    {
                        //test if the value is already exist in the dictionary, if not we add this value as both key and value
                        modifiedMapGrids.Add(key, key);
                    }

                    //draw yellow line to indicate the mapping
                    if (drawPointDistribution) { Debug.DrawLine(positions[i], map[cellX, cellY].transform.position, Color.yellow); }
                }
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
            GUILayout.Label("ExecutionRate");
            executionBreakTime = int.Parse(GUILayout.TextField(executionBreakTime.ToString()));
        }
    }
}
