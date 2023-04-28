using Godot;
using System;

public class WrapMap<TYPE, ITEM> : Map<TYPE, ITEM> where TYPE : Enum where ITEM : class, new()
{
    public WrapMap(int width, int height, TYPE initialTileType) 
        : base(width, height, initialTileType)
    {
    }

    public override bool IsValid(Vector2I coord)
    {
        return true;
    }

    public override int GetIndex(Vector2I coord)
    {
        return Mathf.PosMod(coord.Y, Height) * Width + Mathf.PosMod(coord.X, Width);
    }
}
