using BepInEx;
using BepInEx.Logging;
using RoR2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion(ProperSave.ProperSavePlugin.Version)]
namespace ProperSave
{
    //Support for BiggerBazaar
    [BepInDependency(ModSupport.BiggerBazaarGUID, BepInDependency.DependencyFlags.SoftDependency)]

    //Support for ShareSuit 
    [BepInDependency(ModSupport.ShareSuiteGUID, BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin(GUID, Name, Version)]
    public class ProperSavePlugin : BaseUnityPlugin
    {
        public const string GUID = "com.KingEnderBrine.ProperSave";
        public const string Name = "Proper Save";
        public const string Version = "2.8.2";

        internal static ProperSavePlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;

        internal static string SavesDirectory { get; } = System.IO.Path.Combine(Application.persistentDataPath, "ProperSave", "Saves");
        internal static SaveFile CurrentSave { get; set; }

        private void Start()
        {
            Instance = this;

            SaveFileMetadata.PopulateSavesMetadata();

            ModSupport.GatherLoadedPlugins();
            ModSupport.RegisterHooks();

            Saving.RegisterHooks();
            Loading.RegisterHooks();

            LobbyUI.RegisterHooks();

            LostNetworkUser.Subscribe();

#warning Fix for language, remove when next update is out
            if (RoR2Application.GetBuildId() == "1.2.2.0")
            {
                On.RoR2.Language.SetFolders += LanguageSetFolders;
            }
            else
            {
                Language.collectLanguageRootFolders += CollectLanguageRootFolders;
            }
        }

        private void Destroy()
        {
            Instance = null;

            ModSupport.UnregisterHooks();

            Saving.UnregisterHooks();
            Loading.UnregisterHooks();

            LobbyUI.UnregisterHooks();

            LostNetworkUser.Unsubscribe();

#warning Fix for language, remove when next update is out
            if (RoR2Application.GetBuildId() == "1.2.2.0")
            {
                On.RoR2.Language.SetFolders -= LanguageSetFolders;
            }
            else
            {
                Language.collectLanguageRootFolders -= CollectLanguageRootFolders;
            }
        }

#warning Fix for language, remove when next update is out
        private void LanguageSetFolders(On.RoR2.Language.orig_SetFolders orig, Language self, IEnumerable<string> newFolders)
        {
            var dirs = Directory.EnumerateDirectories(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "Language"), self.name);
            orig(self, newFolders.Union(dirs));
        }

        public void CollectLanguageRootFolders(List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "Language"));
        }
    }
}