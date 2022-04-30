using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class InventoryManager : MonoBehaviour
{
    public CharacterAgentController cachedCharacter;

    public InventoryManager(CharacterAgentController character)
    {
        this.cachedCharacter = character;
        InitializeStartingInventory();
        this.UnitsKilled = new Dictionary<Character, int>();
    }

    public Dictionary<Character, int> UnitsKilled;

    public List<ItemBase> InventoryItems;

    public void SetStaringCharacter(CharacterAgentController character)
    {
        this.cachedCharacter = character;
    }

    public void InitializeStartingInventory()
    {
        foreach (ItemSlot slot in this.cachedCharacter.data.Inventory)
        {
            for (int i = 0; i < slot.ItemCount; ++i)
            {
                AddInventoryItem(slot.Item);
            }
        }
    }

    public void AddUnitKilled(Character character)
    {
        if (!UnitsKilled.ContainsKey(character))
        {
            UnitsKilled.Add(character, 1);
        }
        else
        {
            UnitsKilled[character] += 1;
        }
    }

    public int NumberOfItemsHeld(ItemBase item)
    {
        if (InventoryItems == null || InventoryItems.Count == 0) return 0;
        List<ItemBase> items = InventoryItems.FindAll(i => i == item);
        if (items != null) return items.Count;
        return 0;
    }

    public bool HasItem(ItemBase item)
    {
        if (InventoryItems.Find(i => i == item)) return true;
        return false;
    }

    public void AddInventoryItem(ItemBase item)
    {
        if (InventoryItems == null) InventoryItems = new List<ItemBase>();
        InventoryItems.Add(item);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
