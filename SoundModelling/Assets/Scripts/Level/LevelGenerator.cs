using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelGenerator : SingletonBase<LevelGenerator>
{
    private static readonly int FREE_CELL = int.MinValue;
    private static readonly int EDGE_CELL = int.MaxValue;
    private static readonly int PATH_CELL = 0;
    private static readonly char INI_PATH_INDEX = 'A';

    public Vector3 origin;
    public Vector2 size;
    public int unitSize;
    public GameObject wallPrefab;
    public GameObject roomWallPrefab;
    public Material source;
    public int numOfExit;
    public int numOfRooms;
    public int roomSizeMin;
    public int roomSizeMax;
    public int numOfMaxRoomEntrance;
    public bool haveLoop;
    public int tryoutTime;

    [Space]
    public float wallMaxHeight;
    public float wallMinHeight;

    private Cell[,] levelMap;
    private Cell[] exits;
    [SerializeField]
    private List<Cell> edgeCells;
    [SerializeField]
    private List<Cell> pathCells;
    [SerializeField]
    private List<Room> rooms;

    private int current_room_id;
    private Transform parent;

    class Cell
    {
        public int _id;    // integer_min = freewall, 0 = corrider, <0 = wall for room, integer_max = edge
        public Vector2Int coordInGrid;
        public Vector3 coord;
        public char pathIndex;  //optional, for the use of labelling different path during the exploration

        public Cell() { }
    };

    class Room
    {
        public int _id;
        public Vector2Int origin;
        public Vector2Int size;
        public int numOfEntrance;

        public Room() { }
        public Room(int id, Vector2Int o, Vector2Int s, int num)
        {
            _id = id;
            origin = o;
            size = s;
            numOfEntrance = num;
        }

    }

    public void GenerateLevel()
    {
        Initialize();

        int time = 0;
        do
        {
            AttempGenerateLevel();
            time++;
        }
        while (rooms.Count < numOfRooms && time < tryoutTime);

        AttempCreatingPath();

        //Construct
        ConstructLevel();
    }

    private void Initialize()
    {
        //check if parent is null, if not then we destroy it before construct new map
        if (parent)
        {
            DestroyImmediate(parent.gameObject);
        }
        parent = new GameObject("Level").transform;

        CheckRoomSize();
    }

    public void AttempGenerateLevel()
    {
        SetUp();

        int time = 0;
        while (rooms.Count < numOfRooms && time < tryoutTime)
        {
            bool rel = AttempCreatingRoom();
            time = rel == true ? 0 : time + 1;
            //Debug.Log(rooms.Count);
        }
    }

    private void SetUp()
    {
        //get a array with labels
        levelMap = new Cell[(int)size.x, (int)size.y];

        //set up the edges
        edgeCells = new List<Cell>(); 

        //set up rooms
        rooms = new List<Room>();

        //reset room id
        current_room_id = 1;

        for (int i = 0; i < (int)size.x; i++)
        {
            for (int j = 0; j < (int)size.y; j++)
            {
                Cell c = new Cell
                {
                    _id = FREE_CELL,
                    coordInGrid = new Vector2Int(i, j),
                    coord = origin + new Vector3(i * unitSize, 0, j * unitSize)
                };

                if (i == 0 || i == (int)size.x - 1 || j == 0 || j == (int)size.y - 1)
                {
                    c._id = EDGE_CELL;
                    edgeCells.Add(c);
                }

                levelMap[i, j] = c;
            }
        }
    }

    private bool AttempCreatingRoom()
    {
        //because we use the very outer tile to build walls, so we have to add 1 to achieve wanted result
        //for example, without +2, 4 by 4 will actually only be 3 * 3;
        int length = Random.Range(roomSizeMin, roomSizeMax);
        int width = Random.Range(roomSizeMin, roomSizeMax);
        int lengthWithWall = length + 2;
        int widthWithWall = width + 2;

        int randX = Random.Range(1, (int)size.x-1);
        int randY = Random.Range(1, (int)size.y-1);

        //test validity
        if (randX + lengthWithWall < (int)size.x && randY + widthWithWall < (int)size.y)
        {
            for (int i = randX; i < randX + lengthWithWall; i++)
            {
                for (int j = randY; j < randY + widthWithWall; j++)
                {
                    if (levelMap[i, j]._id != FREE_CELL)
                    {
                        //room overlapping, start a new one
                        return false;
                    }
                }
            }

            //add a new room
            rooms.Add(new Room
            {
                _id = current_room_id,
                origin = new Vector2Int(randX, randY),
                size = new Vector2Int(length, width),
                numOfEntrance = 0
            });
            //Debug.Log(rooms[rooms.Count-1].origin + " " + rooms[rooms.Count-1].size);

            //reach here = safe room
            int edgeX = randX + lengthWithWall - 1;
            int edgeY = randY + widthWithWall - 1;
            for (int i = randX; i < randX + lengthWithWall; i++)
            {
                for (int j = randY; j < randY + widthWithWall; j++)
                {
                    
                    //add walls to all the edges
                    if (i == randX || i == edgeX || j == randY || j == edgeY)
                    {
                        levelMap[i, j]._id = -current_room_id;
                    }
                    else
                    {
                        levelMap[i, j]._id = current_room_id;
                        //Debug.Log(current_room_id);
                    }
                }
            }
            current_room_id++;

            return true;
        }

        return false;
    }

    private void AttempCreatingPath()
    {
        pathCells = new List<Cell>();

        //first select some exit for the level
        exits = new Cell[numOfExit];

        Queue<Cell> _ExploreQueue = new Queue<Cell>();

        int segemntCount = edgeCells.Count / numOfExit;
        int count = 0, tries = 0;
        //Debug.Log(edgeCells.Count);
        while (count < numOfExit && tries < tryoutTime)
        {
            int idx = Random.Range(segemntCount * count, segemntCount * count + segemntCount - 1);
            Debug.Log(edgeCells[idx]._id);
            if (edgeCells[idx]._id == EDGE_CELL)
            {
                Cell cell = edgeCells[idx];
                cell._id = PATH_CELL;     //change the new exit's cell form edge cell to path cell
                cell.pathIndex = (char)(INI_PATH_INDEX + count);
                exits[count] = cell;
                Debug.Log(exits[count].pathIndex);
                //add to queue
                _ExploreQueue.Enqueue(cell);

                tries = 0;
                count++;
            }
            tries++;
        }

        //then start from each exit, we do a random exploration on the map until they meet each other
        int finishCount = 0;
        int turnCount = 0;
        Cell start;

        while (finishCount != 5000 && _ExploreQueue.Count > 0)
        {
            start = _ExploreQueue.Dequeue();
            Vector2Int pos = start.coordInGrid;

            //random choose a direction
            int dir = Random.Range(0, 5);
            Vector2Int newCellPos = new Vector2Int(0, 0);
            switch (dir)
            {
                case 0: //north
                    newCellPos = new Vector2Int(pos.x, pos.y + 1);
                    break;
                case 1: //south
                    newCellPos = new Vector2Int(pos.x, pos.y - 1);
                    break;
                case 2: //east
                    newCellPos = new Vector2Int(pos.x + 1, pos.y);
                    break;
                case 3: //west
                    newCellPos = new Vector2Int(pos.x - 1, pos.y);
                    break;
            }

            //check this new pos
            //first exclude walls and edges
            Cell nextCell;
            if (CheckIfOutOfBound(newCellPos.x, newCellPos.y))
            {
                nextCell = levelMap[newCellPos.x, newCellPos.y];

                //now disscus the situation where it is a free wall or a room wall
                if (nextCell._id == EDGE_CELL)
                {
                    //go back and choose another route
                    _ExploreQueue.Enqueue(start);
                }
                else if (nextCell._id == FREE_CELL)
                {
                    //take the free cell
                    nextCell._id = PATH_CELL;
                    nextCell.pathIndex = start.pathIndex;
                    _ExploreQueue.Enqueue(nextCell);
                    Debug.Log("add a new path");
                }
                else if (nextCell._id == PATH_CELL)
                {
                    //hit the tile that comes form the same path
                    if (nextCell.pathIndex == start.pathIndex)
                    {
                        if (haveLoop)
                        {
                            //continue if loop is allowed
                            _ExploreQueue.Enqueue(nextCell);
                        }
                        else
                        {
                            Debug.Log("no path allowed, go back");
                            _ExploreQueue.Enqueue(start);
                        }
                    }
                    else 
                    {
                        //hit the tile that from some other path, which means we are done here
                        Debug.Log("connencted, yeah!");
                        finishCount++;
                    }
                }
                else if (nextCell._id < 0)
                {
                    Debug.Log("hit a room wall, choose to connect");
                    //if we hit a room wall
                    //first we check if this room already has a door
                    Room roomToConnect = rooms[-nextCell._id];
                    if (roomToConnect.numOfEntrance == 0)
                    {
                        //create a entrance here
                        nextCell._id = PATH_CELL;
                        nextCell.pathIndex = start.pathIndex;
                        _ExploreQueue.Enqueue(start);
                    }
                    else if (roomToConnect.numOfEntrance < numOfMaxRoomEntrance)
                    {
                        //randomly decide if create a door or not
                        int rand = Random.Range(0, 2);
                        if (rand == 1)
                        {
                            //crate a door
                            nextCell._id = PATH_CELL;
                            nextCell.pathIndex = start.pathIndex;
                            _ExploreQueue.Enqueue(start);
                        }
                        else
                        {
                            _ExploreQueue.Enqueue(start);
                        }
                    }
                    else
                    {
                        //skip this tile
                        _ExploreQueue.Enqueue(start);
                    }
                }

                finishCount++;
            }
            else
            {
                _ExploreQueue.Enqueue(start);
            }

            turnCount++;
        }
    }

    private bool CheckIfOutOfBound(int x, int y)
    {
        return (x >= 0 && x < (int)size.x && y >= 0 && y < (int)size.y);
    }

    private void CheckRoomSize()
    {
        //check room size
        if (roomSizeMax > size.x || roomSizeMax > size.y || roomSizeMax < roomSizeMin || roomSizeMax < 0)
        {
            throw new System.Exception("invalid room max size");
        }
        if (roomSizeMin > size.x || roomSizeMin > size.y || roomSizeMax < roomSizeMin || roomSizeMin < 0)
        {
            throw new System.Exception("invalid room min size");
        }
    }

    private void ConstructLevel()
    {
        for (int i = 0; i < (int)size.x; i++)
        {
            for (int j = 0; j < (int)size.y; j++)
            {
                if (levelMap[i, j]._id == PATH_CELL)
                {
                    Debug.Log("path");
                    SpawnTileObject(i, j, wallPrefab, Color.blue);
                }
                else if (levelMap[i, j]._id == EDGE_CELL)
                {
                    SpawnTileObject(i, j, wallPrefab, Color.black);
                }
                else if (levelMap[i, j]._id == FREE_CELL)
                {
                    //SpawnTileObject(i, j, wallPrefab, Color.gray);
                }
                else if (levelMap[i, j]._id < 0)
                {
                    SpawnTileObject(i, j, roomWallPrefab, Color.red);
                }
                else
                {
                    continue;
                }
            }
        }
    }

    private void SpawnTileObject(int i, int j, GameObject prefab, Color color)
    {
        float height = Random.Range(wallMinHeight, wallMaxHeight);
        float distanceToSink = unitSize / 2 - unitSize + height;
        Vector3 position = new Vector3(levelMap[i, j].coord.x, -distanceToSink, levelMap[i, j].coord.z);
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.parent = parent;
            obj.GetComponent<MeshRenderer>().sharedMaterial = new Material(source);
            obj.GetComponent<MeshRenderer>().sharedMaterial.color = color;
        }
    }
}
