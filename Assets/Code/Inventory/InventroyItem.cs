using UnityEngine;


[System.Serializable]

public class InventroyItem
{
    public ItemData item;
    public int stackSize;

    public InventroyItem(ItemData item)
    {
        this.item = item;

        AddToStack();

    }

    public void AddToStack()
    {
         
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }


}
