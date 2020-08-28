using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{

    public string seed;
    System.Random seeded;

    //Room generation variables
    int normalCoinFlip = 50; //Variable that will change to lean towards a consistent 50% chance of a result

    List<int> room = new List<int>();

    private void Awake()
    {
        seeded = new System.Random(seed.GetHashCode()); //Convert seed into usable integer
    }

    public List<int> GenerateRoom(string seed)
    {
        room = new List<int>();

        //Room types
        //Room variant
        room.Add(seeded.Next(1, 3));
        //Small enemy type
        room.Add(seeded.Next(1, 5));
        //Big enemy type
        room.Add(seeded.Next(1, 5));
        //Random hazard type
        room.Add(seeded.Next(1, 4)); //Pit, spike, themed hazard
        //Binary hazard existence
        if (FlipCoin(seed))
        {
            //Binary hazard type
            room.Add(seeded.Next(1, 4)); //Pit, spike, themed hazard
        }
        else
        {
            //0s are for non existent entries to keep list length consistent
            room.Add(0);
        }
        //Binary enemy existence
        if (FlipCoin(seed))
        {
            //Binary enemy type
            room.Add(seeded.Next(1, 5));
        }
        else
        {
            //0s are for non existent entries to keep list length consistent
            room.Add(0);
        }
        //Binary wall existence
        if (FlipCoin(seed))
        {
            room.Add(1);
        }
        else
        {
            //0s are for non existent entries to keep list length consistent
            room.Add(0);
        }

        return room;
    }


    bool FlipCoin(string seed) //Heads is True tails is False
    {
        bool coin = true;

        int result = seeded.Next(1, 101);
        //If heads make tails more likely
        if (result <= normalCoinFlip)
        {
            coin = true;
            if (normalCoinFlip <= 50)
            {
                normalCoinFlip = normalCoinFlip / 2;
            }
            else
            {
                normalCoinFlip = 50;
            }
        }
        //If tails make heads more likely
        else if (result > normalCoinFlip)
        {
            coin = false;
            if (normalCoinFlip >= 50)
            {
                normalCoinFlip = 100 - ((100 - normalCoinFlip) / 2);
            }
            else
            {
                normalCoinFlip = 50;
            }
        }
        return coin;
    }
}
