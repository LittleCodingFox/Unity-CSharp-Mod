using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

public class ModManager
{
    public Dictionary<string, Mod> Mods = new Dictionary<string, Mod>();

    private static ModManager PrivateInstance;
    public static ModManager Instance
    {
        get
        {
            if(PrivateInstance == null)
            {
                PrivateInstance = new ModManager();
                PrivateInstance.Initialize();
            }

            return PrivateInstance;
        }
    }

    public bool LoadMod(string Name)
    {
        if (Mods.ContainsKey(Name))
            return true;

        string ModDirectory = Environment.CurrentDirectory.Replace('\\', '/') + "/Mods/";

        try
        {
            foreach (DirectoryInfo Directory in new DirectoryInfo(ModDirectory).GetDirectories())
            {
                FileInfo[] Path = Directory.GetFiles("*.mod");

                if (Path.Length == 0)
                {
                    Debug.Log("Found no mods bundles at '" + Directory.FullName + "'");
                }
                else if(Path[0].Name == Name + ".mod")
                {
                    FileInfo ModPath = Path[0];

                    try
                    {
                        FileStream TheFile = ModPath.OpenRead();
                        StreamReader TheReader = new StreamReader(TheFile);

                        string Content = TheReader.ReadToEnd();

                        TheReader.Close();
                        TheFile.Close();

                        Mod TheMod = new Mod();
                        TheMod.JSONSource = Content;
                        TheMod.Path = Directory.FullName.Replace('\\', '/');

                        if (!TheMod.Init())
                        {
                            Debug.Log("Failed to load mod bundle '" + ModPath.FullName + "': Load failure");

                            return false;
                        }

                        Mods.Add(Name, TheMod);

                        Debug.Log("Loaded mod '" + ModPath.FullName + "'");

                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Failed to load mod bundle '" + ModPath.FullName + "': " + e.ToString());

                        return false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return false;
    }

    void Initialize()
    {
#if MODMANAGER_LOADS_FROM_MEMORY
        Moddable[] Moddables = UnityEngine.Object.FindObjectsOfType<Moddable>();

        Mod MemoryMod = new Mod();

        Array.ForEach(AppDomain.CurrentDomain.GetAssemblies(), (Assembly) =>
        {
            Array.ForEach(Assembly.GetTypes(), (Type) =>
            {
                if(Type.IsSubclassOf(typeof(Moddable)))
                {
                    MemoryMod.Moddables.Add(Type.Name, Type);
                }
            });
        });

        Mods.Add("__MEMORY__", MemoryMod);
#else
        string ModDirectory = Environment.CurrentDirectory.Replace('\\', '/') + "/Mods/";

        Debug.Log("Loading all mods bundles at '" + ModDirectory + "'");

        int Counter = 0, LoadedCounter = 0;

        try
        {
            foreach (DirectoryInfo Directory in new DirectoryInfo(ModDirectory).GetDirectories())
            {
                FileInfo[] Path = Directory.GetFiles("*.mod");

                if (Path.Length == 0)
                {
                    Debug.Log("Found no mods bundles at '" + Directory.FullName + "'");
                }
                else
                {
                    FileInfo ModPath = Path[0];

                    Counter++;

                    if(LoadMod(ModPath.Name.Replace(".mod", "")))
                    {
                        LoadedCounter++;
                    }
                }
            }
        }
        catch(DirectoryNotFoundException)
        {
            Debug.Log("Found no mods bundles at '" + ModDirectory + "'");
        }
        catch (Exception)
        {
        }

        Debug.Log("Loaded " + LoadedCounter + " mods bundles out of " + Counter);
#endif
    }

    public GameObject Spawn(string Name, Vector3 Position)
    {
        GameObject ClonedObject = new GameObject();
        ClonedObject.transform.position = Position;

        Moddable TheModdable = null;

        try
        {
            Type TheType = null;
            Mod TheMod = null;

            foreach (KeyValuePair<string, Mod> Entry in Mods)
            {
                if (Entry.Value.Moddables.TryGetValue(Name, out TheType))
                {
                    TheMod = Entry.Value;

                    break;
                }
            }

            if(TheType != null)
            {
                TheModdable = (Moddable)ClonedObject.AddComponent(TheType);
                TheModdable.TheMod = TheMod;
            }
        }
        catch(Exception e)
        {
            Debug.Log("Unable to start mod '" + Name + "': " + e.Message);

            return null;
        }

        return ClonedObject;
    }
}
