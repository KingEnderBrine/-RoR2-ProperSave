using BepInEx;
using BiggerBazaar;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Phedg1Studios.StartingItemsGUI;
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
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProperSave
{
    [R2APISubmoduleDependency("LanguageAPI", "CommandHelper")]

    //Support for both versions of TLC
    [BepInDependency("com.blazingdrummer.TemporaryLunarCoins", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.MagnusMagnuson.TemporaryLunarCoins", BepInDependency.DependencyFlags.SoftDependency)]

    //Support for StartingItemsGUI
    [BepInDependency("com.Phedg1Studios.StartingItemsGUI", BepInDependency.DependencyFlags.SoftDependency)]

    //Support for BiggerBazaar
    [BepInDependency("com.MagnusMagnuson.BiggerBazaar", BepInDependency.DependencyFlags.SoftDependency)]

    //Support for ShareSuit 
    [BepInDependency("com.funkfrog_sipondo.sharesuite", BepInDependency.DependencyFlags.SoftDependency)]

    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "2.3.2")]
    public class ProperSave : BaseUnityPlugin
    {
        private static WeakReference<GameObject> lobbyButton = new WeakReference<GameObject>(null);
        private static WeakReference<GameObject> lobbySubmenuLegend = new WeakReference<GameObject>(null);
        private static WeakReference<GameObject> lobbyGlyphAndDescription = new WeakReference<GameObject>(null);
        
        public static ProperSave Instance { get; private set; }

        public static bool IsTLCDefined { get; private set; }
        public static bool IsOldTLCDefined { get; private set; }
        public static bool IsSIGUIDefined { get; private set; }
        public static bool IsBBDefined { get; private set; }
        public static bool IsSSDefined { get; private set; }

        public static string ExecutingDirectory { get; } = Assembly.GetExecutingAssembly().Location.Replace("\\ProperSave.dll", "");
        public static string SavesDirectory { get; } = System.IO.Path.Combine(Application.persistentDataPath, "ProperSave", "Saves");
        public static bool IsLoading { get; private set; }
        public static bool FirstRunStage { get; private set; }
        private static SaveData Save { get; set; }
        private static List<SaveFileMeta> SavesMetadata { get; } = new List<SaveFileMeta>();

        public static RunRngData PreStageRng { get; private set; }
        public static RunArtifactsData RunArtifactData { get; private set; }

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
            IsTLCDefined = IsOldTLCDefined || BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.blazingdrummer.TemporaryLunarCoins");
            IsSIGUIDefined = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Phedg1Studios.StartingItemsGUI");
            IsBBDefined = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.MagnusMagnuson.BiggerBazaar");
            IsSSDefined = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.funkfrog_sipondo.sharesuite");

            CommandHelper.AddToConsoleWhenReady();

            if (IsOldTLCDefined)
            {
                try
                {
                    RegisterTLCOverride();
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to add support for TemporaryLunarCoins");
                    Logger.LogError(e);
                }
            }

            if (IsSIGUIDefined)
            {
                try
                {
                    RegisterSIGUIOverride();
                }
                catch(Exception e)
                {
                    Logger.LogError("Failed to add support for StartingItemsGUI");
                    Logger.LogError(e);
                }
            }

            if (IsBBDefined)
            {
                try
                {
                    RegisterBBOverride();
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to add support for BiggerBazaar");
                    Logger.LogError(e);
                }
            }

            RegisterGameLoading();

            RegisterGameSaving();

            RegisterLobbyButton();
        }

        #region Main registration
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

            RegisterLoopOnceHook();
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
                        Save = null;
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
                    RunArtifactData = new RunArtifactsData();

                    return IsLoading;
                });
                c.Emit(OpCodes.Brfalse, c.Next);
                c.Emit(OpCodes.Ret);
            };
        }

        //Disable LoopOnce achievement check if it's first loop.
        //It was triggered every time run is loaded because 
        //it checks for CurrentScene.stageOrder >= Run.instance.stageClearCount.
        //This is wrong because I first set stageClearCount, then check happens 
        //and only then CurrentScene and its stageOrder changes.
        private void RegisterLoopOnceHook()
        {
            IL.RoR2.Achievements.LoopOnceAchievement.Check += (il) =>
            {
                var c = new ILCursor(il);
                c.EmitDelegate<Func<bool>>(() =>
                {
                    return Run.instance?.loopClearCount == 0;
                });
                c.Emit(OpCodes.Brfalse, c.Next);
                c.Emit(OpCodes.Ret);
            };
        }
        #endregion

        #region Buttons
        private void RegisterLobbyButton()
        {
            On.RoR2.UI.CharacterSelectController.Awake += (orig, self) =>
            {
                try
                {
                    #region LoadButton
                    var quitButton = self.transform.GetChild(2).GetChild(4).GetChild(0).gameObject;
                    var loadButton = Instantiate(quitButton, quitButton.transform.parent);
                    lobbyButton = new WeakReference<GameObject>(loadButton);

                    foreach (var filter in self.GetComponents<InputSourceFilter>())
                    {
                        if (filter.requiredInputSource == MPEventSystem.InputSource.MouseAndKeyboard)
                        {
                            Array.Resize(ref filter.objectsToFilter, filter.objectsToFilter.Length + 1);
                            filter.objectsToFilter[filter.objectsToFilter.Length - 1] = loadButton;
                            break;
                        }
                    }

                    loadButton.name = "[ProperSave] Load";

                    var rectTransform = loadButton.GetComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(1F, 1.5F);
                    rectTransform.anchorMax = new Vector2(1F, 1.5F);

                    var buttonComponent = loadButton.GetComponent<HGButton>();
                    buttonComponent.hoverToken = LanguageConsts.PS_TITLE_CONTINUE_DESC;

                    var languageComponent = loadButton.GetComponent<LanguageTextMeshController>();
                    languageComponent.token = LanguageConsts.PS_TITLE_LOAD;

                    buttonComponent.onClick = new Button.ButtonClickedEvent();
                    buttonComponent.onClick.AddListener(() =>
                    {
                        RoR2.Console.instance.SubmitCmd(null, "ps_load_lobby");
                    });
                    #endregion

                    #region Load GlypAndDescription
                    var submenuLegend = self.transform.GetChild(2).GetChild(4).GetChild(1).gameObject;
                    var loadSubmenuLegend = Instantiate(submenuLegend, submenuLegend.transform.parent);
                    lobbySubmenuLegend = new WeakReference<GameObject>(loadSubmenuLegend);

                    foreach (var filter in self.GetComponents<InputSourceFilter>())
                    {
                        if (filter.requiredInputSource == MPEventSystem.InputSource.Gamepad)
                        {
                            Array.Resize(ref filter.objectsToFilter, filter.objectsToFilter.Length + 1);
                            filter.objectsToFilter[filter.objectsToFilter.Length - 1] = loadSubmenuLegend;
                            break;
                        }
                    }

                    loadSubmenuLegend.name = "[ProperSave] SubmenuLegend";

                    var uiJuiceComponent = loadSubmenuLegend.GetComponent<UIJuice>();
                    var enableEventComponent = loadSubmenuLegend.GetComponent<OnEnableEvent>();

                    enableEventComponent.action.RemoveAllListeners();
                    enableEventComponent.action.AddListener(new UnityEngine.Events.UnityAction(uiJuiceComponent.TransitionPanFromTop));
                    enableEventComponent.action.AddListener(new UnityEngine.Events.UnityAction(uiJuiceComponent.TransitionAlphaFadeIn));

                    var rectTransformComponent = loadSubmenuLegend.GetComponent<RectTransform>();
                    rectTransformComponent.anchorMin = new Vector2(1, 1);
                    rectTransformComponent.anchorMax = new Vector2(1, 2);

                    var glyphAndDescription = loadSubmenuLegend.transform.GetChild(0);
                    lobbyGlyphAndDescription = new WeakReference<GameObject>(glyphAndDescription.gameObject);

                    var glyph = glyphAndDescription.GetChild(0).GetComponent<InputBindingDisplayController>();
                    glyph.actionName = "UISubmenuUp";

                    var description = glyphAndDescription.GetChild(1).GetComponent<LanguageTextMeshController>();
                    description.token = LanguageConsts.PS_TITLE_LOAD;

                    for (var i = 1; i < loadSubmenuLegend.transform.childCount; i++)
                    {
                        Destroy(loadSubmenuLegend.transform.GetChild(i).gameObject);
                    }
                    #endregion

                    UpdateLobbyControls();

                    void GamepadInputEvent()
                    {
                        RoR2.Console.instance?.SubmitCmd(null, "ps_load_lobby");
                    }

                    var gamepadInputEvent = self.gameObject.AddComponent<HGGamepadInputEvent>();
                    gamepadInputEvent.actionName = "UISubmenuUp";
                    gamepadInputEvent.enabledObjectsIfActive = new GameObject[0];

                    gamepadInputEvent.actionEvent = new UnityEngine.Events.UnityEvent();
                    gamepadInputEvent.actionEvent.AddListener(new UnityEngine.Events.UnityAction(GamepadInputEvent));
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
                orig(self);
            };

            NetworkUser.OnPostNetworkUserStart += (user) =>
            {
                UpdateLobbyControls();
            };

            NetworkUser.onNetworkUserLost += (user) =>
            {
                UpdateLobbyControls(user);
            };
        }

        private static void UpdateLobbyControls(NetworkUser exceptUser = null)
        {
            var interactable = 
                SteamworksLobbyManager.isInLobby == SteamworksLobbyManager.ownsLobby &&
                File.Exists(GetLobbySaveMetadata(exceptUser)?.FilePath);
            try
            {
                if (lobbyButton.TryGetTarget(out var button))
                {
                    var component = button?.GetComponent<HGButton>();
                    if (component != null)
                    {
                        component.interactable = interactable;
                    }
                }
            }
            catch { }
            try
            {
                if (lobbyGlyphAndDescription.TryGetTarget(out var glyphAndDescription))
                {
                    var color = interactable ? Color.white : new Color(0.3F, 0.3F, 0.3F);
                    
                    var glyphText = glyphAndDescription.transform.GetChild(0).GetComponent<HGTextMeshProUGUI>();
                    glyphText.color = color;

                    var descriptionText = glyphAndDescription.transform.GetChild(1).GetComponent<HGTextMeshProUGUI>();
                    descriptionText.color = color;
                }
            }
            catch { }
        }
        #endregion

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
            
            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(IsLoading), BindingFlags.Public | BindingFlags.Static).GetMethod);
            c.Emit(OpCodes.Brfalse, c.Next);
            c.Emit(OpCodes.Ret);
        }

        #endregion

        #region StartingItemGUI
        //Loads assembly only when method is called
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RegisterSIGUIOverride()
        {
            var tlcRunStart = typeof(StartingItemsGUI).GetMethod("<Start>b__4_2", BindingFlags.NonPublic | BindingFlags.Instance);
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(tlcRunStart, (Action<ILContext>)SIGUIHook);
        }

        //Hook to StartingItemGUI Start/RoR2.Run.onRunStartGlobal override and disable adding items when loading saved game
        private void SIGUIHook(ILContext il)
        {
            var c = new ILCursor(il);
            ILLabel retLabel = null;
            c.GotoNext(
                x => x.MatchCall(typeof(NetworkClient), "get_active"),
                x => x.MatchStloc(7),
                x => x.MatchLdloc(7),
                x => x.MatchBrfalse(out retLabel));
            c.Index += 4;
            
            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(IsLoading), BindingFlags.Public | BindingFlags.Static).GetMethod);
            c.Emit(OpCodes.Brtrue, retLabel);
        }
        #endregion

        #region BiggerBazaar
        //Loads assembly only when method is called
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RegisterBBOverride()
        {
            var bbRunAdvanceStage = typeof(BiggerBazaar.BiggerBazaar).Assembly.GetType("BiggerBazaar.Bazaar").GetMethod("StartBazaar", BindingFlags.Public | BindingFlags.Instance);
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(bbRunAdvanceStage, (Action<ILContext>)BBHook);
        }

        private static void BBHook(ILContext il)
        {
            var bazaar = typeof(BiggerBazaar.BiggerBazaar).Assembly.GetType("BiggerBazaar.Bazaar");
            var c = new ILCursor(il);
            c.Index++;
            var next = c.Next;

            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(FirstRunStage)).GetGetMethod());
            c.Emit(OpCodes.Brfalse, next);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, bazaar.GetMethod("ResetBazaarPlayers"));
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, bazaar.GetMethod("CalcDifficultyCoefficient"));
        }
        #endregion

        #region Loading and saving
        private static void SaveGame()
        {
            Save = new SaveData();
            var metadata = SaveFileMeta.CreateCurrentMetadata();
            var foundMetadata = GetLobbySaveMetadata();

            Save.SaveFileMeta = foundMetadata ?? metadata;
            if (string.IsNullOrEmpty(Save.SaveFileMeta.FileName))
            {
                do
                {
                    Save.SaveFileMeta.FileName = Guid.NewGuid().ToString();
                }
                while (File.Exists(Save.SaveFileMeta.FilePath));
            }

            try
            {
                var json = JSONWriter.ToJson(Save);
                File.WriteAllText(Save.SaveFileMeta.FilePath, json);
                if (ReferenceEquals(Save.SaveFileMeta, metadata))
                {
                    SavesMetadata.Add(metadata);
                    UpdateSavesMetadata();
                }
                Chat.AddMessage(Language.GetString(LanguageConsts.PS_CHAT_SAVE));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProperSave] Couldn't save game");
            }
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

        [ConCommand(commandName = "ps_load_lobby", flags = ConVarFlags.None, helpText = "Load saved game suitable for current lobby")]
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
        #endregion

        #region SavesMetadata
        private static SaveFileMeta GetLobbySaveMetadata(NetworkUser exceptUser = null)
        {
            var users = NetworkUser.readOnlyInstancesList.Select(el => el.Network_id.steamId.value).ToList();
            if (exceptUser != null)
            {
                users.Remove(exceptUser.Network_id.steamId.value);
            }
            var usersCount = users.Count();
            if (usersCount == 0)
            {
                return null;
            }
            var gameMode = PreGameController.instance ? PreGameController.instance.gameModeIndex : Run.instance ? Run.instance.gameModeIndex : GameModeIndex.Invalid;
            if (gameMode == GameModeIndex.Invalid)
            {
                return null;
            }
            if (usersCount == 1)
            {
                var profile = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName.Replace(".xml", "");
                return SavesMetadata.FirstOrDefault(el => el.UserProfileId == profile && el.SteamIds.Length == 1 && el.GameMode == gameMode);
            }
            return SavesMetadata.FirstOrDefault(el => el.SteamIds.DifferenceCount(users) == 0 && el.GameMode == gameMode);
        }

        private static void PopulateSavesMetadata()
        {
            if (!Directory.Exists(SavesDirectory))
            {
                Directory.CreateDirectory(SavesDirectory);
                return;
            }
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
        #endregion
    }
}