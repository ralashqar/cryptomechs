//#if UNITY_EDITOR
//using Sirenix.OdinInspector.Demos.RPGEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;
//namespace Sirenix.OdinInspector.Demos.RPGEditor
//{ 
using System.Collections.Generic;
using UnityEngine;

    //
    // Instead of adding [CreateAssetMenu] attribute, we've created a Scriptable Object Creator using Odin Selectors.
    // Characters can then be easily created in the RPG Editor window, which also helps ensure that they get located in the right folder.
    //
    // By inheriting from SerializedScriptableObject, we can then also utilize the extra serialization power Odin brings.
    // In this case, Odin serializes the Inventory which is a two-dimensional array. Everything else is serialized by Unity.
    // 

public class Character : UnitBase3D
{
    //[HorizontalGroup("Split", 55, LabelWidth = 70)]
    //[HideLabel, PreviewField(55, ObjectFieldAlignment.Left)]
    //public Texture Icon;

    //[VerticalGroup("Split/Meta")]
    //public string Name;

    //[VerticalGroup("Split/Meta")]
    //public string Surname;

    //[VerticalGroup("Split/Meta"), Range(0, 100)]
    //public int Age;

    [LabelWidth(80)]
    [HorizontalGroup("Split", 290), EnumToggleButtons]
    [VerticalGroup ("Split/Right")]
    public int MaxHealth;

    //[HorizontalGroup("Split", 290), EnumToggleButtons]
    [LabelWidth(80)]
    [VerticalGroup("Split/Right")]
    public StoryNarrative interactionNarrative;

    [LabelWidth(80)]
    [VerticalGroup("Split/Right")]
    public AIBehaviour aiBehaviour;

    [HideLabel]
    [TabGroup("Animations")]
    public AgentMover mover;

    [HideLabel]
    [TabGroup("Animations")]
    public AgentAnimationController AnimationController;

    //[HorizontalGroup("Split", 290), EnumToggleButtons, HideLabel]
    //public CharacterAlignment CharacterAlignment;

    [TabGroup("Starting Inventory")]
    public ItemSlot[,] Inventory = new ItemSlot[12, 6];

    [TabGroup("Starting Stats"), HideLabel]
    public CharacterStats Skills = new CharacterStats();

    [HideLabel]
    [TabGroup("Starting Equipment")]
    public CharacterEquipment StartingEquipment;

    [TabGroup("Starting Abilities")]
    public List<AbilityProjectileItem> StartingAbilities;

#if UNITY_EDITOR
    [TabGroup("Animations")]
    //[BoxGroup("Regenerate Animation Controller")]
    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    public void RegenerateAnimationController()
    {
        string controllerName = Name + "_Controller";
        AnimationController.CreateController(controllerName);
    }
#endif

    public void Update()
    {

    }
}
//}
//#endif
