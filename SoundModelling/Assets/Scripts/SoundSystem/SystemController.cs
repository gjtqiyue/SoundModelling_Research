using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    public class SystemController : SingletonBase<SystemController>
    {
        [SerializeField]
        List<GameObject> agents = new List<GameObject>();

        [Space]
        public Vector3 startPoint;
        public int width;
        public int length;
        public Vector2 resolution;  // x is width, y is length
        public GameObject quadPrefab;

        private float scaleX;
        private float scaleY;
        private int count;
        private GameObject[,] map;

        public void RegisterAgent(GameObject agent)
        {
            agents.Add(agent);
        }

        public void UnregisterAgent(GameObject agent)
        {
            agents.Remove(agent);
        }

        protected override void Awake()
        {
            base.Awake();
            GenerateHeatMapLayout();
        }
        
        private void GenerateHeatMapLayout()
        {
            scaleX = (float)width / resolution.x;
            scaleY = (float)length / resolution.y;

            map = new GameObject[(int)resolution.x, (int)resolution.y];

            Transform parent = new GameObject("SoundMap").transform;

            for (int i=0; i < resolution.x; i++)
            {
                for (int j=0; j < resolution.y; j++)
                {
                    Vector3 location = startPoint + new Vector3(scaleX * i, 0, scaleY * j);
                    GameObject obj = Instantiate(quadPrefab, location, Quaternion.identity);
                    obj.transform.parent = parent;

                    obj.GetComponent<MeshRenderer>().material.color = Color.white;

                    obj.transform.localScale = new Vector3(scaleX, 1, scaleY);

                    map[i, j] = obj;
                }
            }
        }
    }
}
