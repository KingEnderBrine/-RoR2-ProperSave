using BepInEx;
using BepInEx.Configuration;
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
using Zio;
using Zio.FileSystems;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion(ProperSave.ProperSavePlugin.Version)]
namespace ProperSave
{
    [BepInPlugin(GUID, Name, Version)]
    public class ProperSavePlugin : BaseUnityPlugin
    {
        public const string GUID = "com.KingEnderBrine.ProperSave";
        public const string Name = "Proper Save";
        public const string Version = "2.9.0";

        private static readonly char[] invalidSubDirectoryCharacters = new[] { '\\', '/', '.' };

        internal static ProperSavePlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;

        internal static FileSystem SavesFileSystem { get; private set; }
        internal static UPath SavesPath { get; private set; } = (UPath)"/ProperSave" / "Saves";
        private static string SavesDirectory { get; set; }
        internal static SaveFile CurrentSave { get; set; }
        internal static string ContentHash { get; private set; }

        internal static ConfigEntry<bool> UseCloudStorage { get; private set; }
        internal static ConfigEntry<string> CloudStorageSubDirectory { get; private set; }
        internal static ConfigEntry<string> UserSavesDirectory { get; private set; }

        private void Start()
        {
            Instance = this;

            UseCloudStorage = Config.Bind("Main", "UseCloudStorage", false, "Store files in Steam/EpicGames cloud. Enabling this feature would not preserve current saves and disabling it wouldn't clear the cloud.");
            CloudStorageSubDirectory = Config.Bind("Main", "CloudStorageSubDirectory", "", "Sub directory name for cloud storage. Changing it allows to use different save files for different mod profiles.");
            UserSavesDirectory = Config.Bind("Main", "SavesDirectory", "", "Directory where save files will be stored. \"ProperSave\" directory will be created in the directory you have specified. If the directory doesn't exist the default one will be used.");

            RoR2Application.onLoad += () =>
            {
                SavesDirectory = string.IsNullOrEmpty(UserSavesDirectory.Value) || !Directory.Exists(UserSavesDirectory.Value) ? Application.persistentDataPath : UserSavesDirectory.Value;
                if (UseCloudStorage.Value)
                {
                    SavesFileSystem = RoR2Application.cloudStorage;
                    if (!string.IsNullOrEmpty(CloudStorageSubDirectory.Value))
                    {
                        if (CloudStorageSubDirectory.Value.IndexOfAny(invalidSubDirectoryCharacters) != -1)
                        {
                            Logger.LogError($"Config entry \"CloudStorageSubDirectory\" contains invalid characters. Falling back to default location.");
                        }
                        else
                        {
                            SavesPath /= CloudStorageSubDirectory.Value;
                        }
                    }
                }
                else
                {
                    var physicalFileSystem = new PhysicalFileSystem();
                    SavesFileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(SavesDirectory));
                }

                SaveFileMetadata.PopulateSavesMetadata();
            };

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