#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class TokenItem : ItemBase
    {
        private float receiveTimestamp;

        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        public StatList Modifiers;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.Token
                };
            }
        }

        public float GetReceivedTime()
        {
            return receiveTimestamp;
        }

        public override void OnReceive(float timestamp)
        {
            receiveTimestamp = timestamp;
        }
    }
}
#endif
