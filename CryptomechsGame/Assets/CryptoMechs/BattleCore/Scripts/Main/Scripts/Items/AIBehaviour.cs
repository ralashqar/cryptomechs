//#if UNITY_EDITOR
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class AIBehaviour : UnitBase
    {
        [HorizontalGroup("Split", 0.5f, MarginLeft = 5, LabelWidth = 130)]
        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        [BoxGroup(STATS_BOX_GROUP)]
        [BoxGroup(STATS_BOX_GROUP + "/AI")]
        public List<IBehaviourSequence> AISequences;
    }
}
//#endif
