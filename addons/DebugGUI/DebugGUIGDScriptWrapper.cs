using Godot;
using System;
using System.Collections.Generic;
using System.IO;

// Adds support for comment tags on gdscript variables
public static class DebugGUIGDScriptWrapper
{
    // TODO: Wrap nodes in an object which handles both c# and gdscript logic

    static Dictionary<Script, List<string>> scriptGraphProperties = new();
    static Dictionary<Script, List<string>> scriptLogProperties = new();
    static Dictionary<Node, Script> loggedNodes = new();

    const string GRAPH_ANNOTATION = "debugguigraph";
    const string LOG_ANNOTATION = "debugguilog";

    public static void RefreshScripts()
    {
        var dir = DirAccess.Open("res://");
        PopulateFrom(dir);

        foreach (var kvp in scriptGraphProperties)
        {
            GD.Print($"Graphable - {kvp.Key.ResourcePath}:");
            foreach (var field in kvp.Value)
            {
                GD.Print($"\t{field}");
            }
            GD.Print();
        }
        foreach (var kvp in scriptLogProperties)
        {
            GD.Print($"Loggable - {kvp.Key.ResourcePath}:");
            foreach (var field in kvp.Value)
            {
                GD.Print($"\t{field}");
            }
            GD.Print();
        }
    }
    public static void RegisterAttributes()
    {
        //RegisterAttributes(((SceneTree)Engine.GetMainLoop()).Root);
    }

    static void PopulateFrom(DirAccess dir)
    {
        if (dir == null) return;

        foreach (var file in dir.GetFiles())
        {
            if (file.GetExtension() == "gd")
            {
                var scriptPath = Path.Combine(dir.GetCurrentDir(), file);
                ScanScript(ResourceLoader.Load<Script>(scriptPath));
            }
        }

        foreach (var subdir in dir.GetDirectories())
        {
            var da = DirAccess.Open(Path.Combine(dir.GetCurrentDir(), subdir));
            PopulateFrom(da);
        }
    }

    static void ScanScript(Script script)
    {
        Godot.Collections.Array<Godot.Collections.Dictionary> propertyList = null;
        var sourceCode = script.SourceCode;

        using var sr = new StringReader(sourceCode);

        string line;
        while ((line = sr.ReadLine()) != null)
        {
            if (!line.Trim().StartsWith("#"))
            {
                continue;
            }

            var attributeLine = line.Trim('#', ' ').ToLower();
            // Graph attribute
            if (attributeLine.StartsWith(GRAPH_ANNOTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                if (propertyList == null)
                {
                    propertyList = script.GetScriptPropertyList();
                }

                if (ValidateProperty(sr.ReadLine(), propertyList, out string propName))
                {
                    if (!scriptGraphProperties.ContainsKey(script))
                    {
                        scriptGraphProperties.Add(script, new());
                    }
                    scriptGraphProperties[script].Add(propName);
                }
            }
            // Log attribute
            else if (attributeLine.StartsWith(LOG_ANNOTATION, StringComparison.InvariantCultureIgnoreCase))
            {
                if (propertyList == null)
                {
                    propertyList = script.GetScriptPropertyList();
                }

                if (ValidateProperty(sr.ReadLine(), propertyList, out string propName))
                {
                    if (!scriptLogProperties.ContainsKey(script))
                    {
                        scriptLogProperties.Add(script, new());
                    }
                    scriptLogProperties[script].Add(propName);
                }
            }
        }
    }

    readonly static char[] separators = new char[] { ' ', ';' };

    // Verifies this line is a property and returns 
    static bool ValidateProperty(
        string propertyCodeLine,
        Godot.Collections.Array<Godot.Collections.Dictionary> propertyList,
        out string propName)
    {
        propName = null;
        // Graph attribute
        var tokens = propertyCodeLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].Equals("var") && i != tokens.Length - 1)
            {
                propName = tokens[i + 1];
                break;
            }
        }

        if (propName == null) return false;

        foreach (var prop in propertyList)
        {
            if (propName == prop["name"].AsString())
            {
                return true;
            }
        }
        return false;
    }
}
