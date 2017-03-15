using System;
using System.Xml;
using ZzukBot.Objects;

public class Profile
{
    public Location[] Hotspots;
    public Location[] VendorHotspots;
    public Location[] GhostHotspots;
    public int[] Factions;

    public Location RepairNpcPosition;
    public string RepairNpcName;

    public bool FactionsContains(int FactionId)
    {
        bool Result = false;

        foreach(int Id in Factions)
        {
            if(Id == FactionId)
            {
                Result = true;
                break;
            }
        }

        return Result;
    }

    public static Location[] ParseHotspots(XmlNodeList Parent)
    {
        Location[] Hotspots = new Location[Parent.Count];
        
        for(int HotspotIndex = 0; HotspotIndex < Parent.Count; ++HotspotIndex)
        {
            XmlNode Node = Parent[HotspotIndex];

            XmlNode X = Node.SelectSingleNode("X");
            XmlNode Y = Node.SelectSingleNode("Y");
            XmlNode Z = Node.SelectSingleNode("Z");

            Hotspots[HotspotIndex] = new Location(float.Parse(X.InnerText), float.Parse(Y.InnerText), float.Parse(Z.InnerText));
        }

        return Hotspots;
    }

    public static Profile ParseProfile(string Path)
    {
        try
        {
            Profile Profile = new Profile();

            XmlDocument XmlDocument = new XmlDocument();
            XmlDocument.Load(Path);

            XmlNodeList Hotspots = XmlDocument.GetElementsByTagName("Hotspot");
            XmlNodeList VendorHotspots = XmlDocument.GetElementsByTagName("VendorHotspot");
            XmlNodeList GhostHotspots = XmlDocument.GetElementsByTagName("GhostHotspot");
            XmlNodeList Factions = XmlDocument.GetElementsByTagName("Faction");
            XmlNodeList Repair = XmlDocument.GetElementsByTagName("Repair");

            Profile.Hotspots = ParseHotspots(Hotspots);
            Profile.VendorHotspots = ParseHotspots(Hotspots);
            Profile.GhostHotspots = ParseHotspots(GhostHotspots);

            Profile.Factions = new int[Factions.Count];
            for(int FactionIndex = 0; FactionIndex < Factions.Count; ++FactionIndex)
            {
                XmlNode Node = Factions[FactionIndex];
                Profile.Factions[FactionIndex] = int.Parse(Node.InnerText);
                SimpleGrinder.DebugMsg("Faction[" + FactionIndex + "] = " + Profile.Factions[FactionIndex]);
            }
            
            if(Repair.Count > 0)
            {
                XmlNode RepairNode = Repair[0];
                
                XmlNode X = RepairNode.SelectSingleNode("Position/X");
                XmlNode Y = RepairNode.SelectSingleNode("Position/Y");
                XmlNode Z = RepairNode.SelectSingleNode("Position/Z");
                XmlNode NameNode = RepairNode.SelectSingleNode("Name");

                Profile.RepairNpcPosition = new Location(float.Parse(X.InnerText), float.Parse(Y.InnerText), float.Parse(Z.InnerText));
                SimpleGrinder.DebugMsg("X="+Profile.RepairNpcPosition.X);
                SimpleGrinder.DebugMsg("Y="+Profile.RepairNpcPosition.Y);
                SimpleGrinder.DebugMsg("Z="+Profile.RepairNpcPosition.Z);

                Profile.RepairNpcName = NameNode.InnerText;
                SimpleGrinder.DebugMsg("RepairNpcName="+Profile.RepairNpcName);
            }
            else
            {
                SimpleGrinder.DebugMsg("Not using repair");
            }
            
            return Profile;
        }
        catch(Exception e)
        {
            SimpleGrinder.DebugMsg("Error while parsing profile. " + e.ToString());
            return null;
        }
    }
};