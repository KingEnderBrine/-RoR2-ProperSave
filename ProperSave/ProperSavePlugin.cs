using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using UnityEngine;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion(ProperSave.ProperSavePlugin.Version)]
namespace ProperSave
{
    [BepInPlugin(GUID, Name, Version)]
    public class ProperSavePlugin : BaseUnityPlugin
    {
        public const string GUID = "com.KingEnderBrine.ProperSave";
        public const string Name = "Proper Save";
        public const string Version = "2.8.7";

        internal static ProperSavePlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;

        internal static string SavesDirectory { get; } = System.IO.Path.Combine(Application.persistentDataPath, "ProperSave", "Saves");
        internal static SaveFile CurrentSave { get; set; }
        internal static string ContentHash { get; private set; }

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
            
            Language.collectLanguageRootFolders += CollectLanguageRootFolders;
            ContentManager.onContentPacksAssigned += ContentManagerOnContentPacksAssigned;
        }

        private void Destroy()
        {
            Instance = null;

            ModSupport.UnregisterHooks();

            Saving.UnregisterHooks();
            Loading.UnregisterHooks();

            LobbyUI.UnregisterHooks();

            LostNetworkUser.Unsubscribe();
            
            Language.collectLanguageRootFolders -= CollectLanguageRootFolders;
            ContentManager.onContentPacksAssigned -= ContentManagerOnContentPacksAssigned;
        }

        public void CollectLanguageRootFolders(List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "Language"));
        }

        private void ContentManagerOnContentPacksAssigned(HG.ReadOnlyArray<ReadOnlyContentPack> contentPacks)
        {
            var md5 = MD5.Create();
            using (var writer = new StringWriter())
            {
                foreach (var contentPack in contentPacks)
                {
                    writer.Write(contentPack.identifier);
                    writer.Write(';');
                    WriteCollection(contentPack.artifactDefs, nameof(contentPack.artifactDefs));
                    WriteCollection(contentPack.bodyPrefabs, nameof(contentPack.bodyPrefabs));
                    WriteCollection(contentPack.equipmentDefs, nameof(contentPack.equipmentDefs));
                    WriteCollection(contentPack.expansionDefs, nameof(contentPack.expansionDefs));
                    WriteCollection(contentPack.gameModePrefabs, nameof(contentPack.gameModePrefabs));
                    WriteCollection(contentPack.itemDefs, nameof(contentPack.itemDefs));
                    WriteCollection(contentPack.itemTierDefs, nameof(contentPack.itemTierDefs));
                    WriteCollection(contentPack.masterPrefabs, nameof(contentPack.masterPrefabs));
                    WriteCollection(contentPack.sceneDefs, nameof(contentPack.sceneDefs));
                    WriteCollection(contentPack.skillDefs, nameof(contentPack.skillDefs));
                    WriteCollection(contentPack.skillFamilies, nameof(contentPack.skillFamilies));
                    WriteCollection(contentPack.survivorDefs, nameof(contentPack.survivorDefs));
                    WriteCollection(contentPack.unlockableDefs, nameof(contentPack.unlockableDefs));
                }

                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(writer.ToString()));
                ContentHash = Convert.ToBase64String(hash);

                void WriteCollection<T>(ReadOnlyNamedAssetCollection<T> collection, string collectionName)
                {
                    writer.Write(collectionName);
                    var i = 0;
                    foreach (var asset in collection)
                    {
                        writer.Write(i);
                        writer.Write('_');
                        writer.Write(collection.GetAssetName(asset) ?? string.Empty);
                        writer.Write(';');
                        i++;
                    }
                }
            }
        }
    }
}