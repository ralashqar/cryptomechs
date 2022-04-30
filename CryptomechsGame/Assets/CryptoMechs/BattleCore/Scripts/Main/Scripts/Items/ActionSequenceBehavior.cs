//#if UNITY_EDITOR
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class ActionSequenceBehavior : UnitBase
    {
        [HorizontalGroup("Split", 0.5f, MarginLeft = 5, LabelWidth = 130)]
        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        //[BoxGroup(STATS_BOX_GROUP)]
        
        //[BoxGroup("ActionSequence")]
        public ActionSequence actionSequence;
    }
}
//#endif
