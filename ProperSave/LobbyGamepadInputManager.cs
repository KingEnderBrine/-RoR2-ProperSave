using R2API.Utils;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProperSave
{
    [RequireComponent(typeof(CharacterSelectController))]
    public class LobbyGamepadInputManager : MonoBehaviour
    {
        private CharacterSelectController characterSelectController;
        public void Awake()
        {
            characterSelectController = GetComponent<CharacterSelectController>();
        }

        public void Update()
        {
            var eventSystem = characterSelectController.GetFieldValue<MPEventSystem>("eventSystem");
            if (eventSystem.currentInputSource != MPEventSystem.InputSource.Gamepad)
            {
                return;
            }
            if (!eventSystem.player.GetButtonDown(RewiredConsts.Action.UISubmenuUp))
            {
                return;
            }

            RoR2.Console.instance.SubmitCmd(null, "ps_load");
        }
    }
}
