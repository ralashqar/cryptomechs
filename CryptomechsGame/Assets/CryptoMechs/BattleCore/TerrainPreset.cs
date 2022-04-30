using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public enum TerrainType
{
    NEUTRAL = 0,
    DESERT,
    OASIS,
    MOUNTAIN,
    GRASSPLANE,
    SETTLEMENT,
    CUSTOM
}

[System.Serializable]
public class TerrainPreset
{
    [JsonConverter(typeof(StringEnumConverter))]
    public string ID = "Neutral";
    public float TimeMultiplier = 1;
    public float ConsumptionMultiplier = 1;
	public float FatigueMultiplier = 1;
	public float WaterAttrition = 1;
	public float FoodAttrition = 1;

    public void CopyFrom(TerrainPreset other)
    {
        ID = other.ID;
        TimeMultiplier = other.TimeMultiplier;
        ConsumptionMultiplier = other.ConsumptionMultiplier;
        FatigueMultiplier = other.FatigueMultiplier;
        WaterAttrition = other.WaterAttrition;
        FoodAttrition = other.FoodAttrition;
    }
}
