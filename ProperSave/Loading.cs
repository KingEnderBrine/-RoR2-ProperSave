﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections;
using PSTinyJson;
using Zio;

namespace ProperSave
{
    public static class Loading
    {
        private static bool isLoading;
        public static bool IsLoading 
        {
            get => isLoading;
            private set
            {
                if (isLoading == value)
                {
                    return;
                }
                isLoading = value;
                if (isLoading)
                {
                    OnLoadingStarted?.Invoke(CurrentSave);
                }
                else
                {
                    OnLoadingEnded?.Invoke(CurrentSave);
                }
            }
        }
        public static bool FirstRunStage { get; internal set; }

        public static event Action<SaveFile> OnLoadingStarted;
        public static event Action<SaveFile> OnLoadingEnded;

        public static SaveFile CurrentSave => ProperSavePlugin.CurrentSave;

        internal static void RegisterHooks()
        {
            //Replace with custom run load
            IL.RoR2.Run.Start += RunStart;

            //Restore team expirience
            On.RoR2.TeamManager.Start += TeamManagerStart;
        }

        internal static void UnregisterHooks()
        {
            IL.RoR2.Run.Start -= RunStart;
            On.RoR2.TeamManager.Start -= TeamManagerStart;
        }

        private static void TeamManagerStart(On.RoR2.TeamManager.orig_Start orig, TeamManager self)
        {
            orig(self);
            if (IsLoading)
            {
                ProperSavePlugin.CurrentSave.LoadTeam();
                //This is last part of loading process
                IsLoading = false;
            }
        }

        private static void RunStart(ILContext il)
        {
            var c = new ILCursor(il);
            c.EmitDelegate<Func<bool>>(() =>
            {
                FirstRunStage = true;
                if (IsLoading)
                {
                    CurrentSave.LoadRun();
                    CurrentSave.LoadArtifacts();
                    CurrentSave.LoadPlayers();
                }
                else
                {
                    ProperSavePlugin.CurrentSave = null;
                }

                return IsLoading;
            });
            c.Emit(OpCodes.Brfalse, c.Next);
            c.Emit(OpCodes.Ret);
        }

        internal static IEnumerator LoadLobby()
        {
            if (PreGameController.instance == null)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("PreGameController instance not found");
                yield break;
            }
            if (NetworkManagerSystem.singleton?.desiredHost.hostingParameters.listen == true && !PlatformSystems.lobbyManager.ownsLobby)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("You must be a lobby leader to load the game");
                yield break;
            }

            var metadata = SaveFileMetadata.GetCurrentLobbySaveMetadata();
            if (metadata == null)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("Save file for current users is not found");
                yield break;
            }

            var filePath = metadata.FilePath;
            if (!filePath.HasValue)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("Metadata doesn't contain file name for the save file");
                yield break;
            }
            if (!ProperSavePlugin.SavesFileSystem.FileExists(filePath.Value))
            {
                ProperSavePlugin.InstanceLogger.LogInfo($"File \"{filePath}\" is not found");
                yield break;
            }

            var saveJSON = ProperSavePlugin.SavesFileSystem.ReadAllText(filePath.Value);
            ProperSavePlugin.CurrentSave = JSONParser.FromJson<SaveFile>(saveJSON);
            ProperSavePlugin.CurrentSave.SaveFileMeta = metadata;
            IsLoading = true;

            if (ProperSavePlugin.CurrentSave.ContentHash != null && ProperSavePlugin.CurrentSave.ContentHash != ProperSavePlugin.ContentHash)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Loading run but content mismatch detected which may result in errors");
            }

            PreGameController.instance.StartRun();
        }
    }
}
