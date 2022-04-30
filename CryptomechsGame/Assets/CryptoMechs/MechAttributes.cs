using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PartAttribute
{
    SPEED,
    AGILITY,
    STRENGTH,
    DEFENSE,
    
    ABILITY,
    ATTACK_DAMAGE,
    ATTACK_SPEED,
    ATTACK_RANGE,
    CRIT_CHANCE,
    DODGE_CHANCE,
    
    ROUNDS_PER_ATTACK
}

[System.Serializable]
public class AttributeDefinition
{
    public static bool HasStringVal(PartAttribute attrType)
    {
        switch (attrType)
        {
            case PartAttribute.ABILITY:
                return true;
            default:
                return false;
        }
    }

    public PartAttribute attributeType;
    public float attributeVal;
    public string attributeValStr;

    public AttributeDefinition(PartAttribute attributeType, float attributeVal, string strVal = "")
    {
        this.attributeType = attributeType;
        this.attributeVal = attributeVal;
        this.attributeValStr = strVal;
    }
}
public class MechAttributes
{

// Update is called once per frame
void Update()
    {
        
    }
}
