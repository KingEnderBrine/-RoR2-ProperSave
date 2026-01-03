using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ProperSave
{
    public static class Loading
    {
        private static bool isLoading;
        public static bool IsLoading 
        {
            get => isLoading;
            internal set
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

        public static SaveFile CurrentSave => ProperSavePlugin.CurrentSave?.Body;

        internal static void RegisterHooks()
        {
            //Replace with custom run load
            IL.RoR2.Run.Start += RunStart;

            //Restore team experience
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
                CurrentSave.LoadTeam();
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

            metadata.ReadBody();
            ProperSavePlugin.CurrentSave = metadata;
            IsLoading = true;

            if (metadata.Header.ContentHash != ProperSavePlugin.ContentHash)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Loading run but content mismatch detected which may result in errors");
            }

            PreGameController.instance.StartRun();
        }

        [ConCommand(commandName = "ps_force_load", flags = ConVarFlags.SenderMustBeServer, helpText = "[ProperSave] Load save from specified file ignoring user identifier.")]
        internal static void LoadForce(ConCommandArgs args)
        {
            var path = args.TryGetArgString(0);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Debug.LogError("Incorrect path");
                return;
            }

            var metadata = new SaveFileMetadata();

            try
            {
                metadata.ReadForce(path);
                ProperSavePlugin.CurrentSave = metadata;
                IsLoading = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load save file at path \"{path}\"");
                ProperSavePlugin.InstanceLogger.LogError(e);
                ResetLoading();
            }

            if (metadata.Header.ContentHash != ProperSavePlugin.ContentHash)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Loading run but content mismatch detected which may result in errors");
            }

            if (PreGameController.instance)
            {
                if (NetworkUser.readOnlyInstancesList.Count > 0)
                {
                    Debug.LogWarning("Force loading only allowed for 1 player in lobby");
                    ResetLoading();
                    return;
                }
                PreGameController.instance.StartRun();
            }
            else
            {
                ProperSavePlugin.Instance.StartCoroutine(LoadForceCoroutine());
            }

            static void ResetLoading()
            {
                ProperSavePlugin.CurrentSave = null;
                IsLoading = false;
            }
        }

        private static IEnumerator LoadForceCoroutine()
        {
            RoR2.Console.instance.SubmitCmd(null, "host 0");
            yield return new WaitUntil(() => PreGameController.instance != null);
            PreGameController.instance.StartRun();
        }
    }
}
