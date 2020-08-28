using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DungeonGenerator : MonoBehaviour {
    
    RoomGenerator roomGenerator;

    public string seed;
    float newSeed = 0.0f;

    string path; //Default path for dungeon text files.

    public Sprite entrance, enemy, puzzle, locked, key, locked3, locked2, key3, key2, key1, end, blank;
    public GameObject verticalDoor, horizontalDoor;

    public Text pathText; //Shows the connections between rooms in a text box on screen

    //The dungeon
    public Dungeon dungeon
    {get; protected set;}

    // Use this for initialization
    void Start () {
        //Pull required components from this game object
        roomGenerator = this.GetComponent<RoomGenerator>();
        
        GenerateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        newSeed += Time.deltaTime*100f; //Seed generation
        if (Input.GetKeyDown("space"))
        {
            seed = newSeed.ToString();
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("Door")) //Clear the old connections
            {
                Destroy(g);
            }
            GenerateDungeon();
        }
        if (Input.GetKeyDown("s"))
        {
            GenerateDungeonFile(seed);
        }
    }

    void GenerateDungeon()
    {
        dungeon = new Dungeon();

        GameObject rooms = GameObject.Find("Rooms"); //For ease of regenerating when space is pressed
        if (rooms == null)
        {
            rooms = new GameObject();
            rooms.name = "Rooms";
        }
        rooms.transform.parent = this.transform;
        
        dungeon.CriticalPath(seed); //Create the critical path
        dungeon.CriticalPathLayout(seed); //Lay out the critical path
        dungeon.Branch(seed); //Lay out the branch paths
        //Create a game object for each room
        for (int x = 0; x < dungeon.Width; x++)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                Room roomData = dungeon.GetRoom(x, y);

                GameObject roomGO = new GameObject();
                roomGO.AddComponent<SpriteRenderer>();
                roomGO.GetComponent<SpriteRenderer>().sprite = blank;

                //Set name to indicate coordiate and type
                roomGO.name = "Room_(" + x + "," + y + ")_" + roomData.Type.ToString();

                //Sets each tile to be a child of DungeonGenerator
                roomGO.transform.SetParent(GameObject.Find("Rooms").transform, true); 
                roomGO.transform.position = new Vector3(x, y, 0);

                roomData.RegisterRoomTypeChangedCallback((room) => { OnRoomTypeChange(room, roomGO); });

                OnRoomTypeChange(roomData, roomGO);
            }
        }
        //Need to check that rooms are one tile apart before placing a connection sprite.
        //It's possible for them not to be, since the distance between a branch end and a new branch can be anything.
        for (int i = 0; i < dungeon.connectionCoordinates.Count; i++)
        {
            if (i < dungeon.connectionCoordinates.Count - 1)
            {
                //Check pairs of coordinates from the list
                int[] coord1 = dungeon.connectionCoordinates[i];
                int[] coord2 = dungeon.connectionCoordinates[i + 1];
                int xDif = coord2[0] - coord1[0];
                int yDif = coord2[1] - coord1[1];
                //If the distance between rooms is 1 unit then they are clsoe enough to create a door
                if (xDif == 1 && yDif == 0 || xDif == -1 && yDif == 0)
                {
                    float x = coord1[0] + (xDif / 2f);
                    float y = coord1[1];
                    Vector3 doorPlacement = new Vector3(x, y, 0);
                    Instantiate(horizontalDoor, doorPlacement, Quaternion.identity);
                }
                if (yDif == 1 && xDif == 0 || yDif == -1 && xDif == 0)
                {
                    float x = coord1[0];
                    float y = coord1[1] + (yDif / 2f);
                    Vector3 doorPlacement = new Vector3(x, y, 0);
                    Instantiate(verticalDoor, doorPlacement, Quaternion.identity);
                }
            }
        }
        //Create an extra connection, has a chance to connect end of branches to other parts of the dungeon.
        for (int i = 0; i < dungeon.extraConnectionCoords.Count; i+=2)
        {
            int[] coord1 = dungeon.extraConnectionCoords[i];
            int[] coord2 = dungeon.extraConnectionCoords[i + 1];
            int xDif = coord2[0] - coord1[0];
            int yDif = coord2[1] - coord1[1];
            if (xDif == 1 || xDif == -1 && yDif == 0)
            {
                float x = coord1[0] + (xDif / 2f);
                float y = coord1[1];
                Vector3 doorPlacement = new Vector3(x, y, 0);
                Instantiate(horizontalDoor, doorPlacement, Quaternion.identity);
            }
            if (yDif == 1 || yDif == -1 && xDif == 0)
            {
                float x = coord1[0];
                float y = coord1[1] + (yDif / 2f);
                Vector3 doorPlacement = new Vector3(x, y, 0);
                Instantiate(verticalDoor, doorPlacement, Quaternion.identity);
            }
        }
        pathText.text = dungeon.connections;
    }

    void GenerateDungeonFile(string seed)
    {
        List<Room> dungeonRooms = dungeon.GetDungeonData(); //Save it for use
        string filename = seed;
        path = "Assets/Resources/" + filename + ".txt"; //Create a unique path based on the dungeon seed.
        
        StreamWriter writer = new StreamWriter(path, true); //Get ready to create a file
        /*
        Entrance is technically located at (10,10) but we will assume it is at (0,0) and change all coordinates to be
        relative to it
        */
        foreach(Room r in dungeonRooms) //Loop through all the rooms
        {
            switch(r.Type)
            {
                case RoomType.Entrance:
                    writer.WriteLine("Entrance located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.End:
                    writer.WriteLine("Final Room located at (" + (r.X-10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Key:
                    writer.WriteLine("Key Room located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Lock:
                    writer.WriteLine("Locked Room located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Puzzle:
                    writer.WriteLine("Puzzle Room located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Enemy:
                    writer.WriteLine("Enemy Room located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Lock2:
                    writer.WriteLine("Double Locked Room located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Key2:
                    writer.WriteLine("Second Key located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Key1:
                    writer.WriteLine("First Key located at(" + (r.X - 10).ToString() + ", " + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Lock3:
                    writer.WriteLine("Triple Locked Room located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
                case RoomType.Key3:
                    writer.WriteLine("Third Key located at (" + (r.X - 10).ToString() + "," + (r.Y - 10).ToString() + ")");
                    break;
            }
            List<int>room = roomGenerator.GenerateRoom(seed);
            writer.WriteLine("Layout Number: " + room[0].ToString());
            writer.WriteLine("Enemy types. Small: " + room[1].ToString() + ", Large: " + room[2].ToString());
            writer.WriteLine("Hazard type: " + room[3].ToString());
            writer.WriteLine("Optional hazard type: " + room[4].ToString());
            writer.WriteLine("Optional enemy type: " + room[5].ToString());
            writer.WriteLine("Optional wall existence: " + room[6].ToString());
            writer.WriteLine("#### Room Complete ####");
        }
        writer.Close();
        Debug.Log("Room saved.");
    }

    void OnRoomTypeChange(Room roomData, GameObject roomGO)
    {
        switch(roomData.Type)
        {
            case RoomType.Entrance:
                roomGO.GetComponent<SpriteRenderer>().sprite = entrance;
                break;
            case RoomType.Enemy:
                roomGO.GetComponent<SpriteRenderer>().sprite = enemy;
                break;
            case RoomType.Puzzle:
                roomGO.GetComponent<SpriteRenderer>().sprite = puzzle;
                break;
            case RoomType.Lock:
                roomGO.GetComponent<SpriteRenderer>().sprite = locked;
                break;
            case RoomType.Lock3:
                roomGO.GetComponent<SpriteRenderer>().sprite = locked3;
                break;
            case RoomType.Lock2:
                roomGO.GetComponent<SpriteRenderer>().sprite = locked2;
                break;
            case RoomType.Key3:
                roomGO.GetComponent<SpriteRenderer>().sprite = key3;
                break;
            case RoomType.Key2:
                roomGO.GetComponent<SpriteRenderer>().sprite = key2;
                break;
            case RoomType.Key1:
                roomGO.GetComponent<SpriteRenderer>().sprite = key1;
                break;
            case RoomType.Key:
                roomGO.GetComponent<SpriteRenderer>().sprite = key;
                break;
            case RoomType.End:
                roomGO.GetComponent<SpriteRenderer>().sprite = end;
                break;
        }
    }
}
