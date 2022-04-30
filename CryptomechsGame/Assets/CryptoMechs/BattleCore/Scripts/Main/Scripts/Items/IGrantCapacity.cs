
public interface ITradeable
{

}


public interface IGrantCapacity
{
    //[BoxGroup(STATS_BOX_GROUP + "/Grants")]
    int GrantCapacity { get; set; }
}

public interface IConsumesFoodAndWater
{
    int WaterConsumption { get; set; }
    int NutritionConsumption { get; set; }
}

public interface IInventoryItem
{

}

public interface IExpireable
{
    float GetReceivedTime();
    float GetExpiryTime();
}