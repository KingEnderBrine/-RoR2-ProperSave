using ProperSave.SaveData;
using RoR2;
using System;
using System.IO;
using PSTinyJson;
using UnityEngine.Networking;

namespace ProperSave
{
    internal static class Saving
    {
        internal static RunRngData PreStageRng { get; private set; }

        internal static void RegisterHooks()
        {
            //Save game after stage is loaded
            Stage.onStageStartGlobal += StageOnStageStartGlobal;

            //Delete save file when run is over
            Run.onServerGameOver += RunOnServerGameOver;

            //Save stage RNG before it changes
            On.RoR2.Run.GenerateStageRNG += RunGenerateStageRNG;
        }

        internal static void UnregisterHooks()
        {
            Stage.onStageStartGlobal -= StageOnStageStartGlobal;
            Run.onServerGameOver -= RunOnServerGameOver;
            On.RoR2.Run.GenerateStageRNG -= RunGenerateStageRNG;
        }

        private static void RunGenerateStageRNG(On.RoR2.Run.orig_GenerateStageRNG orig, Run self)
        {
            PreStageRng = new RunRngData(Run.instance);
            orig(self);
        }

        private static void RunOnServerGameOver(Run run, GameEndingDef ending)
        {
            try
            {
                var metadata = ProperSavePlugin.CurrentSave?.SaveFileMeta;
                if (metadata != null)
                {
                    File.Delete(metadata.FilePath);
                    SaveFileMetadata.Remove(metadata);
                    ProperSavePlugin.CurrentSave = null;
                }
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Failed to delete save file");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }

        private static void StageOnStageStartGlobal(Stage stage)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (Loading.FirstRunStage)
            {
                Loading.FirstRunStage = false;
                return;
            }
            if (stage.sceneDef.baseSceneName == "outro")
            {
                return;
            }

            SaveGame();
        }

        private static void SaveGame()
        {
            var save = ProperSavePlugin.CurrentSave = new SaveFile
            {
                SaveFileMeta = SaveFileMetadata.GetCurrentLobbySaveMetadata() ?? SaveFileMetadata.CreateMetadataForCurrentLobby()
            };

            if (string.IsNullOrEmpty(save.SaveFileMeta.FileName))
            {
                do
                {
                    save.SaveFileMeta.FileName = Guid.NewGuid().ToString();
                }
                while (File.Exists(save.SaveFileMeta.FilePath));
            }

            try
            {
                var json = JSONWriter.ToJson(save);
                File.WriteAllText(save.SaveFileMeta.FilePath, json);

                SaveFileMetadata.AddIfNotExists(save.SaveFileMeta);
                Chat.AddMessage(Language.GetString(LanguageConsts.PS_CHAT_SAVE));
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Failed to save the game");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }
    }
}
