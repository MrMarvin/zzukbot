using System;
using System.IO;
using ZzukBot.Game.Statics;
using static ZzukBot.Constants.Enums;

public static class Util
{
    public static string ReadAllText(string Path)
    {
        string Result = null;

        try
        {
            Result = File.ReadAllText(Path);
        }
        catch (Exception) {}

        return Result;
    }
    
    public static void DebugMsg(string String)
    {
        ZzukBot.ExtensionMethods.StringExtensions.Log(String, "SimpleGrinder.txt", true);
        //Lua.Instance.Execute("DEFAULT_CHAT_FRAME:AddMessage(\"" + String + "\");");
    }
    
    public static bool IsManaClass(ClassId Class)
    {
        return Class != ClassId.Warrior && Class != ClassId.Rogue;
    }
}