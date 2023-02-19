using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class FarmonSaveData
{
    // Things that need to be saved:
    // uniqueID used for recognizing individual player farmon
    public uint uniqueID = 0;

    // Names
    public string FarmonName;
    public string Nickname;

    // bonus Stats
    public int GritBonus;
    public int PowerBonus;
    public int ReflexBonus;
    public int FocusBonus;
    public int SpeedBonus;

    // Level and experience
    public int Level;
    public int experience;
    public int perkPoints;
    public int attributePoints;

    public List<string> perks = new List<string>();

    // Prefab reference

    // Item(s)

    // Selected Perks

    // - also number of perk points/stat points

    public void Init(Farmon farmon)
    {
        uniqueID = farmon.uniqueID;

        FarmonName = farmon.farmonName;
        Nickname = farmon.nickname;

        GritBonus = farmon.GritBonus;
        PowerBonus = farmon.PowerBonus;
        ReflexBonus = farmon.ReflexBonus;
        FocusBonus = farmon.FocusBonus;
        SpeedBonus = farmon.SpeedBonus;

        Level = farmon.level;
        experience = farmon.experience;
        perkPoints = farmon.perkPoints;
        attributePoints = farmon.attributePoints;

        foreach(var perk in farmon.perkList)
        {
            perks.Add(perk.Key + " " + perk.Value);
        }
    }
}

public class PlayerSaveData
{
    public string SaveName = "Default Save Name";

    public int StoryProgress = 0;

    public uint[] FarmonSquadIds = new uint[Player.farmonPerTeam];

    public void Init(string saveName, uint[] farmonSquadIds, int storyProgress)
    {
        SaveName = saveName;
        FarmonSquadIds = farmonSquadIds;
        StoryProgress = storyProgress;
    }
}

public class SaveController : MonoBehaviour
{
    public void CreateNewPlayerSave(string saveName)
    {
        PlayerSaveData data = new PlayerSaveData();
        data.Init(saveName, new uint[Player.farmonPerTeam], 0);

        XmlSerializer serializer = new XmlSerializer(typeof(PlayerSaveData));
        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/playerSave1.xml");
        serializer.Serialize(writer.BaseStream, data);
        writer.Close();
    }

    public static void SavePlayer()
    {
        Player player = Player.instance;

        PlayerSaveData data = new PlayerSaveData();
        data.Init(player.SaveName, player.FarmonSquadIds, player.StoryProgress);

        XmlSerializer serializer = new XmlSerializer(typeof(PlayerSaveData));
        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/playerSave1.xml");
        serializer.Serialize(writer.BaseStream, data);
        writer.Close();
    }

    public static void LoadPlayer()
    {
        PlayerSaveData data;
        XmlSerializer serializer = new XmlSerializer(typeof(PlayerSaveData));
        // If that didn't work, try loading it directly (this may not work in an actual build
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/playerSave1.xml");
        data = serializer.Deserialize(reader) as PlayerSaveData;
        reader.Close();

        Player player = Player.instance;

        player.SaveName = data.SaveName;
        player.StoryProgress = data.StoryProgress;
        player.FarmonSquadIds = data.FarmonSquadIds;
    }

    public static void SaveFarmon(Farmon farmon, string fileName)
    {
        FarmonSaveData data = new FarmonSaveData();
        data.Init(farmon);

        XmlSerializer serializer = new XmlSerializer(typeof(FarmonSaveData));
        StreamWriter writer = new StreamWriter("Assets/Resources/FarmonInstances/" + fileName + ".xml");
        serializer.Serialize(writer.BaseStream, data);
        writer.Close();
    }

    public static uint SaveFarmonPlayer(Farmon farmon)
    {
        uint uniqueId;
        // Check if this farmon already has a unique ID (id != 0)
        if(farmon.uniqueID != 0)
        {
            uniqueId = farmon.uniqueID;
        }
        else
        {
            //Find the next available farmonId.

            Player player = Player.instance;

            //Get the next available farmonID
            //ToDo: update to find "holes" in the farmon IDs instead of always incrementing the ID.
            uint maxId = 0;
            foreach (uint id in player.FarmonSquadIds)
            {
                if (id > maxId)
                {
                    maxId = id;
                }
            }

            uniqueId = maxId + 1;
        }

        FarmonSaveData data = new FarmonSaveData();
        data.Init(farmon);
        data.uniqueID = uniqueId;

        XmlSerializer serializer = new XmlSerializer(typeof(FarmonSaveData));
        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + uniqueId + ".xml");
        serializer.Serialize(writer.BaseStream, data);
        writer.Close();

        return uniqueId;
    }


    public static GameObject LoadFarmon( string fileName)
    {
        // Attempt to load the farmon from the resources fold via it's .meta file. 
        TextAsset _xml = Resources.Load<TextAsset>(fileName);
        FarmonSaveData data;
        XmlSerializer serializer = new XmlSerializer(typeof(FarmonSaveData));

        if (_xml)
        {
            StringReader reader = new StringReader(_xml.ToString());
            data = serializer.Deserialize(reader) as FarmonSaveData;
            reader.Close();
        }
        else
        {
            // If that didn't work, try loading it directly (this may not work in an actual build
            StreamReader reader = new StreamReader("Assets/Resources/FarmonInstances/" + fileName + ".xml");
            data = serializer.Deserialize(reader) as FarmonSaveData;
            reader.Close();
        }

        return ConstructFarmon(data);
    }

    public static GameObject LoadFarmonPlayer(uint uniqueID)
    {
        FarmonSaveData data;
        XmlSerializer serializer = new XmlSerializer(typeof(FarmonSaveData));

        // If that didn't work, try loading it directly (this may not work in an actual build
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/" + uniqueID + ".xml");
        data = serializer.Deserialize(reader) as FarmonSaveData;
        reader.Close();

        return ConstructFarmon(data);
    }

    public static GameObject ConstructFarmon(FarmonSaveData data)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log("Loading data:" +
                      "\nNickname: " + data.Nickname +
                      "\nFarmon Name: " + data.FarmonName +
                      "\n\nLevel: " + data.Level +
                      "\nExperience: " + data.experience +
                      "\nPerk Points: " + data.perkPoints +
                      "\nAttribute Points: " + data.attributePoints +
                      "\n\nGrit Bonus: " + data.GritBonus +
                      "\nPower Bonus: " + data.PowerBonus +
                      "\nReflex Bonus: " + data.ReflexBonus +
                      "\nFocus Bonus: " + data.FocusBonus +
                      "\nSpeed Bonus: " + data.SpeedBonus);
        }

        GameObject farmonPrefab = Resources.Load("Farmon/" + data.FarmonName) as GameObject;
        GameObject farmonGameObject = GameObject.Instantiate(farmonPrefab);

        Farmon farmon = farmonGameObject.GetComponent<Farmon>();

        farmon.uniqueID = data.uniqueID;

        farmon.farmonName = data.FarmonName;
        farmon.nickname = data.Nickname;

        farmon.GritBonus = data.GritBonus;
        farmon.PowerBonus = data.PowerBonus;
        farmon.ReflexBonus = data.ReflexBonus;
        farmon.FocusBonus = data.FocusBonus;
        farmon.SpeedBonus = data.SpeedBonus;

        farmon.level = data.Level;
        farmon.experience = data.experience;
        farmon.perkPoints = data.perkPoints;
        farmon.attributePoints = data.attributePoints;

        foreach(string perkString in data.perks)
        {
            string[] perkStringSplit = perkString.Split(" ");

            farmon.perkList[perkStringSplit[0]] = int.Parse(perkStringSplit[1]);
        }

        if (farmon.nickname == "")
        {
            farmonGameObject.name = farmon.farmonName;
        }
        else
        {
            farmonGameObject.name = farmon.nickname;
        }

        return farmonGameObject;
    }
}

