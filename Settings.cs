using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Settings
{
    public static float SearchMobRange = 50;

    public static int DrinkAt = 45;
    public static int EatAt = 50;
    public static string Drink;
    public static string Food;
    public static string PetFood;

    public static int VendorFreeSlots = 3;
    // item quality
    public static string[] ProtectedItems;

    public static bool Looting = true;
    public static bool Skinning = true;
}