using System.Collections.Generic;
using UnityEngine;

public class SpecialData
{
    public string name;
    public string description;

    public SpecialData(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}

public class ResourceManager : Singleton<ResourceManager>
{
    protected override bool IsDontDestroying => true;
    private readonly Dictionary<SpecialType, SpecialData> specialDictionary = new Dictionary<SpecialType, SpecialData>();

    protected override void OnCreated()
    {
        LoadSpecialData();
    }

    private void LoadSpecialData()
    {
        string pathName = "SpecialCard";
        var tsvText = Resources.Load<TextAsset>(pathName).text;
        var lines = tsvText.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var columns = lines[i].Split('\t');
            if (string.IsNullOrEmpty(columns[0])) continue;

            var specialData = new SpecialData(columns[0], columns[1]);
            SpecialType specialType = (SpecialType)(i - 1);

            specialDictionary.Add(specialType, specialData);
        }
    }

    public SpecialData GetSpecialData(SpecialType type)
    {
        if (specialDictionary.ContainsKey(type))
            return specialDictionary[type];

        return new SpecialData(name: "error", description: "special data error");
    }
}