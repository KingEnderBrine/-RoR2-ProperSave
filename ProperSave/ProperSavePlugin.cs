using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: R2API.Utils.ManualNetworkRegistration]
[assembly: EnigmaticThunder.Util.ManualNetworkRegistration]
namespace ProperSave
{
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

    [BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "2.7.0")]
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

            On.RoR2.Language.LoadStrings += LanguageConsts.OnLoadStrings;
        }

        private void Destroy()
        {
            Instance = null;

            ModSupport.UnregisterHooks();

            Saving.UnregisterHooks();
            Loading.UnregisterHooks();

            LobbyUI.UnregisterHooks();

            On.RoR2.Language.LoadStrings -= LanguageConsts.OnLoadStrings;
        }
    }
}