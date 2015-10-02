using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

public class Mod
{
    public string JSONSource;
    public string Name;
    public string Description;
    public string Type;
    public string Path;

    public Dictionary<string, Type> Moddables = new Dictionary<string, Type>();
    public Assembly ScriptAssembly;
    public List<string> Dependencies = new List<string>();

    private bool Inited = false;

    public bool Init()
    {
        if (Inited)
            return true;

        if (JSONSource == null)
        {
            Debug.Log("JSONSource is invalid, returning");

            return false;
        }

        JSONObject Object = JSONObject.Create(JSONSource);

        JSONObject Value = Object.GetField("Name");

        if(Value != null && Value.type == JSONObject.Type.STRING)
        {
            Name = Value.str;
        }

        Value = Object.GetField("Description");

        if (Value != null && Value.type == JSONObject.Type.STRING)
        {
            Description = Value.str;
        }

        Value = Object.GetField("Dependencies");

        if(Value != null && Value.type == JSONObject.Type.STRING)
        {
            Dependencies.AddRange(Value.str.Split("|".ToArray()));
        }

        foreach(string Dependency in Dependencies)
        {
            if(!ModManager.Instance.LoadMod(Dependency))
            {
                Debug.Log("Unable to load Mod Bundle '" + Name + "': Unable to load dependency '" + Dependency + "'");

                return false;
            }
        }

        try
        {
            string AssemblyPath = Path + "/" + Name + ".dll";

#if MODMANAGER_COMPILES_MODS
            DirectoryInfo SourceDirectory = new DirectoryInfo(Path + "/Scripts/");

            if (SourceDirectory != null && SourceDirectory.Exists)
            {
                List<string> SourceFiles = new List<string>();

                Array.ForEach(SourceDirectory.GetFiles("*.cs", SearchOption.AllDirectories), (FileInfo File) =>
                {
                    SourceFiles.Add(File.FullName);
                });

                string DataPath = Application.dataPath;

#if UNITY_EDITOR
                DataPath = Application.dataPath + "/../" + Application.productName + "_Data/";
#endif

                StringBuilder Parameters = new StringBuilder();

                Parameters.Append("-target:library -out:\"" + AssemblyPath + "\"");

                Parameters.Append(" -r:\"" + Application.dataPath + "/../Library/ScriptAssemblies/Assembly-CSharp.dll" + "\"");
                Parameters.Append(" -r:\"" + DataPath + "/Managed/UnityEngine.dll" + "\"");
                Parameters.Append(" -r:\"" + DataPath + "/Managed/UnityEngine.UI.dll" + "\"");

                foreach (string Dependency in Dependencies)
                {
                    Parameters.Append(" -r:\"" + Dependency + "\"");
                }

                foreach(string SourceFile in SourceFiles)
                {
                    Parameters.Append(" \"" + SourceFile + "\"");
                }

                string GMCSFileName = Application.dataPath + "/../Mono/bin/gmcs";

                if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    GMCSFileName += ".bat";
                }

                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = GMCSFileName,
                    Arguments = Parameters.ToString(),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                try
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
                    process.WaitForExit();

                    if (!process.StandardError.EndOfStream)
                    {
                        Debug.Log("Unable to compile Mod Bundle '" + Name + "': " + process.StandardError.ReadToEnd());

                        return false;
                    }

                    if (!process.StandardOutput.EndOfStream)
                    {
                        Debug.Log(process.StandardOutput.ReadToEnd());
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Unable to compile Mod Bundle '" + Name + "'" + e);

                    return false;
                }
            }
#endif

            FileInfo AssemblyFileInfo = new FileInfo(AssemblyPath);

            if(AssemblyFileInfo.Exists)
            {
                FileStream AssemblyStream = AssemblyFileInfo.OpenRead();

                BinaryReader Reader = new BinaryReader(AssemblyStream);

                byte[] Content = Reader.ReadBytes((int)Reader.BaseStream.Length);

                ScriptAssembly = Assembly.Load(Content);

                if (ScriptAssembly == null)
                {
                    Debug.Log("Unable to load assembly '" + AssemblyPath + "'");

                    return false;
                }

                Type[] Types = ScriptAssembly.GetTypes();

                Array.ForEach(Types, (Type) =>
                {
                    if (Type.IsSubclassOf(typeof(Moddable)))
                    {
                        Moddables.Add(Type.Name, Type);
                    }
                });
            }
            else
            {
                Debug.Log("Unable to find assembly for Mod Bundle '" + Name + "'");

                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Unable to load script code for Mod Bundle '" + Name + "'");

            return false;
        }

        Inited = true;

        return true;
    }
}
