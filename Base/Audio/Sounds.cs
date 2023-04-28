using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public partial class Sounds : Node
{
    public static Sounds Instance { get; set; }


    private static Dictionary<int, AudioStreamPlayer> _sounds = new();

    public static void RegisterSound<T>(T soundType, AudioStreamPlayer player) where T : Enum
    {
        int intType = Unsafe.As<T, int>(ref soundType);

        _sounds.Add(intType, player);
    }

    public static void PlaySound<T>(T soundType) where T : Enum
    {
        int intType = Unsafe.As<T, int>(ref soundType);

        _sounds[intType].Play();
    }
}
