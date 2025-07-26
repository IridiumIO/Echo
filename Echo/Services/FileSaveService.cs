using CommunityToolkit.Mvvm.Messaging;
using Echo.Messages;
using Echo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Echo.Services;

public class FileSaveService
{

    private readonly DirectoryInfo _FileLocation;
    private readonly FileInfo _ProcessTriggersJSONFile;


    public FileSaveService()
    {
        _FileLocation = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "Echo"));
        _ProcessTriggersJSONFile = new FileInfo(Path.Combine(_FileLocation.FullName, "ProcessTriggers.json"));
    }

    public List<ProcessTrigger> LoadProcessTriggersFromDisk()
    {

        if (!Directory.Exists(_FileLocation.FullName)) _FileLocation.Create();
        if (!File.Exists(_ProcessTriggersJSONFile.FullName)) _ProcessTriggersJSONFile.Create().Dispose();

        List<ProcessTrigger> processTriggers = new();

        try
        {
            var jsonContent = File.ReadAllText(_ProcessTriggersJSONFile.FullName);
            processTriggers = System.Text.Json.JsonSerializer.Deserialize<List<ProcessTrigger>>(jsonContent) ?? new List<ProcessTrigger>();

            // Re-wire event handlers for each ripple in each trigger
            foreach (var trigger in processTriggers)
            {
                trigger.TargetProcessPaths.CollectionChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();
                trigger.RipplePrograms.CollectionChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();

                foreach (var ripple in trigger.RipplePrograms)
                {
                    ripple.ProcessTriggerOptions.PropertyChanged += (s, e) => WeakReferenceMessenger.Default.Send<ProcessModifiedMessage>();
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading process triggers: {ex.Message}");
        }

        return processTriggers;

    }



    private readonly JsonSerializerOptions Jsonoptions = new JsonSerializerOptions {WriteIndented = true };

    public void SaveProcessTriggersToDisk(List<ProcessTrigger> processTriggers)
    {
        if (!Directory.Exists(_FileLocation.FullName)) _FileLocation.Create();
        if (!File.Exists(_ProcessTriggersJSONFile.FullName)) _ProcessTriggersJSONFile.Create().Dispose();
        try
        {
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(processTriggers, Jsonoptions);
            File.WriteAllText(_ProcessTriggersJSONFile.FullName, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving process triggers: {ex.Message}");
        }
    }


}
