using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{

    Room[,] rooms; //The rooms that make up the dungeon.
    int width; //Dungeon width
    public int Width //Return dungeon width
    { get { return width; } }
    int height; //Dungeon height
    public int Height //Return dungeon height
    { get { return height; } }
    
    System.Random seeded; //Converts the seed into a usable format
    List<string> dungeon = new List<string>(); //Contains the dungeon grammar
    List<string> keys; //Contains all the keys required to solve the critical path
    List<Room> dungeonData = new List<Room>(); //Contains all rooms that have been placed

    public string connections = "Entrance: "; //Contains all the connections between rooms
    public List<int[]> connectionCoordinates = new List<int[]>(); //Contains pairs of coordinates for placement of sprites
    public List<int[]> extraConnectionCoords = new List<int[]>(); //An extra connection used at the end of branches

    //Dungeon constructor
    public Dungeon()
    {
        this.width = 20;
        this.height = 20;

        rooms = new Room[width, height]; //Create the array of rooms

        //Create a default room in every space of the array
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SetRoom(x, y, RoomType.Empty);
            }
        }
    }
    //Get a Room at a specific coordinate, if it doesn't exist create it
    public Room GetRoom(int x, int y)
    {
        //Error check to catch tile being out of range
        if (x >= width || x < 0 || y >= height || y < 0)
        {
            Debug.LogError("Room (" + x + ", " + y + ") is out of range.");
            return null;
        }
        if (rooms[x, y] == null)
        {
            rooms[x, y] = new Room(this, x, y, RoomType.Empty);
        }
        return rooms[x, y];
    }

    //Set a Room at a specific coordinate
    public void SetRoom(int x, int y, RoomType type)
    {
        //Error check to catch tile being out of range
        if (x >= width || x < 0 || y >= height || y < 0)
        {
            Debug.LogError("Room (" + x + ", " + y + ") is out of range.");
        }
        rooms[x, y] = new Room(this, x, y, type);
    }

    //Generate the critical path
    public void CriticalPath(string seed)
    {
        //Begining of the grammar, S stands for segment to be replaced.
        dungeon = new List<string>() { "Entrance,", "S", "End" };

        //Replace the "S" with random rooms
        seeded = new System.Random(seed.GetHashCode()); //Convert seed into usable integer
        int rand = 0;

        while (dungeon.Contains("S")) //Keep creating the path out of various combinations until all "S"s have been removed
        {
            rand = seeded.Next(1, 7);
            int index = dungeon.IndexOf("S");
            switch (rand)
            {
                case 1:
                    dungeon[index] = "Puzzle,";
                    dungeon.Insert(index, "Lock,");
                    dungeon.Insert(index, "S");
                    break;
                case 2:
                    dungeon[index] = "Enemy,";
                    if (!dungeon.Contains("Lock(2),"))
                    {
                        dungeon.Insert(index, "Lock(2),");
                    }
                    dungeon.Insert(index, "S");
                    break;
                case 3:
                    dungeon[index] = "Puzzle,";
                    if (!dungeon.Contains("Lock(3),"))
                    {
                        dungeon.Insert(index, "Lock(3),");
                    }
                    dungeon.Insert(index, "Enemy,");
                    break;
                case 4:
                    dungeon[index] = "Puzzle,";
                    break;
                case 5:
                    dungeon[index] = "Enemy,";
                    break;
                case 6:
                    dungeon[index] = "Lock,";
                    dungeon.Insert(index, "Enemy,");
                    break;
            }
        }
    }

    public void CriticalPathLayout(string seed)
    {
        //Turn the seed into a useful integer
        seeded = new System.Random(seed.GetHashCode());
        //Set starting point
        int x = Width / 2;
        int y = Height / 2;
        CreateRoom("Entrance,", x, y); //Place entrance in the middle
        dungeonData.Add(GetRoom(10, 10));
        dungeon.Remove("Entrance,"); //Remove the entrance from the list to stop it being created twice
        //For each room on the critical path
        foreach (string r in dungeon)
        {
            //Pick a direction
            int[] coords = FindSpace(x, y);
            CreateRoom(r, coords[0], coords[1]); //Make a room in the chosen direction
            dungeonData.Add(GetRoom(coords[0], coords[1]));//Add the room to the list for use later in creating branches
            x = coords[0];
            y = coords[1];
            connections += " to {" + x.ToString() + ", " + y.ToString() + "}"; //Store the connection between rooms
        }
    }

    public void Branch(string seed)
    {
        //Need to know how many keys are required by the critical path.
        keys = new List<string>();
        foreach (string r in dungeon) //Add appropriate keys based on the list of rooms in the dungeon.
        {
            switch (r)
            {
                case "Lock,":
                    keys.Add("Key,");
                    break;
                case "Lock(2),":
                    keys.Add("Key 1,");
                    keys.Add("Key 2,");
                    break;
                case "Lock(3),":
                    keys.Add("Key 1,");
                    keys.Add("Key 2,");
                    keys.Add("Key 3,");
                    break;
            }
        }
        while (keys.Count > 0) //While there are still keys to place
        {
            Room start = dungeonData[0]; //Want to pick a random room that is the first when possible.
            int x = start.X; //Grab the coordinates from the branch's starting point
            int y = start.Y;
            connections += " to {" + x.ToString() + ", " + y.ToString() + "}"; //Add connections made to the list
            //For each room in the branch
            foreach (string r in BranchGrammar(keys[0]))
            {
                //Pick a direction
                int[] coords = FindSpace(x, y);
                CreateRoom(r, coords[0], coords[1]);
                dungeonData.Add(GetRoom(coords[0], coords[1]));//Add the room to the list for use later in creating branches
                x = coords[0];
                y = coords[1];
                connections += " to {" + x.ToString() + ", " + y.ToString() + "}"; //Add connections made to the list
            }
            //Find an adjacent room for an additional connection.
            int[] adjacentRoom = FindAdjacent(x, y, dungeonData[dungeonData.Count - 2].X, dungeonData[dungeonData.Count - 2].Y);
            connections += " to {" + adjacentRoom[0].ToString() + ", " + adjacentRoom[1].ToString() + "}";
            keys.RemoveAt(0); //remove the key that generates this branch from the list
        }

    }

    List<string> BranchGrammar(string key)
    {
        //Branch grammar adds random rooms until all "S"s are removes
        List<string> branch = new List<string>() {"S", key }; 
        while (branch.Contains("S"))
        {
            int random = seeded.Next(1, 8);
            int index = branch.IndexOf("S");
            switch (random)
            {
                case 1:
                    branch.Remove("S");
                    break;
                case 2:
                    branch[index] = "Puzzle,";
                    branch.Insert(index, "S");
                    break;
                case 3:
                    branch[index] = "Enemy,";
                    branch.Insert(index, "S");
                    break;
                case 4:
                    branch[index] = "Puzzle,";
                    break;
                case 5:
                    branch[index] = "Enemy,";
                    break;
                case 6:
                    branch[index] = "S";
                    branch.Insert(index, "Lock,");
                    keys.Add("Key,");
                    break;
                case 7: //Only create a double locked door if one does not already exist
                    if (!dungeon.Contains("Lock(2),"))
                    {
                        branch[index] = "Lock(2),";
                        branch.Insert(index, "S");
                        keys.Add("Key 1,");
                        keys.Add("Key 2,");
                        dungeon.Add("Lock(2),");
                    }
                    else
                    {
                        branch[index] = "Enemy,";
                        branch.Insert(index, "S");
                        branch.Insert(index, "Puzzle,");
                    }
                    break;
            }
        }
        return branch;
    }

    //Find available space
    int[] FindSpace(int x, int y)
    {
        //Find all adjacent empty rooms
        List<Room> availableRooms = new List<Room>();
        if (GetRoom(x, y +1 ).Type == RoomType.Empty)
        {
            availableRooms.Add(GetRoom(x, y + 1));
        }
        if (GetRoom(x, y - 1).Type == RoomType.Empty)
        {
            availableRooms.Add(GetRoom(x, y - 1));
        }
        if (GetRoom(x - 1, y).Type == RoomType.Empty)
        {
            availableRooms.Add(GetRoom(x - 1, y));
        }
        if (GetRoom(x + 1, y).Type == RoomType.Empty)
        {
            availableRooms.Add(GetRoom(x + 1, y));
        }
        if(availableRooms.Count == 0) //If there are no adjacent rooms just try again by picking a different room as start point
        {
            Debug.LogError("No adjacent empty spaces");

            int randIndex = seeded.Next(0, dungeonData.Count - 1);
            Room start = dungeonData[randIndex]; //Want to pick a random room that isn't the last and is possibly the first
            while (start.Type == RoomType.End)
            {
                randIndex = seeded.Next(0, dungeonData.Count - 1);
                start = dungeonData[randIndex];
            }
            int x2 = start.X;
            int y2 = start.Y;
            connections += "{" + x2.ToString() + ", " + y2.ToString() + "} to ";

            int[] coords = FindSpace(x2, y2);
            int[] prevCoords = { x, y };
            connectionCoordinates.Add(prevCoords);
            connectionCoordinates.Add(coords);
            return coords;
        }
        else
        {
            //Pick a random room to return from the available options
            Room randomRoom = availableRooms[seeded.Next(0, availableRooms.Count)]; 
            int newX = randomRoom.X; //x coord of new room
            int newY = randomRoom.Y; //y coord of new room
            int[] coords = { newX, newY }; //new room's coords to return
            int[] prevCoords = { x, y }; //old coords for connection purposes
            connectionCoordinates.Add(prevCoords);
            connectionCoordinates.Add(coords);
            return coords;
        }
    }

    //Find adjacent occupied room that is not the previous room without creating a shortcut into the final room
    int[]FindAdjacent(int x, int y, int prevX, int prevY)
    {
        //Find all adjacent filled rooms
        List<Room> availableRooms = new List<Room>();
        if (GetRoom(x, y + 1).Type != RoomType.Empty && GetRoom(x, y + 1).Type != RoomType.End)
        {
            availableRooms.Add(GetRoom(x, y + 1));
        }
        if (GetRoom(x, y - 1).Type != RoomType.Empty && GetRoom(x, y - 1).Type != RoomType.End)
        {
            availableRooms.Add(GetRoom(x, y - 1));
        }
        if (GetRoom(x - 1, y).Type != RoomType.Empty && GetRoom(x - 1, y).Type != RoomType.End)
        {
            availableRooms.Add(GetRoom(x - 1, y));
        }
        if (GetRoom(x + 1, y).Type != RoomType.Empty && GetRoom(x + 1, y).Type != RoomType.End)
        {
            availableRooms.Add(GetRoom(x + 1, y));
        }
        int[] prevCoords = { x, y };
        if (availableRooms.Count > 1)
        {
            Room previousRoom = GetRoom(prevX, prevY); //Find the room we don't want
            availableRooms.Remove(previousRoom); //Remove it
            Room randomRoom = availableRooms[seeded.Next(0, availableRooms.Count)];
            int newX = randomRoom.X;
            int newY = randomRoom.Y;
            int[] coords = { newX, newY };
            extraConnectionCoords.Add(prevCoords);
            extraConnectionCoords.Add(coords);
            return coords;
        }
        else
        {
            return prevCoords;
        }
    }

    //Convert string into room type
    void CreateRoom(string type, int x, int y)
    {
        switch (type)
        {
            case "Entrance,":
                SetRoom(x, y, RoomType.Entrance);
                break;
            case "Lock,":
                SetRoom(x, y, RoomType.Lock);
                break;
            case "Lock(3),":
                SetRoom(x, y, RoomType.Lock3);
                break;
            case "Lock(2),":
                SetRoom(x, y, RoomType.Lock2);
                break;
            case "Key,":
                SetRoom(x, y, RoomType.Key);
                break;
            case "Key 3,":
                SetRoom(x, y, RoomType.Key3);
                break;
            case "Key 2,":
                SetRoom(x, y, RoomType.Key2);
                break;
            case "Key 1,":
                SetRoom(x, y, RoomType.Key1);
                break;
            case "Puzzle,":
                SetRoom(x, y, RoomType.Puzzle);
                break;
            case "Enemy,":
                SetRoom(x, y, RoomType.Enemy);
                break;
            case "End":
                SetRoom(x, y, RoomType.End);
                break;
        }
    }

    public List<Room> GetDungeonData()
    {
        return dungeonData;
    }
}
