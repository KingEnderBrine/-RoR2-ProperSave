using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProperSave.Components
{
    public class GamepadTooltipProvider : TooltipProvider
    {
        public HoldGamepadInputEvent inputEvent;
        private RectTransform rectTransform;

        public bool offsetWidth;
        public bool offsetHeight;

        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Start()
        {
            inputEvent.holdStartEvent.AddListener(HoldStart);
            inputEvent.holdEndEvent.AddListener(HoldEnd);
        }

        private void HoldStart()
        {
            MPEventSystem current = EventSystem.current as MPEventSystem;
            if (current == null || !tooltipIsAvailable)
            {
                return;
            }

            TooltipController.SetTooltip(current, this, new Vector2(int.MaxValue, 0));
            if (current.currentTooltip && current.currentTooltipProvider == this)
            {
                current.currentTooltip.owner = null;
                current.currentTooltip.tooltipCenterTransform.position = (current.currentTooltip.uiCamera?.camera ?? Camera.main).WorldToScreenPoint(rectTransform.position);
            }
        }

        private void HoldEnd()
        {
            MPEventSystem current = EventSystem.current as MPEventSystem;
            if (current == null || !tooltipIsAvailable)
            {
                return;
            }
            TooltipController.RemoveTooltip(current, this);
        }
    }
}
