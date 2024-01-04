﻿using ProperSave.SaveData;
using RoR2;
using System;
using PSTinyJson;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.UI;
using ProperSave.Data;
using Zio;

namespace ProperSave
{
    internal static class Saving
    {
        internal static RunRngData PreStageRng { get; private set; }
        internal static RngData PreStageInfiniteTowerSafeWardRng { get; private set; }

        internal static void RegisterHooks()
        {
            //Save game after stage is loaded
            On.RoR2.Run.BeginStage += StageOnStageStartGlobal;

            //Delete save file when run is over
            Run.onServerGameOver += RunOnServerGameOver;

            //Save stage RNG before it changes
            On.RoR2.Run.GenerateStageRNG += RunGenerateStageRNG;

            //Adding message to quit confirmation dialog
            IL.RoR2.QuitConfirmationHelper.IssueQuitCommand_Action += IssueQuitCommandIL;
        }

        internal static void UnregisterHooks()
        {
            On.RoR2.Run.BeginStage -= StageOnStageStartGlobal;
            Run.onServerGameOver -= RunOnServerGameOver;
            On.RoR2.Run.GenerateStageRNG -= RunGenerateStageRNG;
            IL.RoR2.QuitConfirmationHelper.IssueQuitCommand_Action -= IssueQuitCommandIL;
        }

        private static void RunGenerateStageRNG(On.RoR2.Run.orig_GenerateStageRNG orig, Run self)
        {
            PreStageRng = new RunRngData(Run.instance);
            if (self is InfiniteTowerRun infiniteTowerRun)
            {
                PreStageInfiniteTowerSafeWardRng = new RngData(infiniteTowerRun.safeWardRng);
            }
            orig(self);
        }

        private static void RunOnServerGameOver(Run run, GameEndingDef ending)
        {
            try
            {
                var metadata = ProperSavePlugin.CurrentSave?.SaveFileMeta;
                if (metadata != null && metadata.FilePath.HasValue)
                {
                    ProperSavePlugin.SavesFileSystem.DeleteFile(metadata.FilePath.Value);
                    SaveFileMetadata.Remove(metadata);
                }
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Failed to delete save file");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }

        private static void StageOnStageStartGlobal(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            try
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

                var sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
                if (sceneDef.sceneType == SceneType.Menu || sceneDef.sceneType == SceneType.Cutscene)
                {
                    return;
                }

                SaveGame();
            }
            finally
            {
                orig(self);
            }
        }

        private static void SaveGame()
        {
            var save = new SaveFile
            {
                SaveFileMeta = SaveFileMetadata.GetCurrentLobbySaveMetadata() ?? SaveFileMetadata.CreateMetadataForCurrentLobby()
            };

            if (string.IsNullOrEmpty(save.SaveFileMeta.FileName))
            {
                do
                {
                    save.SaveFileMeta.FileName = Guid.NewGuid().ToString();
                }
                while (ProperSavePlugin.SavesFileSystem.FileExists(save.SaveFileMeta.FilePath.Value));
            }

            try
            {
                var json = JSONWriter.ToJson(save);
                ProperSavePlugin.SavesFileSystem.WriteAllText(save.SaveFileMeta.FilePath.Value, json);

                ProperSavePlugin.CurrentSave = save;
                SaveFileMetadata.AddIfNotExists(save.SaveFileMeta);
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = string.Format(Language.GetString(LanguageConsts.PROPER_SAVE_CHAT_SAVE), Language.GetString(SceneCatalog.currentSceneDef.nameToken)) });
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Failed to save the game");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }

        private static void IssueQuitCommandIL(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdftn(out _), 
                x => x.MatchNewobj(out _),
                x => x.MatchLdstr(out _));

            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<SimpleDialogBox>>(AddQuitText);
        }

        private static void AddQuitText(SimpleDialogBox simpleDialogBox)
        {
            if (!NetworkServer.active && NetworkUser.readOnlyInstancesList.Count != NetworkUser.readOnlyLocalPlayersList.Count)
            {
                return;
            }
            if (ProperSavePlugin.CurrentSave == null)
            {
                simpleDialogBox.descriptionLabel.text += Language.GetString(LanguageConsts.PROPER_SAVE_QUIT_DIALOG_NOT_SAVED);
                return;
            }
            if (ProperSavePlugin.CurrentSave.RunData.stageClearCount == Run.instance.stageClearCount)
            {
                simpleDialogBox.descriptionLabel.text += Language.GetString(LanguageConsts.PROPER_SAVE_QUIT_DIALOG_SAVED);
                return;
            }
            simpleDialogBox.descriptionLabel.text += Language.GetStringFormatted(LanguageConsts.PROPER_SAVE_QUIT_DIALOG_SAVED_BEFORE, new[] { (Run.instance.stageClearCount - ProperSavePlugin.CurrentSave.RunData.stageClearCount).ToString() });
        }
    }
}
