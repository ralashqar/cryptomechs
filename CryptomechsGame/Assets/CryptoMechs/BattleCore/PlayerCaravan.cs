using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class PlayerCaravan : MonoBehaviour
{
    public List<Item> playerResources;

    // Start is called before the first frame update

    public int GetTotalCaravanCapacity()
    {
        int totalCapacity = 0;
        foreach (Item r in playerResources)
        {
            IGrantCapacity c = r as IGrantCapacity;
            if (c != null)
            {
                totalCapacity += c.GrantCapacity;
            }
        }
        return totalCapacity;
        //playerResources.FindAll(r => r.grantsCapacity)
    }

    public int GetConsumedCaravanCapacity()
    {
        int totalCapacity = 0;
        foreach (Item r in playerResources)
        {
            IGrantCapacity c = r as IGrantCapacity;
            if (c != null)
            {
                totalCapacity += r.CapacityConsumed;
            }
        }
        return totalCapacity;
        //playerResources.FindAll(r => r.grantsCapacity)
    }

    public float GetTotalFoodConsumptionRateMultiplier(TerrainPreset terrain, float journeyTimeDays)
    {
        float totalConsumptionRate = 0;
        foreach(Item r in playerResources)
        {
            IConsumesFoodAndWater c = r as IConsumesFoodAndWater;
            if (r != null)
            {
                totalConsumptionRate += c.NutritionConsumption;
            }
        }

        return terrain.FoodAttrition * totalConsumptionRate * journeyTimeDays;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
