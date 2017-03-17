using ZzukBot.Settings;

public class Settings
{
    public static Settings Instance;
    static Settings()
    {
        OptionManager Manager = OptionManager.Get("SimpleGrinder");
        Instance = Manager.LoadFromJson<Settings>();

        if(Instance == null)
        {
            Instance = new Settings();
        }
    }
    
    public static void SaveSettings()
    {
        OptionManager Manager = OptionManager.Get("SimpleGrinder");
        Manager.SaveToJson(Instance);
    }

    public string ProfileFilePath;
    public int MaxLevelDifference = 5;
    public float SearchMobRange = 50;
    public bool AlwaysDrinkWhenEating = false; // useful for mages

    public int DrinkAt = 45;
    public int EatAt = 50;
    public string Drink;
    public string Food;
    public string PetFood;

    public int VendorFreeSlots = 3;
    public string[] ProtectedItems;

    public bool Looting = true;
    public bool Skinning = true;
}