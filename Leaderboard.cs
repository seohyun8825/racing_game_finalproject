using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

struct PlayerStats
{
    public string name;
    public int position;
    public float timeEntered;

    public PlayerStats(string n, int p, float t)
    {
        name = n;
        position = p;
        timeEntered = t;
    }
}

public class Leaderboard
{
    static Dictionary<int, PlayerStats> lb = new Dictionary<int, PlayerStats>();
    static int carsRegistered = -1;

    public static void Reset()
    {
        lb.Clear();
        carsRegistered = -1;
    }
    public static int RegisterCar(string name)
    {
        carsRegistered++;
        lb.Add(carsRegistered, new PlayerStats(name, 0, 0f));
        // Debug.Log($"RegisterCar called with name: {name}. Current cars registered: {carsRegistered}");

        return carsRegistered;
    }

    public static void SetPosition(int rego, int lap, int checkpoint, float timeEntered)
    {
        int position = lap * 1000 + checkpoint;
        lb[rego] = new PlayerStats(lb[rego].name, position, timeEntered);

        // Debug.Log($"SetPosition called with rego: {rego}, lap: {lap}, checkpoint: {checkpoint}, timeEntered: {timeEntered}");
    }

    public static string GetPosition(int rego)
    {
        int index = 0;
        foreach (KeyValuePair<int, PlayerStats> pos in lb.OrderByDescending(key => key.Value.position).ThenBy(key => key.Value.timeEntered))

        {
            index++;
            if (pos.Key == rego)
            {
                switch (index) 
                {
                    case 1: return "First";
                    case 2: return "Second";
                    case 3: return "Third";
                    case 4: return "Fourth";
                }
            }
        }
        return "Unknown";
    }

    public static List<string> GetPlaces()
    {
        List<string> places = new List<string>();
        foreach (KeyValuePair<int, PlayerStats> pos in lb.OrderByDescending(key => key.Value.position).ThenBy(key => key.Value.timeEntered))

        {
            places.Add(pos.Value.name);

        }
        string log = "GetPlaces() Result: ";
        foreach (string place in places)
        {
            log += place + ", ";
        }
        // Debug.Log(log);


        return  places;
    }
}
