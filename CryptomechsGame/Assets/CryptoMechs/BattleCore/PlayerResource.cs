using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyFloatPair
{
    public string Key;
    public float Value;
}

[System.Serializable]
public class KeyStringPair
{
    public string Key;
    public string Value;
}

[System.Serializable]
public class PlayerResource
{

    public string ResourceID = "";

    public int Amount = 0;

    // VISUALS
    public string Description;
    public string SpritePath;

    //TYPE DATA
    public bool IsConsumable = true;
    public bool IsSpecialItem = false;
    public bool IsHumanServant = false;
    public bool IsLivestock = false;

    // CAPACITY
    public bool GrantsCapacity = false;
    public int CapacityGranted = 0;
    public int CapacityConsumed = 0;

    //CONSUMPTION DATA FOR HIRES AND LIVESTOCK
    public float GoldConsumedOverTime = 0; // for rents and hires
    public float DirhamConsumedOverTime = 0; // for rents and hires
    public float NutritionConsumedOverTime = 0; 
    public float WaterConsumedOverTime = 0;

    public float EmergencyLivestockNutritionValue = 0;
    public float EmergencyLivestockWaterValue = 0;


    //CONSUMABLES DATA
    public float BaseConsumptionRate = 0;
    public float NutritionGrantedOverTime = 0;
    public float WaterGrantedOverTime = 0;

    //BASE TRADE ATTRIBUTES
    public float GoldValue = 0;
    public float DirhamValue = 0;

    // SPECIAL ITEM VALUES - sell values in specific places, where the key is the map node location, i.e. a specific settlement
    public List<KeyFloatPair> SpecialGoldSellValues;
    public List<KeyFloatPair> SpecialSilverSellValues;
    public List<KeyStringPair> SpecialTradeValues; // Specific barter trade one to one

}
