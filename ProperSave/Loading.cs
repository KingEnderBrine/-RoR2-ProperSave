using Mono.Cecil.Cil;
using MonoMod.Cil;
using ProperSave.Data;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections;
using System.IO;
using TinyJson;

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
                    OnLoadingStarted?.Invoke();
                }
                else
                {
                    OnLoadingEnded?.Invoke();
                }
            }
        }
        public static bool FirstRunStage { get; internal set; }

        public static event Action OnLoadingStarted;
        public static event Action OnLoadingEnded;

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
                ProperSave.CurrentSave.LoadTeam();
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
                    ProperSave.CurrentSave.LoadRun();
                    ProperSave.CurrentSave.LoadArtifacts();
                    ProperSave.CurrentSave.LoadPlayers();
                }

                return IsLoading;
            });
            c.Emit(OpCodes.Brfalse, c.Next);
            c.Emit(OpCodes.Ret);
        }

        private static IEnumerator LoadLobby()
        {
            if (PreGameController.instance == null)
            {
                ProperSave.InstanceLogger.LogInfo("PreGameController instance not found");
                yield break;
            }
            if (GameNetworkManager.singleton?.desiredHost.hostingParameters.listen == true && !SteamworksLobbyManager.ownsLobby)
            {
                ProperSave.InstanceLogger.LogInfo("You must be a lobby leader to load the game");
                yield break;
            }
            var metadata = SaveFileMeta.GetCurrentLobbySaveMetadata();

            if (metadata == null)
            {
                ProperSave.InstanceLogger.LogInfo("Save file for current users is not found");
                yield break;
            }
            var filePath = metadata.FilePath;
            if (!File.Exists(filePath))
            {
                ProperSave.InstanceLogger.LogInfo($"File \"{filePath}\" is not found");
                yield break;
            }

            IsLoading = true;
            var saveJSON = File.ReadAllText(filePath);
            ProperSave.CurrentSave = JSONParser.FromJson<SaveData>(saveJSON);
            ProperSave.CurrentSave.SaveFileMeta = metadata;

            PreGameController.instance.StartLaunch();
        }

        [ConCommand(commandName = "ps_load_lobby", flags = ConVarFlags.None, helpText = "Load saved game suitable for current lobby")]
        private static void CCRequestLoadLobby(ConCommandArgs args)
        {
            if (Run.instance != null)
            {
                ProperSave.InstanceLogger.LogInfo("Can't load while run is active");
                return;
            }
            if (IsLoading)
            {
                ProperSave.InstanceLogger.LogInfo("Already loading");
                return;
            }
            ProperSave.Instance.StartCoroutine(LoadLobby());
        }
    }
}
