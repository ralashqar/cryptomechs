using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MechPlacement
{
    PLAYER_1,
    PLAYER_2,
    OPPONENT_1,
    OPPONENT_2,
    NONE
}

public class MechPlacementNode : MonoBehaviour
{
    public MechPlacement placement;
}
