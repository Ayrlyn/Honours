using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

    Action<Room> cbRoomTypeChanged; //The callback for room type changing

    RoomType type = RoomType.Empty; //Default room type

    public RoomType Type
    { get { return type; }
        set
        {
            RoomType oldRoom = type; //Remember the old type so a change can be checked.
            type = value;

            //If the type isn't null and has actually changed then update it
            if (cbRoomTypeChanged != null && oldRoom != type)
            {
                cbRoomTypeChanged(this);
            }
        }
    }

    Dungeon dungeon; //The Dungeon the Room exists in

    //Need to access coordinates but coordinates should never change
    int x; //The Room's X coordinate
    public int X{get{return x;}}
    int y; //The Room's Y coordinate
    public int Y{get{return y;}}

    //The constructor
    public Room(Dungeon dungeon, int x, int y, RoomType roomType)
    {
        //Set local variables to arguments passed by constructor
        this.dungeon = dungeon;
        this.x = x;
        this.y = y;
        this.type = roomType;
    }

    public void RegisterRoomTypeChangedCallback(Action<Room> callback)
    {
        cbRoomTypeChanged += callback; //Can store multiple functions in a kind of array
    }

    public void UnregisterRoomTypeChangedCallback(Action<Room> callback)
    {
        cbRoomTypeChanged -= callback; //Can remove functions from the array
    }
}
