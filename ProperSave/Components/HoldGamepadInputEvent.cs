using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ProperSave.Components
{
    public class HoldGamepadInputEvent : HGGamepadInputEvent
    {
        public UnityEvent holdStartEvent = new UnityEvent();
        public UnityEvent holdEndEvent = new UnityEvent();

        protected new void Update()
        {
            bool flag = CanAcceptInput();
            if (couldAcceptInput != flag)
            {
                foreach (var gameObject in enabledObjectsIfActive)
                    gameObject.SetActive(flag);
            }
            if (CanAcceptInput())
            {
                if (eventSystem.player.GetButtonShortPressDown(actionName))
                {
                    holdStartEvent?.Invoke();
                }
                else if (eventSystem.player.GetButtonShortPressUp(actionName))
                {
                    holdEndEvent?.Invoke();
                }
                else if (eventSystem.player.GetButtonUp(actionName))
                {
                    actionEvent?.Invoke();
                }
            }
            couldAcceptInput = flag;
        }
    }
}
