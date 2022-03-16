using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProperSave
{
    internal static class ModSupport
    {
        public const string BiggerBazaarGUID = "com.MagnusMagnuson.BiggerBazaar";
        public const string ShareSuiteGUID = "com.funkfrog_sipondo.sharesuite";

        public static bool IsBBLoaded { get; private set; }
        public static bool IsSSLoaded { get; private set; }

        private static Dictionary<MethodInfo, Action<ILContext>> RegisteredILHooks { get; } = new Dictionary<MethodInfo, Action<ILContext>>();

        public static void GatherLoadedPlugins()
        {
            IsBBLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(BiggerBazaarGUID);
            IsSSLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ShareSuiteGUID);
        }

        public static void RegisterHooks()
        {
            if (IsBBLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(BiggerBazaarGUID))
            {
                try
                {
                    RegisterBBHooks();
                }
                catch (Exception e)
                {
                    ProperSavePlugin.InstanceLogger.LogError("Failed to add support for BiggerBazaar");
                    ProperSavePlugin.InstanceLogger.LogError(e);
                }
            }
        }

        public static void UnregisterHooks()
        {
            foreach (var row in RegisteredILHooks)
            {
                MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Unmodify(row.Key, row.Value);
            }
        }

        #region BiggerBazaar
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterBBHooks()
        {
            var bbRunAdvanceStage = typeof(BiggerBazaar.BiggerBazaar).Assembly.GetType("BiggerBazaar.Bazaar").GetMethod("StartBazaar", BindingFlags.Public | BindingFlags.Instance);
            var methodDelegate = (Action<ILContext>)BBHook;
            
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(bbRunAdvanceStage, methodDelegate);
            RegisteredILHooks.Add(bbRunAdvanceStage, methodDelegate);
        }

        private static void BBHook(ILContext il)
        {
            var bazaar = typeof(BiggerBazaar.BiggerBazaar).Assembly.GetType("BiggerBazaar.Bazaar");
            var c = new ILCursor(il);
            c.Index++;
            var next = c.Next;

            c.Emit(OpCodes.Call, typeof(Loading).GetProperty(nameof(Loading.FirstRunStage)).GetGetMethod());
            c.Emit(OpCodes.Brfalse, next);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, bazaar.GetMethod("ResetBazaarPlayers"));
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, bazaar.GetMethod("CalcDifficultyCoefficient"));
        }
        #endregion
        
        #region ShareSuite
        public static void LoadShareSuiteMoney(uint money)
        {
            if (IsSSLoaded)
            {
                ProperSavePlugin.Instance.StartCoroutine(LoadShareSuiteMoneyInternal(money));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerator LoadShareSuiteMoneyInternal(uint money)
        {
            yield return new WaitUntil(() => !ShareSuite.MoneySharingHooks.MapTransitionActive);
            ShareSuite.MoneySharingHooks.SharedMoneyValue = (int)money;
        }

        public static void ShareSuiteMapTransition()
        {
            if (IsSSLoaded)
            {
                ShareSuiteMapTransionInternal();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ShareSuiteMapTransionInternal()
        {
            ShareSuite.MoneySharingHooks.MapTransitionActive = true;
        }
        #endregion
    }
}
