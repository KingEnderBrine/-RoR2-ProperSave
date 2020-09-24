using BepInEx;
using BepInEx.Logging;
using ProperSave.Data;
using R2API;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ProperSave
{
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(CommandHelper))]

    //Support for both versions of TLC
    [BepInDependency("com.blazingdrummer.TemporaryLunarCoins", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.MagnusMagnuson.TemporaryLunarCoins", BepInDependency.DependencyFlags.SoftDependency)]

    //Support for StartingItemsGUI
    [BepInDependency("com.Phedg1Studios.StartingItemsGUI", BepInDependency.DependencyFlags.SoftDependency)]

    //Support for BiggerBazaar
    [BepInDependency("com.MagnusMagnuson.BiggerBazaar", BepInDependency.DependencyFlags.SoftDependency)]

    //Support for ShareSuit 
    [BepInDependency("com.funkfrog_sipondo.sharesuite", BepInDependency.DependencyFlags.SoftDependency)]

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "2.4.5")]
    [DisallowMultipleComponent]
    public class ProperSave : BaseUnityPlugin
    {
        internal static ProperSave Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;

        internal static string SavesDirectory { get; } = System.IO.Path.Combine(Application.persistentDataPath, "ProperSave", "Saves");
        internal static SaveData CurrentSave { get; set; }

        public void OnEnabled()
        {
            Instance = this;

            SaveFileMeta.PopulateSavesMetadata();

            CommandHelper.AddToConsoleWhenReady();

            ModSupport.GatherLoadedPlugins();
            ModSupport.RegisterHooks();

            Saving.RegisterHooks();
            Loading.RegisterHooks();

            LobbyUI.RegisterHooks();
        }

        public void OnDisabled()
        {
            Instance = null;
            ModSupport.UnregisterHooks();

            Saving.UnregisterHooks();
            Loading.UnregisterHooks();

            LobbyUI.UnregisterHooks();
        }
    }
}