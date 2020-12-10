using ProperSave.SaveData;
using RoR2;
using System;
using System.IO;
using PSTinyJson;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.UI;

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

            //Adding message to quit confirmation dialog
            IL.RoR2.QuitConfirmationHelper.IssueQuitCommand_Action += IssueQuitCommandIL;
        }

        internal static void UnregisterHooks()
        {
            Stage.onStageStartGlobal -= StageOnStageStartGlobal;
            Run.onServerGameOver -= RunOnServerGameOver;
            On.RoR2.Run.GenerateStageRNG -= RunGenerateStageRNG;
            IL.RoR2.QuitConfirmationHelper.IssueQuitCommand_Action -= IssueQuitCommandIL;
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
                while (File.Exists(save.SaveFileMeta.FilePath));
            }

            try
            {
                var json = JSONWriter.ToJson(save);
                File.WriteAllText(save.SaveFileMeta.FilePath, json);

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
