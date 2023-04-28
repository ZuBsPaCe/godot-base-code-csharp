using Godot;
using System;
using System.Collections.Generic;

public static class Extensions
{
    public static Vector2I ToVector(this Direction4 dir)
    {
        switch (dir)
        {
            case Direction4.N:
                return Vector2I.Up;
            case Direction4.E:
                return Vector2I.Right;
            case Direction4.S:
                return Vector2I.Down;
            case Direction4.W:
                return Vector2I.Left;
            default:
                throw new Exception($"Unknown Direction [{dir}]");
        }
    }

    public static Vector2I ToVector(this Direction8 dir)
    {
        // Hint: Can't normalize diagonal vectors. Used in Map.IsTypeAtDir8().

        switch (dir)
        {
            case Direction8.N:
                return Vector2I.Up;
            case Direction8.E:
                return Vector2I.Right;
            case Direction8.S:
                return Vector2I.Down;
            case Direction8.W:
                return Vector2I.Left;

            case Direction8.NE:
                return new Vector2I(1, -1);
            case Direction8.SE:
                return new Vector2I(1, 1);
            case Direction8.SW:
                return new Vector2I(-1, -1);
            case Direction8.NW:
                return new Vector2I(-1, 1);

            default:
                throw new Exception($"Unknown Direction [{dir}]");
        }
    }

    public static Vector2I StepDir(this Vector2I v, Direction4 dir)
    {
        return v + ToVector(dir);
    }

    public static Vector2I StepDir(this Vector2I v, Direction8 dir)
    {
        return v + ToVector(dir);
    }

    public static bool HasPoint(this Rect2I rect, int x, int y)
    {
        Vector2I coord = new(x, y);
        return rect.HasPoint(coord);
    }

    public static void AddRange<T>(this HashSet<T> hashset, IList<T> values)
    {
        for (int i = 0; i < values.Count; i++)
            hashset.Add(values[i]);
    }

    public static T GetRandomItem<T>(this List<T> list)
    {
        if (TryGetRandomItem(list, out var item)) 
            return item;

        throw new Exception($"GetRandomItem from empty list not possible");
    }

    public static bool TryGetRandomItem<T>(this List<T> list, out T item)
    {
        if (list.Count == 1)
        {
            item = list[0];
            return true;
        }

        if (list.Count > 1)
        {
            item = list[GD.RandPosInt() % list.Count];
            return true;
        }

        item = default;
        return false;
    }

    public static Orientation Reverse(this Orientation orientation)
    {
        if (orientation == Orientation.CW)
            return Orientation.CCW;

        Debug.Assert(orientation == Orientation.CCW);
        return Orientation.CW;
    }

    public static Direction4 Reverse(this Direction4 dir)
    {
        switch (dir)
        {
            case Direction4.N:
                return Direction4.S;

            case Direction4.E:
                return Direction4.W;

            case Direction4.S:
                return Direction4.N;

            case Direction4.W:
                return Direction4.E;

            default:
                throw new Exception($"Unknown Direction [{dir}]");
        }
    }
    
    public static Direction4 Turn(this Direction4 dir, Orientation orientation)
    {
        if (orientation == Orientation.CW)
        {
            switch (dir)
            {
                case Direction4.N:
                    return Direction4.E;

                case Direction4.E:
                    return Direction4.S;

                case Direction4.S:
                    return Direction4.W;

                case Direction4.W:
                    return Direction4.N;

                default:
                    throw new Exception($"Unknown Direction [{dir}]");
            }
        }

        Debug.Assert(orientation == Orientation.CCW);

        switch (dir)
        {
            case Direction4.N:
                return Direction4.W;

            case Direction4.E:
                return Direction4.N;

            case Direction4.S:
                return Direction4.E;

            case Direction4.W:
                return Direction4.S;

            default:
                throw new Exception($"Unknown Direction [{dir}]");
        }
    }
}
