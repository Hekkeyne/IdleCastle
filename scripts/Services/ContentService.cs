using System;
using Godot;
using System.Collections.Generic;
using System.Text.Json;
public class BuildingData
{
    public int Version;
    public List<BuildingDef> Buildings;
}
public class ContentService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        IncludeFields = true,//яитаем публичные поля, иначе BuildingDef будет пустой
        PropertyNameCaseInsensitive = true//чтобы не париться с регистром (я дебил написал в json с маленькими буквами, а в коде с большими)
    };
    public static List<BuildingDef> LoadBuildingDefs()
    {
        const string path = "res://data/buildings.json";//путь до джсона в годоте
        string json = FileAccess.GetFileAsString(path);//читаем файл в строку
        if (string.IsNullOrEmpty(json))
        {
            GD.PrintErr($"ContentService Не получилось прочитать{path}");
            return new List<BuildingDef>();
        }//пустой - ошибка, возвращаем новывй пустой список без зданий (чтоб игра могла запуститься)
        try
        {
            var data = JsonSerializer.Deserialize<BuildingData>(json, Options);
            if (data?.Buildings == null)
            {
                GD.PrintErr($"ContentService Битый файл {path}");
                return new List<BuildingDef>();
            }
            return data.Buildings;
            //берём троку и собираем объект BuildingData, который содержит список зданий. Если что-то пошло не так - ошибка и пустой список
        }
        catch (JsonException e)
        {
            GD.PrintErr($"ContentService Ошибка JSON {path}: {e.Message}");
            return new List<BuildingDef>();
        }
    }
}