using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSelectionController : MonoBehaviour
{
    public List<ActivateZone> zones;

    public string GetActivatedZone()
    {
        // loop through all zones and return the first activated zone by string
        foreach (ActivateZone zone in zones)
        {
            if (zone.GetIsActivated)
            {
                return zone.GetZoneName();
            }
        }

        return "None";
    }
}
