using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ProperSave.Data;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.UI;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TinyJson;
using UnityEngine;
using UnityEngine.UI;

namespace ProperSave
{
    [R2APISubmoduleDependency("LanguageAPI", "CommandHelper")]

    //Support for both versions of TLC
    [BepInDependency("com.blazingdrummer.TemporaryLunarCoins", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.MagnusMagnuson.TemporaryLunarCoins", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "1.1.0")]
    public class ProperSave : BaseUnityPlugin
    {
        private static WeakReference<GameObject> singleplayerContinueButton = new WeakReference<GameObject>(null);
        private static WeakReference<GameObject> multiplayerContinueButton = new WeakReference<GameObject>(null);
        private static WeakReference<GameObject> lobbyContinueButton = new WeakReference<GameObject>(null);

        public static ProperSave Instance { get; private set; }
        public static bool IsTLCDefined { get; private set; }
        public static bool IsOldTLCDefined { get; private set; }

        public static string ExecutingDirectory { get; } = Assembly.GetExecutingAssembly().Location.Replace("\\ProperSave.dll", "");
        public static string SavesDirectory { get; } = $"{ExecutingDirectory}\\Saves";
        private static bool IsLoading { get; set; }
        private static bool FirstRunStage { get; set; }
        private static SaveData Save { get; set; }
        private static List<SaveFileMeta> SavesMetadata { get; } = new List<SaveFileMeta>();

        public static RunRngData PreStageRng { get; private set; }
        public static ArtifactsData RunArtifactData { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }

            PopulateSavesMetadata();

            IsOldTLCDefined = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.MagnusMagnuson.TemporaryLunarCoins");
            IsTLCDefined = IsOldTLCDefined | BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.blazingdrummer.TemporaryLunarCoins");
            
            RegisterLanguage();

            CommandHelper.AddToConsoleWhenReady();

            if (IsOldTLCDefined)
            {
                RegisterTLCOverride();
            }

            RegisterGameLoading();

            RegisterGameSaving();

            RegisterSingleplayerContinueButton();

            //RegisterMultiplayerContinueButton();

            RegisterLobbyContinueButton();
        }

        [ConCommand(commandName = "ps_load", flags = ConVarFlags.None, helpText = "Load saved game")]
        private static void CCRequestLoad(ConCommandArgs args)
        {
            if (Run.instance != null)
            {
                Debug.Log("[ProperSave] Can't load while run is active");
                return;
            }
            if (IsLoading)
            {
                Debug.Log("[ProperSave] Already loading");
                return;
            }

            Instance.StartCoroutine(LoadGame());
        }

        private static IEnumerator LoadGame()
        {
            var metadata = GetSingleplayerSaveMetadata();
            if (metadata == null)
            {
                Debug.Log("[ProperSave] Save for current user not found");
                yield break;
            }

            var filePath = metadata.FilePath;
            if (!File.Exists(filePath))
            {
                Debug.Log($"[ProperSave] File \"{filePath}\" not found");
                yield break;
            }

            IsLoading = true;
            var saveJSON = File.ReadAllText(filePath);
            Save = JSONParser.FromJson<SaveData>(saveJSON);
            Save.SaveFileMeta = metadata;

            GameNetworkManager.singleton.desiredHost = new GameNetworkManager.HostDescription(new GameNetworkManager.HostDescription.HostingParameters
            {
                listen = false,
                maxPlayers = 1
            });
            yield return new WaitUntil(() => PreGameController.instance != null);
            PreGameController.instance?.StartLaunch();
        }

        private static void SaveGame()
        {
            Save = new SaveData();
            var metadata = Save.CreateMetadata();
            var foundMetadata = SavesMetadata.FirstOrDefault(el => el == metadata);
            var filePath = "";

            if (foundMetadata != null)
            {
                Save.SaveFileMeta = foundMetadata;
                filePath = foundMetadata.FilePath;
            }
            else
            {
                Save.SaveFileMeta = metadata;
                var saveId = Guid.NewGuid();
                metadata.FileName = saveId.ToString();
                filePath = metadata.FilePath;
                
                SavesMetadata.Add(metadata);
                UpdateSavesMetadata();
            }

            try
            {
                var json = JSONWriter.ToJson(Save);
                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static SaveFileMeta GetSingleplayerSaveMetadata()
        {
            if (LocalUserManager.readOnlyLocalUsersList.Count == 0)
            {
                return null;
            }
            var profile = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName.Replace(".xml", "");
            return SavesMetadata.FirstOrDefault(el => el.UserProfileId == profile && el.SteamIds.Length == 1);
        }

        private static SaveFileMeta GetLobbySaveMetadata()
        {
            var users = NetworkUser.readOnlyInstancesList.Select(el => el.Network_id.steamId.value).ToArray();
            if (users.Length == 0)
            {
                return null;
            }
            if (users.Length == 1)
            {
                return GetSingleplayerSaveMetadata();
            }
            return SavesMetadata.FirstOrDefault(el => el.SteamIds.Except(users).Count() == 0);
        }

        private void RegisterGameLoading()
        {
            //Replace with custom run load
            RegisterRunStartHook();

            //Restore team expirience
            On.RoR2.TeamManager.Start += (orig, self) =>
            {
                orig(self);
                if (IsLoading)
                {
                    Save.LoadTeam();
                    //This is last part of loading process
                    IsLoading = false;
                }
            };
        }

        private void RegisterGameSaving()
        {
            //Save game after stage is loaded
            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);

                if (FirstRunStage)
                {
                    FirstRunStage = false;
                    return;
                }
                Debug.Log("[ProperSave] Game Saved");
                SaveGame();
            };

            //Delete save file when run is over
            On.RoR2.GameOverController.Awake += (orig, self) =>
            {
                orig(self);

                try
                {
                    var metadata = Save?.SaveFileMeta;
                    if (metadata != null)
                    {
                        File.Delete(metadata.FilePath);
                        SavesMetadata.Remove(metadata);
                        UpdateSavesMetadata();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            };

            //Save stage RNG before it changed
            On.RoR2.Run.GenerateStageRNG += (orig, self) =>
            {
                PreStageRng = new RunRngData(Run.instance);
                orig(self);
            };
        }

        private void RegisterRunStartHook()
        {
            IL.RoR2.Run.Start += (il) =>
            {
                var c = new ILCursor(il);
                c.EmitDelegate<Func<bool>>(() =>
                {
                    FirstRunStage = true;
                    if (IsLoading)
                    {
                        Save.LoadRun();
                        Save.LoadArtifacts();
                        Save.LoadPlayers();
                    }
                    RunArtifactData = new ArtifactsData();

                    return IsLoading;
                });
                c.Emit(OpCodes.Brfalse, c.Next);
                c.Emit(OpCodes.Ret);
            };
        }

        private void RegisterSingleplayerContinueButton()
        {
            On.RoR2.UI.MainMenu.MainMenuController.Start += (orig, self) => {
                var singlePlayerButton = GameObject.Find("GenericMenuButton (Singleplayer)");
                var continueButton = Instantiate(singlePlayerButton, singlePlayerButton.transform.parent);
                ProperSave.singleplayerContinueButton = new WeakReference<GameObject>(continueButton);
                continueButton.name = "[PS] Continue";
                continueButton.transform.SetSiblingIndex(1);

                var buttonComponent = continueButton.GetComponent<HGButton>();
                buttonComponent.hoverToken = LanguageConsts.PS_TITLE_CONTINUE_DESC;

                var languageComponent = continueButton.GetComponent<LanguageTextMeshController>();
                languageComponent.token = LanguageConsts.PS_TITLE_CONTINUE;

                buttonComponent.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                buttonComponent.onClick.AddListener(() =>
                {
                    RoR2.Console.instance.SubmitCmd(null, "ps_load");
                });
                UpdateSingleplayerButton();

                orig(self);
            };

            On.RoR2.UI.MainMenu.ProfileMainMenuScreen.SetMainProfile += (orig, self, profile) =>
            {
                orig(self, profile);
                UpdateSingleplayerButton();
            };

        }

        private void RegisterLobbyContinueButton()
        {
            On.RoR2.PreGameController.Awake += (orig, self) =>
            {
                var quitButton = GameObject.Find("NakedButton (Quit)");
                var continueButton = Instantiate(quitButton, quitButton.transform.parent);
                ProperSave.lobbyContinueButton = new WeakReference<GameObject>(continueButton);
                continueButton.name = "[PS] Continue";
                continueButton.transform.SetSiblingIndex(1);
                var rectTransform = continueButton.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1F, 1.5F);
                rectTransform.anchorMax = new Vector2(1F, 1.5F);

                var buttonComponent = continueButton.GetComponent<HGButton>();
                buttonComponent.hoverToken = LanguageConsts.PS_TITLE_CONTINUE_DESC;

                var languageComponent = continueButton.GetComponent<LanguageTextMeshController>();
                languageComponent.token = LanguageConsts.PS_TITLE_CONTINUE;

                buttonComponent.onClick = new Button.ButtonClickedEvent();
                buttonComponent.onClick.AddListener(() =>
                {
                    RoR2.Console.instance.SubmitCmd(null, "ps_load_lobby");
                });
                UpdateLobbyButton();

                orig(self);
            };

            SteamworksLobbyManager.onLobbyOwnershipGained += () =>
            {
                UpdateLobbyButton();
            };
            SteamworksLobbyManager.onLobbyOwnershipLost += () =>
            {
                UpdateLobbyButton();
            };
            SteamworksLobbyManager.onLobbyChanged += () =>
            {
                UpdateLobbyButton();
            };
            SteamworksLobbyManager.onLobbyJoined += (obj) =>
            {
                UpdateLobbyButton();
            };
            SteamworksLobbyManager.onLobbyLeave += (obj) =>
            {
                UpdateLobbyButton();
            };
            SteamworksLobbyManager.onPlayerCountUpdated += () =>
            {
                UpdateLobbyButton();
            };
        }

        private void RegisterLanguage()
        {
            var flag = false;
            foreach (var file in Directory.GetFiles(ExecutingDirectory, "ps_*.json", SearchOption.AllDirectories))
            {
                flag = true;
                var languageToken = Regex.Match(file, ".+ps_(?<lang>[a-zA-Z]+).json\\Z").Groups["lang"].Value;
                var tokens = JSON.Parse(File.ReadAllText(file));

                if (languageToken == "en")
                {
                    foreach (var key in tokens.Keys)
                    {
                        LanguageAPI.Add(key, tokens[key].Value);
                    }
                }
                foreach (var key in tokens.Keys)
                {
                    LanguageAPI.Add(key, tokens[key].Value, languageToken);
                }
            }
            if (!flag)
            {
                Debug.LogWarning("Localizaiton files not found");
            }
        }

        #region Old TemporaryLunarCoins
        
        //Loads assembly only when method is called
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RegisterTLCOverride()
        {
            var tlcRunStart = typeof(TemporaryLunarCoins.TemporaryLunarCoins).GetMethod("Run_Start", BindingFlags.NonPublic | BindingFlags.Instance);
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(tlcRunStart, (Action<ILContext>)TLCHook);
        }

        //Hook to TemporaryLunarCoins Run.Start override and disable it when loading saved game
        private void TLCHook(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdarg(2),
                x => x.MatchCallvirt("On.RoR2.Run/orig_Start", "Invoke"));
            c.Index += 3;
            
            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(IsLoading), BindingFlags.NonPublic | BindingFlags.Static).GetMethod);
            c.Emit(OpCodes.Brfalse, c.Next);
            c.Emit(OpCodes.Ret);
        }

        #endregion

        private static void UpdateSingleplayerButton()
        {
            try
            {
                if (singleplayerContinueButton.TryGetTarget(out var button))
                {
                    var component = button?.GetComponent<HGButton>();
                    if (component != null)
                    {
                        component.interactable = File.Exists(GetSingleplayerSaveMetadata()?.FilePath);
                    }
                }
            }
            catch (Exception e) { }
        }

        private static void UpdateLobbyButton()
        {
            try
            {
                if (lobbyContinueButton.TryGetTarget(out var button))
                {
                    var component = button?.GetComponent<HGButton>();
                    if (component != null)
                    {
                        component.interactable = File.Exists(GetLobbySaveMetadata()?.FilePath);
                    }
                }
            }
            catch (Exception e) { }
        }

        private static IEnumerator LoadLobby()
        {
            if (PreGameController.instance == null)
            {
                Debug.Log("[ProperSave] PreGameController not found");
                yield break;
            }
            if (GameNetworkManager.singleton?.desiredHost.hostingParameters.listen == true && !SteamworksLobbyManager.ownsLobby)
            {
                Debug.Log("[ProperSave] Must be lobby leader to load game");
                yield break;
            }
            var metadata = GetLobbySaveMetadata();

            if (metadata == null)
            {
                Debug.Log("[ProperSave] Save for current users not found");
                yield break;
            }
            var filePath = metadata.FilePath;
            if (!File.Exists(filePath))
            {
                Debug.Log($"[ProperSave] File \"{filePath}\" not found");
                yield break;
            }

            IsLoading = true;
            var saveJSON = File.ReadAllText(filePath);
            Save = JSONParser.FromJson<SaveData>(saveJSON);
            Save.SaveFileMeta = metadata;

            PreGameController.instance?.StartLaunch();
        }

        [ConCommand(commandName = "ps_load_lobby", flags = ConVarFlags.None, helpText = "Load saved multiplayer game")]
        private static void CCRequestLoadLobby(ConCommandArgs args)
        {
            if (Run.instance != null)
            {
                Debug.Log("[ProperSave] Can't load while run is active");
                return;
            }
            if (IsLoading)
            {
                Debug.Log("[ProperSave] Already loading");
                return;
            }
            Instance.StartCoroutine(LoadLobby());
        }

        private static void UpdateSavesMetadata()
        {
            var path = $"{SavesDirectory}\\SavesMetadata.json";
            if (!Directory.Exists(SavesDirectory))
            {
                Directory.CreateDirectory(SavesDirectory);
            }

            try
            {
                File.WriteAllText(path, JSONWriter.ToJson(SavesMetadata));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProperSave] Can't update SavesMetadata file");
            }
        }

        private static void PopulateSavesMetadata()
        {
            var path = $"{SavesDirectory}\\SavesMetadata.json";
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                var metadata = JSONParser.FromJson<SaveFileMeta[]>(json);

                SavesMetadata.AddRange(metadata);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProperSave] SavesMetadata file corrupted.");
            }
        }
    }
}