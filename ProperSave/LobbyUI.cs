using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.IO;
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

        #region Buttons
        public static void RegisterHooks()
        {
            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectControllerAwake;
            NetworkUser.OnPostNetworkUserStart += NetworkUserOnPostNetworkUserStart;
            NetworkUser.onNetworkUserLost += NetworkUserOnNetworkUserLost;
        }

        public static void UnregisterHooks()
        {
            On.RoR2.UI.CharacterSelectController.Awake -= CharacterSelectControllerAwake;
            NetworkUser.OnPostNetworkUserStart -= NetworkUserOnPostNetworkUserStart;
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

                var rectTransform = lobbyButton.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1F, 1.5F);
                rectTransform.anchorMax = new Vector2(1F, 1.5F);

                var buttonComponent = lobbyButton.GetComponent<HGButton>();
                buttonComponent.hoverToken = LanguageConsts.PS_TITLE_CONTINUE_DESC;

                var languageComponent = lobbyButton.GetComponent<LanguageTextMeshController>();
                languageComponent.token = LanguageConsts.PS_TITLE_LOAD;

                buttonComponent.onClick = new Button.ButtonClickedEvent();
                buttonComponent.onClick.AddListener(LoadOnInputEvent);
                #endregion

                #region Load GlypAndDescription
                var submenuLegend = self.transform.GetChild(2).GetChild(4).GetChild(1).gameObject;
                var lobbySubmenuLegend = GameObject.Instantiate(submenuLegend, submenuLegend.transform.parent);

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

                var lobbyGlyphAndDescription = lobbySubmenuLegend.transform.GetChild(0);

                var glyph = lobbyGlyphAndDescription.GetChild(0).GetComponent<InputBindingDisplayController>();
                glyph.actionName = "UISubmenuUp";

                var description = lobbyGlyphAndDescription.GetChild(1).GetComponent<LanguageTextMeshController>();
                description.token = LanguageConsts.PS_TITLE_LOAD;

                for (var i = 1; i < lobbySubmenuLegend.transform.childCount; i++)
                {
                    GameObject.Destroy(lobbySubmenuLegend.transform.GetChild(i).gameObject);
                }
                #endregion

                UpdateLobbyControls();

                var gamepadInputEvent = self.gameObject.AddComponent<HGGamepadInputEvent>();
                gamepadInputEvent.actionName = "UISubmenuUp";
                gamepadInputEvent.enabledObjectsIfActive = Array.Empty<GameObject>();

                gamepadInputEvent.actionEvent = new UnityEngine.Events.UnityEvent();
                gamepadInputEvent.actionEvent.AddListener(LoadOnInputEvent);
            }
            catch (Exception e)
            {
                ProperSave.InstanceLogger.LogWarning("Failed while adding lobby buttons");
                ProperSave.InstanceLogger.LogError(e);
            }
            orig(self);
        }

        private static void LoadOnInputEvent()
        {
            RoR2.Console.instance?.SubmitCmd(null, "ps_load_lobby");
        }

        private static void UpdateLobbyControls(NetworkUser exceptUser = null)
        {
            var interactable =
                SteamworksLobbyManager.isInLobby == SteamworksLobbyManager.ownsLobby &&
                File.Exists(ProperSave.GetLobbySaveMetadata(exceptUser)?.FilePath);
            try
            {
                if (lobbyButton)
                {
                    var component = lobbyButton?.GetComponent<HGButton>();
                    if (component)
                    {
                        component.interactable = interactable;
                    }
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
            }
            catch { }
        }
        #endregion
    }
}
