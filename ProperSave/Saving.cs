using ProperSave.SaveData;
using RoR2;
using System;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.UI;
using ProperSave.Data;

namespace ProperSave
{
    internal static class Saving
    {
        internal static RunRngData PreStageRng { get; private set; }
        internal static RngData PreStageInfiniteTowerSafeWardRng { get; private set; }
        internal static string PreStageSceneName { get; private set; }

        internal static void RegisterHooks()
        {
            //Save game after stage is loaded
            On.RoR2.Run.BeginStage += StageOnStageStartGlobal;

            //Delete save file when run is over
            Run.onServerGameOver += RunOnServerGameOver;

            //Save stage RNG before it changes
            On.RoR2.Run.AdvanceStage += RunAdvanceStage;

            //Adding message to quit confirmation dialog
            IL.RoR2.QuitConfirmationHelper.IssueQuitCommand_Action += IssueQuitCommandIL;
        }

        internal static void UnregisterHooks()
        {
            On.RoR2.Run.BeginStage -= StageOnStageStartGlobal;
            Run.onServerGameOver -= RunOnServerGameOver;
            On.RoR2.Run.AdvanceStage -= RunAdvanceStage;
            IL.RoR2.QuitConfirmationHelper.IssueQuitCommand_Action -= IssueQuitCommandIL;
        }

        private static void RunAdvanceStage(On.RoR2.Run.orig_AdvanceStage orig, Run self, SceneDef sceneDef)
        {
            PreStageSceneName = SceneCatalog.GetSceneDefForCurrentScene().cachedName;
            PreStageRng = RunRngData.Create(Run.instance);
            if (self is InfiniteTowerRun infiniteTowerRun)
            {
                PreStageInfiniteTowerSafeWardRng = RngData.Create(infiniteTowerRun.safeWardRng);
            }
            orig(self, sceneDef);
        }

        private static void RunOnServerGameOver(Run run, GameEndingDef ending)
        {
            try
            {
                var metadata = ProperSavePlugin.CurrentSave;
                if (metadata != null && metadata.FilePath.HasValue)
                {
                    if (ProperSavePlugin.SavesFileSystem.FileExists(metadata.FilePath.Value))
                    {
                        ProperSavePlugin.SavesFileSystem.DeleteFile(metadata.FilePath.Value);
                    }
                    ProperSavePlugin.CurrentSave = null;
                    SaveFileMetadata.SavesMetadata.Remove(metadata);
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

                if (ProperSavePlugin.CurrentSave?.ForceLoad ?? false)
                {
                    return;
                }

                SaveGame();
            }
            catch (Exception ex)
            {
                ProperSavePlugin.InstanceLogger.LogError(ex);
            }
            finally
            {
                orig(self);
            }
        }

        private static void SaveGame()
        {
            var oldMetadata = ProperSavePlugin.CurrentSave;
            var metadata = new SaveFileMetadata
            {
                FileName = oldMetadata?.FileName,
            };
            metadata.FillMetadataForCurrentLobby();

            try
            {
                metadata.Write(ProperSavePlugin.Resilient.Value);
                ProperSavePlugin.CurrentSave = metadata;
                SaveFileMetadata.Replace(metadata, oldMetadata);
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
            if (ProperSavePlugin.CurrentSave.Header.StageClearCount == Run.instance.stageClearCount)
            {
                simpleDialogBox.descriptionLabel.text += Language.GetString(LanguageConsts.PROPER_SAVE_QUIT_DIALOG_SAVED);
                return;
            }
            simpleDialogBox.descriptionLabel.text += Language.GetStringFormatted(LanguageConsts.PROPER_SAVE_QUIT_DIALOG_SAVED_BEFORE, new[] { (Run.instance.stageClearCount - ProperSavePlugin.CurrentSave.Header.StageClearCount).ToString() });
        }
    }
}
