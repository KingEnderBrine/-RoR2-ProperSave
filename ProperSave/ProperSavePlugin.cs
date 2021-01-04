using BepInEx;
using BepInEx.Logging;
using R2API;
using R2API.Utils;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ProperSave
{
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(CommandHelper))]

    //Support for BlazingDrummer's TemporaryLunarCoins
    [BepInDependency(ModSupport.BDTemporaryLunarCoinsGUID, BepInDependency.DependencyFlags.SoftDependency)]
    //Support for TemporaryLunarCoins
    [BepInDependency(ModSupport.TemporaryLunarCoinsGUID, BepInDependency.DependencyFlags.SoftDependency)]

    //Support for StartingItemsGUI
    [BepInDependency(ModSupport.StartingItemsGUIGUID, BepInDependency.DependencyFlags.SoftDependency)]

    //Support for BiggerBazaar
    [BepInDependency(ModSupport.BiggerBazaarGUID, BepInDependency.DependencyFlags.SoftDependency)]

    //Support for ShareSuit 
    [BepInDependency(ModSupport.ShareSuiteGUID, BepInDependency.DependencyFlags.SoftDependency)]

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "2.6.1")]
    [DisallowMultipleComponent]
    public class ProperSavePlugin : BaseUnityPlugin
    {
        internal static ProperSavePlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;

        internal static string SavesDirectory { get; } = System.IO.Path.Combine(Application.persistentDataPath, "ProperSave", "Saves");
        internal static SaveFile CurrentSave { get; set; }

        private void Awake()
        {
            Instance = this;

            SaveFileMetadata.PopulateSavesMetadata();

            ModSupport.GatherLoadedPlugins();
            ModSupport.RegisterHooks();

            Saving.RegisterHooks();
            Loading.RegisterHooks();

            LobbyUI.RegisterHooks();

            Commands.RegisterCommands();
        }

        private void Destroy()
        {
            Instance = null;

            Commands.UnregisterCommands();

            ModSupport.UnregisterHooks();

            Saving.UnregisterHooks();
            Loading.UnregisterHooks();

            LobbyUI.UnregisterHooks();
        }
    }
}