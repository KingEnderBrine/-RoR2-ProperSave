using ProperSave.Components;
using ProperSave.SaveData;
using PSTinyJson;
using RoR2;
using RoR2.UI;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ProperSave
{
    internal static class LobbyUI
    {
        private static GameObject lobbyButton;
        private static GameObject lobbySubmenuLegend;
        private static GameObject lobbyGlyphAndDescription;
        private static TooltipProvider tooltipProvider;
        private static GamepadTooltipProvider gamepadTooltipProvider;

        #region Buttons
        public static void RegisterHooks()
        {
            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectControllerAwake;
            NetworkUser.onPostNetworkUserStart += NetworkUserOnPostNetworkUserStart;
            NetworkUser.onNetworkUserLost += NetworkUserOnNetworkUserLost;
        }

        public static void UnregisterHooks()
        {
            On.RoR2.UI.CharacterSelectController.Awake -= CharacterSelectControllerAwake;
            NetworkUser.onPostNetworkUserStart -= NetworkUserOnPostNetworkUserStart;
            NetworkUser.onNetworkUserLost -= NetworkUserOnNetworkUserLost;
        }

        private static void NetworkUserOnNetworkUserLost(NetworkUser networkUser)
        {
            UpdateLobbyControls(networkUser);
        }

        private static void NetworkUserOnPostNetworkUserStart(NetworkUser networkUser)
        {
            UpdateLobbyControls();
        }

        private static void CharacterSelectControllerAwake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, CharacterSelectController self)
        {
            try
            {
                #region LoadButton
                var quitButton = self.transform.GetChild(2).GetChild(4).GetChild(0).gameObject;
                lobbyButton = GameObject.Instantiate(quitButton, quitButton.transform.parent);

                foreach (var filter in self.GetComponents<InputSourceFilter>())
                {
                    if (filter.requiredInputSource == MPEventSystem.InputSource.MouseAndKeyboard)
                    {
                        Array.Resize(ref filter.objectsToFilter, filter.objectsToFilter.Length + 1);
                        filter.objectsToFilter[filter.objectsToFilter.Length - 1] = lobbyButton;
                        break;
                    }
                }

                lobbyButton.name = "[ProperSave] Load";

                tooltipProvider = lobbyButton.AddComponent<TooltipProvider>();

                var rectTransform = lobbyButton.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1F, 1.5F);
                rectTransform.anchorMax = new Vector2(1F, 1.5F);

                var buttonComponent = lobbyButton.GetComponent<HGButton>();
                buttonComponent.hoverToken = LanguageConsts.PROPER_SAVE_TITLE_CONTINUE_DESC;

                var languageComponent = lobbyButton.GetComponent<LanguageTextMeshController>();
                languageComponent.token = LanguageConsts.PROPER_SAVE_TITLE_LOAD;

                buttonComponent.onClick = new Button.ButtonClickedEvent();
                buttonComponent.onClick.AddListener(LoadOnInputEvent);
                #endregion

                #region Load GlypAndDescription
                var submenuLegend = self.transform.GetChild(2).GetChild(4).GetChild(1).gameObject;
                lobbySubmenuLegend = GameObject.Instantiate(submenuLegend, submenuLegend.transform.parent);

                foreach (var filter in self.GetComponents<InputSourceFilter>())
                {
                    if (filter.requiredInputSource == MPEventSystem.InputSource.Gamepad)
                    {
                        Array.Resize(ref filter.objectsToFilter, filter.objectsToFilter.Length + 1);
                        filter.objectsToFilter[filter.objectsToFilter.Length - 1] = lobbySubmenuLegend;
                        break;
                    }
                }

                lobbySubmenuLegend.name = "[ProperSave] SubmenuLegend";

                var uiJuiceComponent = lobbySubmenuLegend.GetComponent<UIJuice>();
                var enableEventComponent = lobbySubmenuLegend.GetComponent<OnEnableEvent>();

                enableEventComponent.action.RemoveAllListeners();
                enableEventComponent.action.AddListener(uiJuiceComponent.TransitionPanFromTop);
                enableEventComponent.action.AddListener(uiJuiceComponent.TransitionAlphaFadeIn);

                var rectTransformComponent = lobbySubmenuLegend.GetComponent<RectTransform>();
                rectTransformComponent.anchorMin = new Vector2(1, 1);
                rectTransformComponent.anchorMax = new Vector2(1, 2);

                lobbyGlyphAndDescription = lobbySubmenuLegend.transform.GetChild(0).gameObject;

                var glyph = lobbyGlyphAndDescription.transform.GetChild(0).GetComponent<InputBindingDisplayController>();
                glyph.actionName = "UISubmenuUp";

                var description = lobbyGlyphAndDescription.transform.GetChild(1).GetComponent<LanguageTextMeshController>();
                description.token = LanguageConsts.PROPER_SAVE_TITLE_LOAD;

                for (var i = 1; i < lobbySubmenuLegend.transform.childCount; i++)
                {
                    GameObject.Destroy(lobbySubmenuLegend.transform.GetChild(i).gameObject);
                }
                #endregion

                UpdateLobbyControls();

                var gamepadInputEvent = self.gameObject.AddComponent<HoldGamepadInputEvent>();
                gamepadInputEvent.actionName = "UISubmenuUp";
                gamepadInputEvent.enabledObjectsIfActive = Array.Empty<GameObject>();

                gamepadInputEvent.actionEvent = new UnityEngine.Events.UnityEvent();
                gamepadInputEvent.actionEvent.AddListener(LoadOnInputEvent);

                gamepadTooltipProvider = glyph.gameObject.AddComponent<GamepadTooltipProvider>();
                gamepadTooltipProvider.inputEvent = gamepadInputEvent;
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Failed while adding lobby buttons");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
            orig(self);
        }

        private static void LoadOnInputEvent()
        {
            if (Run.instance != null)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("Can't load while run is active");
                return;
            }
            if (Loading.IsLoading)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("Already loading");
                return;
            }
            ProperSavePlugin.Instance.StartCoroutine(Loading.LoadLobby());
        }

        private static void UpdateLobbyControls(NetworkUser exceptUser = null)
        {
            var metadata = SaveFileMetadata.GetCurrentLobbySaveMetadata(exceptUser);
            var interactable =
                PlatformSystems.lobbyManager.isInLobby == PlatformSystems.lobbyManager.ownsLobby &&
                File.Exists(metadata?.FilePath);
            var tooltipContent = metadata == null ? default : new TooltipContent
            {
                titleToken = LanguageConsts.PROPER_SAVE_TOOLTIP_LOAD_TITLE,
                overrideBodyText = GetSaveDescription(metadata),
                titleColor = Color.black,
                disableBodyRichText = false
            };

            try
            {
                if (lobbyButton)
                {
                    var component = lobbyButton.GetComponent<HGButton>();
                    if (component)
                    {
                        component.interactable = interactable;
                    }
                }
                if (tooltipProvider)
                {
                    tooltipProvider.SetContent(tooltipContent);
                }
            }
            catch { }
            try
            {
                if (lobbyGlyphAndDescription)
                {
                    var color = interactable ? Color.white : new Color(0.3F, 0.3F, 0.3F);

                    var glyphText = lobbyGlyphAndDescription.transform.GetChild(0).GetComponent<HGTextMeshProUGUI>();
                    glyphText.color = color;

                    var descriptionText = lobbyGlyphAndDescription.transform.GetChild(1).GetComponent<HGTextMeshProUGUI>();
                    descriptionText.color = color;
                }
                if (gamepadTooltipProvider)
                {
                    gamepadTooltipProvider.SetContent(tooltipContent);
                }
            }
            catch { }
        }

        private static string GetSaveDescription(SaveFileMetadata saveMetadata)
        {
            var saveJSON = File.ReadAllText(saveMetadata.FilePath);
            var save = JSONParser.FromJson<SaveFile>(saveJSON);

            var builder = new StringBuilder();
            foreach (var playerData in save.PlayersData)
            {
                var networkUser = NetworkUser.readOnlyInstancesList.FirstOrDefault(user => playerData.userId.Load().Equals(user.id));
                var body = BodyCatalog.FindBodyPrefab(playerData.characterBodyName);
                var survivor = SurvivorCatalog.FindSurvivorDefFromBody(body);
                builder.Append(Language.GetStringFormatted(LanguageConsts.PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_CHARACTER, networkUser?.userName, survivor != null ? Language.GetString(survivor.displayNameToken) : ""));
            }

            var stage = SceneCatalog.GetSceneDefFromSceneName(save.RunData.sceneName);
            var difficulty = DifficultyCatalog.GetDifficultyDef((DifficultyIndex)save.RunData.difficulty);
            var time = save.RunData.isPaused ? (int)save.RunData.offsetFromFixedTime : (int)(save.RunData.fixedTime + save.RunData.offsetFromFixedTime);

            return Language.GetStringFormatted(
                LanguageConsts.PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_BODY,
                builder.ToString(),
                stage ? Language.GetString(stage.nameToken) : "",
                (save.RunData.stageClearCount + 1).ToString(),
                $"{(time / 60):00}:{(time % 60):00}",
                difficulty != null ? Language.GetString(difficulty.nameToken) : "",
                save.ContentHash != null && save.ContentHash != ProperSavePlugin.ContentHash ? Language.GetString(LanguageConsts.PROPER_SAVE_TOOLTIP_LOAD_CONTENT_MISMATCH) : "");
        }
        #endregion
    }
}
