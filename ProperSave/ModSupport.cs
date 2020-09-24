using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProperSave
{
    internal static class ModSupport
    {
        public const string TemporaryLunarCoinsGUID = "com.MagnusMagnuson.TemporaryLunarCoins";
        public const string BDTemporaryLunarCoinsGUID = "com.blazingdrummer.TemporaryLunarCoins";
        public const string StartingItemsGUIGUID = "com.Phedg1Studios.StartingItemsGUI";
        public const string BiggerBazaarGUID = "com.MagnusMagnuson.BiggerBazaar";
        public const string ShareSuiteGUID = "com.funkfrog_sipondo.sharesuite";

        public static bool IsBDTLCLoaded { get; private set; }
        public static bool IsTLCLoaded { get; private set; }
        public static bool IsSIGUILoaded { get; private set; }
        public static bool IsBBLoaded { get; private set; }
        public static bool IsSSLoaded { get; private set; }

        private static bool AreHooksRegistered { get; set; }
        private static Dictionary<MethodInfo, Action<ILContext>> RegisteredILHooks { get; } = new Dictionary<MethodInfo, Action<ILContext>>();

        public static void GatherLoadedPlugins()
        {
            IsTLCLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(TemporaryLunarCoinsGUID);
            IsSIGUILoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(StartingItemsGUIGUID);
            IsBBLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(BiggerBazaarGUID);
            IsBDTLCLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(BDTemporaryLunarCoinsGUID);
            IsSSLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ShareSuiteGUID);
        }

        public static void RegisterHooks()
        {
            if (AreHooksRegistered)
            {
                return;
            }
            if (IsTLCLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(TemporaryLunarCoinsGUID))
            {
                try
                {
                    RegisterTLCHooks();
                }
                catch (Exception e)
                {
                    ProperSave.InstanceLogger.LogWarning("Failed to add support for TemporaryLunarCoins");
                    ProperSave.InstanceLogger.LogError(e);
                }
            }
            if (IsSIGUILoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(StartingItemsGUIGUID))
            {
                try
                {
                    RegisterSIGUIHooks();
                }
                catch (Exception e)
                {
                    ProperSave.InstanceLogger.LogWarning("Failed to add support for StartingItemsGUI");
                    ProperSave.InstanceLogger.LogError(e);
                }
            }
            if (IsBBLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(BiggerBazaarGUID))
            {
                try
                {
                    RegisterBBHooks();
                }
                catch (Exception e)
                {
                    ProperSave.InstanceLogger.LogError("Failed to add support for BiggerBazaar");
                    ProperSave.InstanceLogger.LogError(e);
                }
            }
            AreHooksRegistered = true;
        }

        public static void UnregisterHooks()
        {
            if (!AreHooksRegistered)
            {
                return;
            }

            foreach (var row in RegisteredILHooks)
            {
                MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Unmodify(row.Key, row.Value);
            }

            AreHooksRegistered = false;
        }

        #region TemporaryLunarCoins

        //Loads assembly only when method is called
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RegisterTLCHooks()
        {
            var tlcRunStart = typeof(TemporaryLunarCoins.TemporaryLunarCoins).GetMethod("Run_Start", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodDelegate = (Action<ILContext>)TLCHook;
            
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(tlcRunStart, methodDelegate);
            RegisteredILHooks.Add(tlcRunStart, methodDelegate);
        }

        //Hook to TemporaryLunarCoins Run.Start override and disable it when loading saved game
        private static void TLCHook(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdarg(2),
                x => x.MatchCallvirt("On.RoR2.Run/orig_Start", "Invoke"));
            c.Index += 3;

            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(ProperSave.IsLoading), BindingFlags.Public | BindingFlags.Static).GetMethod);
            c.Emit(OpCodes.Brfalse, c.Next);
            c.Emit(OpCodes.Ret);
        }

        #endregion

        #region StartingItemGUI
        //Loads assembly only when method is called
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RegisterSIGUIHooks()
        {
            var siguiOnRunStartGlobal = typeof(Phedg1Studios.StartingItemsGUI.StartingItemsGUI).GetMethod("OnRunStartGlobal", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodDelegate = (Action<ILContext>)SIGUIHook;
            
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(siguiOnRunStartGlobal, methodDelegate);
            RegisteredILHooks.Add(siguiOnRunStartGlobal, methodDelegate);
        }

        //Hook to StartingItemGUI Start/RoR2.Run.onRunStartGlobal override and disable adding items when loading saved game
        private static void SIGUIHook(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(Phedg1Studios.StartingItemsGUI.StartingItemsGUI), "SetLocalUsers"));

            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(ProperSave.IsLoading), BindingFlags.Public | BindingFlags.Static).GetMethod);
            c.Emit(OpCodes.Brfalse, c.Next);
            c.Emit(OpCodes.Ret);
        }
        #endregion

        #region BiggerBazaar
        //Loads assembly only when method is called
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
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

            c.Emit(OpCodes.Call, typeof(ProperSave).GetProperty(nameof(ProperSave.FirstRunStage)).GetGetMethod());
            c.Emit(OpCodes.Brfalse, next);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, bazaar.GetMethod("ResetBazaarPlayers"));
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, bazaar.GetMethod("CalcDifficultyCoefficient"));
        }
        #endregion
    }
}
