// ||=======================================================================||
// || ItemGroup: Defines a collection of items of the same type.            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[System.Serializable]
public class ItemGroup
{
    public ItemGroup(Item item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }

    public Item Item;
    public int  Quantity;
}