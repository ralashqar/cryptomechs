using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;

public interface IMinable : IBaseUnit
{
    float MineValue { get; set; }
    void Mine(float amount, float time);
}

public class MinableUnit : BaseUnit, IMinable 
{
    [OnValueChanged("LoadMinable")]
    [InlineEditor]
    public Item data;

    public float MineValue { get; set; }

    public override void Initialize()
    {
        LoadMinable();
    }

    public void LoadMinable()
    {
        //if (data.Has3DRepresentation)
        //    LoadVisuals(data.prefabObj);
        CharacterAgentsManager.AddMinable(this);
    }

    public void Mine(float amount, float time = 0)
    {
        SummonFX summonFX = GetComponentInChildren<SummonFX>();
        summonFX?.TriggerMine(amount, time);
    }

    public override void Tick()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
