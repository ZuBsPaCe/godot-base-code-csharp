using System;
using System.Runtime.CompilerServices;

public class Debug
{
    internal static void Fail(
        string msg = null, 
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
    {
#if DEBUG
        Assert(false, msg, file, member, line);
#endif
    }

    internal static void Assert(
        bool cond, 
        string msg = null, 
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
#if DEBUG
    {
        if (cond) return;

        GD.PrintErr($"{(!string.IsNullOrEmpty(msg) ? msg : "Unknown Assert")}{Environment.NewLine}{file}({line})");
    }
#else
    {}
#endif
}
