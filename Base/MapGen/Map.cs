using Godot;
using System;
using System.Collections.Generic;

public class Map<TYPE, ITEM> where TYPE : Enum where ITEM : class, new()
{
    private readonly List<TYPE> _types;
    private readonly List<ITEM> _items;

    public int Width { get; }
    public int Height { get; }
    public int Size { get; }
    public Rect2I Rect { get; }

    public Map(int width, int height, TYPE initialTileType)
    {
        Width = width;
        Height = height;
        Size = width * height;
        Rect = new(0, 0, width, height);

        _types = new(Size);
        _items = new(Size);

        for (int i = 0; i < Size; ++i)
        {
            _types.Add(initialTileType);
            _items.Add(new ITEM());
        }
    }

    public void SetAllTypes(TYPE tileType)
    {
        for (int i = 0; i < Size; ++i)
        {
            _types[i] = tileType;
        }
    }


    // Same for Map and WrapMap
    public bool IsInside(Vector2I coord)
    {
        return IsInside(coord.X, coord.Y);
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool IsOffsetInside(Vector2I coord, Vector2I offset)
    {
        var final = coord + offset;
        return IsInside(final);
    }

    // Will be overriden by WrapMap
    public virtual bool IsValid(Vector2I coord)
    {
        return IsValid(coord.X, coord.Y);
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool IsIndexValid(int index)
    {
        return index >= 0 && index < Size;
    }

    // Will be overriden by WrapMap
    public virtual int GetIndex(Vector2I coord)
    {
        return GetIndex(coord.X, coord.Y);
    }

    public int GetIndex(int x, int y)
    {
        return y * Width + x;
    }

    public Vector2I GetCoord(int index)
    {
        int y = index / Width;
        int x = index - y * Width;
        return new Vector2I(x, y);
    }

    public Vector2I PosModCoord(Vector2I coord)
    {
        return new Vector2I(Mathf.PosMod(coord.X, Width), Mathf.PosMod(coord.Y, Height));
    }

    // The coord must be valid
    public void SetType(Vector2I coord, TYPE tileType)
    {
        _types[GetIndex(coord)] = tileType;
    }

    public void SetType(int x, int y, TYPE tileType)
    {
        _types[GetIndex(x, y)] = tileType;
    }

    public void SetTypeIndex(int index, TYPE tileType)
    {
        _types[index] = tileType;
    }

    public void SetItem(Vector2I coord, ITEM item)
    {
        _items[GetIndex(coord)] = item;
    }

    public void SetItem(int x, int y, ITEM item)
    {
        _items[GetIndex(x, y)] = item;
    }

    public void SetItemIndexed(int index, ITEM item)
    {
        _items[index] = item;
    }

    // The coord must be valid
    public TYPE GetType(Vector2I coord)
    {
        return _types[GetIndex(coord)];
    }

    public TYPE GetType(int x, int y)
    {
        return _types[GetIndex(x, y)];
    }

    public TYPE GetTypeIndexed(int index)
    {
        return _types[index];
    }

    public bool TryGetTypeWithOffset(Vector2I coord, Vector2I offset, out TYPE tileType)
    {
        Vector2I final = coord + offset;
        if (!IsValid(final))
        {
            tileType = default;
            return false;
        }
        tileType = GetType(final);
        return true;
    }

    public ITEM GetItem(Vector2I coord)
    {
        return _items[GetIndex(coord)];
    }

    public ITEM GetItem(int x, int y)
    {
        return _items[GetIndex(x, y)];
    }

    public bool TryGetItemWithOffset(Vector2I coord, Vector2I offset, out ITEM item)
    {
        Vector2I final = coord + offset;
        if (!IsValid(final))
        {
            item = default;
            return false;
        }
        item = GetItem(final);
        return true;
    }

    // The coord must be valid
    public bool IsType(Vector2I coord, TYPE tileType)
    {
        return GetType(coord).Equals(tileType);
    }

    public bool IsType(int x, int y, TYPE tileType)
    {
        return GetType(x, y).Equals(tileType);
    }

    public bool IsTypeIndexed(int index, TYPE tileType)
    {
        return GetTypeIndexed(index).Equals(tileType);
    }

    // Will check, if the coord is valid
    public bool IsTypeWithOffset(Vector2I coord, Vector2I offset, TYPE tileType)
    {
        return TryGetTypeWithOffset(coord, offset, out TYPE foundTileType) && foundTileType.Equals(tileType);
    }

    public bool IsTypeOf(Vector2I coord, ICollection<TYPE> tileTypes)
    {
        return tileTypes.Contains(GetType(coord));
    }

    // Will check, if the coord is valid
    public bool IsTypeAtDir4(Vector2I coord, Direction4 dir, TYPE tileType)
    {
        return TryGetTypeWithOffset(coord, dir.ToVector(), out TYPE foundTileType) && foundTileType.Equals(tileType);
    }

    // Will check, if the coord is valid
    public bool IsTypeAtDir8(Vector2I coord, Direction8 dir, TYPE tileType)
    {
        return TryGetTypeWithOffset(coord, dir.ToVector(), out TYPE foundTileType) && foundTileType.Equals(tileType);
    }

    public int GetNeighbour4Count(Vector2I coord, TYPE tileType)
    {
        int count = 0;

        foreach (Direction4 dir in Enum.GetValues<Direction4>())
            if (IsTypeAtDir4(coord, dir, tileType))
                count += 1;

        return count;
    }

    public int GetNeighbour8Count(Vector2I coord, TYPE tileType)
    {
        int count = 0;

        foreach (Direction8 dir in Enum.GetValues<Direction8>())
            if (IsTypeAtDir8(coord, dir, tileType))
                count += 1;

        return count;
    }


    public List<Vector2I> GetCoordsInRect(Rect2I rect)
    {
        List<Vector2I> coords = new(rect.Size.X * rect.Size.Y);
        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
                coords.Add(new Vector2I(x, y));

        return coords;
    }
}
