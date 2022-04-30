//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class MarketItemSlot
    {
        [BoxGroup("MarketItem")]
        public int ItemCount;
        [BoxGroup("MarketItem")]
        public Item Item;
        [BoxGroup("MarketItem")]
        [TextArea(4, 12)]
        public string marketDescription;

        [BoxGroup("Monetary")]
        public bool useBaseRate;
        [ShowIf("useBaseRate")]
        [BoxGroup("Monetary")]
        public float baseRateMultiplier;

        [BoxGroup("Monetary")]
        public bool canBuyWithCurrency;
        [BoxGroup("Monetary")]
        [ShowIf("canBuyWithCurrency")]
        public int goldPrice;
        [BoxGroup("Monetary")]
        [ShowIf("canBuyWithCurrency")]
        public int dirhamPrice;

        [BoxGroup("Trade")]
        public List<ItemSlot> tradesFor;

    }

    [Serializable]
    public struct ItemSlot
    {
        [BoxGroup]
        public int ItemCount;
        [BoxGroup]
        public Item Item;
    }
}
//#endif
