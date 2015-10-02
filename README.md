# Unity-CSharp-Mod
Native C# Modding support for Unity

This is a sample project for creating a fully moddable environment in Unity in runtime using the Mono compiler, inspired by the Cities: Skylines game's modding method.

There's a Mod Manager that searches all directories in the Mods directory for .mod files containing info about a mod bundle. It runs the Mono compiler on the mod bundle's scripts folder to produce an assembly .dll that it loads into Unity.

The .mod format is a simple JSON file with a name, description, and any dependencies needed separated by |'s.

Every Moddable object must derive from the Moddable class, which contains a TheMod variable (to get the mod it came from and details about where it's located). You can spawn each moddable object by its class name using the Spawn method in the ModManager class.

You may choose to enable compilation with the MODMANAGER_COMPILES_MODS define on your player. Otherwise, it'll only try to load the mod .dll. You need a Mono folder containing the Mono compiler (usually from your Unity installation) in the same folder as the project folder in order to compile.

You may choose to disable mod loading at runtime and instead load all mods from memory by compiling all mods into your project and adding the MODMANAGER_LOADS_FROM_MEMORY define to make the mod manager search for all Moddable Types in your Unity assemblies.
This is useful in Mobile builds where you can't run the compiler.

# Credits
Art by Stephen Challener (Redshrike), hosted by OpenGameArt.org
Code by Nuno Silva (LittleCodingFox)
