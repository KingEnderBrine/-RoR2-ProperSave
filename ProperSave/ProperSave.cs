using BepInEx;
using ProperSave.Data;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.UI;
using SimpleJSON;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TinyJson;
using UnityEngine;

namespace ProperSave
{
    [R2APISubmoduleDependency("LanguageAPI", "CommandHelper")]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.ProperSave", "Proper Save", "1.0.0")]
    public class ProperSave : BaseUnityPlugin
    {
        private static WeakReference<GameObject> continueButton = new WeakReference<GameObject>(null);

        public static ProperSave Instance { get; set; }

        public static string SavesDirectory { get; } = Assembly.GetExecutingAssembly().Location.Replace("ProperSave.dll", "Saves");
        public static string LanguageDirectory { get; } = Assembly.GetExecutingAssembly().Location.Replace("ProperSave.dll", "Language");
        private static bool IsLoadingScene { get; set; }
        private static bool FirstRunStage { get; set; }
        private static SaveData Save { get; set; }

        public static RunRngData PreStageRng { get; set; }

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

            foreach (var file in Directory.GetFiles(LanguageDirectory, "ps_*.json"))
            {
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

            CommandHelper.AddToConsoleWhenReady();

            //Load players and theirs minions
            On.RoR2.SceneDirector.PopulateScene += (orig, self) => {
                try
                {
                    orig(self);
                }
                catch { }

                if (IsLoadingScene)
                {
                    Save.LoadPlayers();
                    IsLoadingScene = false;
                }
            };

            //Replace with custom run load
            On.RoR2.Run.Start += (orig, self) =>
            {
                FirstRunStage = true;
                if (IsLoadingScene)
                {
                    Save.LoadRun();
                    Save.LoadArtifacts();
                }
                else
                {
                    orig(self);
                }
            };

            //Restore team expirience
            On.RoR2.TeamManager.Start += (orig, self) =>
            {
                orig(self);
                if(IsLoadingScene)
                {
                    Save.LoadTeam();
                }
            };

            //Add "Continue" button to main menu
            On.RoR2.UI.MainMenu.MainMenuController.Start += (orig, self) => {
                var singlePlayerButton = GameObject.Find("GenericMenuButton (Singleplayer)");
                var continueButton = Instantiate(singlePlayerButton, singlePlayerButton.transform.parent);
                ProperSave.continueButton = new WeakReference<GameObject>(continueButton);
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
                buttonComponent.interactable = File.Exists(GetSavePath());

                orig(self);
            };

            On.RoR2.UI.MainMenu.ProfileMainMenuScreen.SetMainProfile += (orig, self, profile) =>
            {
                orig(self, profile);
                if (continueButton.TryGetTarget(out var button))
                {
                    button.GetComponent<HGButton>().interactable = File.Exists(GetSavePath());
                }
            };

            //Save game after stage is loaded
            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);
                if (GameNetworkManager.singleton.desiredHost.hostingParameters.listen)
                {
                    return;
                }
                if (FirstRunStage)
                {
                    FirstRunStage = false;
                    return;
                }

                SaveGame();
            };

            On.RoR2.GameOverController.Awake += (orig, self) =>
            {
                orig(self);
                if (GameNetworkManager.singleton.desiredHost.hostingParameters.listen)
                {
                    return;
                }
                File.Delete(GetSavePath());
            };

            On.RoR2.Run.GenerateStageRNG += (orig, self) =>
            {
                PreStageRng = new RunRngData(Run.instance);
                orig(self);
            };
        }

        [ConCommand(commandName = "ps_load", flags = ConVarFlags.None, helpText = "Load saved game")]
        private static void CCRequestLoad(ConCommandArgs args)
        {
            if (Run.instance != null)
            {
                Debug.Log("Can't load while run is active");
                return;
            }
            if (IsLoadingScene)
            {
                Debug.Log("Already loading");
                return;
            }

            Instance.StartCoroutine(LoadGame());
        }

        private static IEnumerator LoadGame()
        {
            string filePath = GetSavePath();

            if (!File.Exists(filePath))
            {
                Debug.Log($"File \"{filePath}\" not found");
                yield break;
            }

            IsLoadingScene = true;
            var saveJSON = File.ReadAllText(filePath);
            Save = JSONParser.FromJson<SaveData>(saveJSON);

            GameNetworkManager.singleton.desiredHost = new GameNetworkManager.HostDescription(new GameNetworkManager.HostDescription.HostingParameters
            {
                listen = false,
                maxPlayers = 4
            });
            yield return new WaitUntil(() => PreGameController.instance != null);
            PreGameController.instance?.StartLaunch();
        }

        private static void SaveGame()
        {
            string filePath = GetSavePath();

            var save = new SaveData();
            var json = JSONWriter.ToJson(save);

            File.WriteAllText(filePath, json);
        }

        public static NetworkUser GetPlayerFromUsername(string username)
        {
            foreach (var item in NetworkUser.readOnlyInstancesList)
            {
                if (username == item.userName)
                {
                    return item;
                }
            }

            return null;
        }

        private static string GetSavePath()
        {
            if (LocalUserManager.readOnlyLocalUsersList.Count == 0)
            {
                return null;
            }
            var profile = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName.Replace(".xml", "");
            if (!Directory.Exists(SavesDirectory))
            {
                Directory.CreateDirectory(SavesDirectory);
            }
            return $"{SavesDirectory}/{profile}.json";
        }
    }
}
