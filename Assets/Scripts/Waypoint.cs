using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Board.Player.Type playerType;

    public List<Waypoint> nextFields;
    public List<Waypoint> previousFields;

    private void Awake()
    {
        nextFields = new List<Waypoint>();
        previousFields = new List<Waypoint>();
    }

    public void AddNextWaypoint(Waypoint waypoint)
    {
        if (waypoint != null)
            nextFields.Add(waypoint);
    }

    public IList<Waypoint> NextWaypoint()
    {
        return nextFields;
    }

    public void AddPreviousWaypoint(Waypoint waypoint)
    {
        if (waypoint != null)
            previousFields.Add(waypoint);
    }

    public IList<Waypoint> PreviousWaypoint()
    {
        return previousFields;
    }

    public void UseBy(Board.Player.Type type)
    {
        playerType = type;
    }

    public bool IsPreferedBy(Board.Player.Type type)
    {
        // Dedicated field is always prefered
        if (type == playerType)
        {
            return true;
        }

        // If this field is not for Anyone => we should not be here
        // If this field is for Anyone, but there might be better
        // waypoint for this player
        if (playerType != Board.Player.Type.Any ||
            (previousFields.Count == 1
            && previousFields[0].nextFields.Count == 2 &&
            (previousFields[0].nextFields[0].playerType == type || previousFields[0].nextFields[1].playerType == type)))
        {
            return false;
        }

        return true;
    }
}
