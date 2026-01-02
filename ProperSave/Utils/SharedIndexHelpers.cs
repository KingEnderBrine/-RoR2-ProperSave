using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using RoR2;
using RoR2.Stats;

namespace ProperSave.Utils
{
    public static class SharedIndexHelpers
    {
        public static int FromDifficulty(DifficultyIndex difficultyIndex, WriterContext context)
        {
            if (difficultyIndex == DifficultyIndex.Invalid)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)difficultyIndex;
            }

            var def = DifficultyCatalog.GetDifficultyDef(difficultyIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.nameToken);

            return index;
        }

        public static DifficultyIndex ResolveDifficulty(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return DifficultyIndex.Invalid;
            }

            if (!context.Resilient)
            {
                return (DifficultyIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var index = CatalogHelpers.FindDifficultyIndex(name);
            if (index == DifficultyIndex.Invalid)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find difficulty \"{name}\" in DifficultyCatalog");
                return DifficultyIndex.Invalid;
            }

            return index;
        }

        public static int FromRule(int ruleIndex, WriterContext context)
        {
            if (!context.Resilient || ruleIndex == -1)
            {
                return ruleIndex;
            }

            var ruleDef = RuleCatalog.GetRuleDef(ruleIndex);
            var index = context.SharedStrings.AddOrIndexOf(ruleDef.globalName);

            return index;
        }

        public static int ResolveRule(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var def = RuleCatalog.FindRuleDef(name);
            if (def == null)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find rule \"{name}\" in RuleCatalog");
                return -1;
            }

            return def.globalIndex;
        }

        public static int FromArtifact(ArtifactIndex artifactIndex, WriterContext context)
        {
            if (artifactIndex == ArtifactIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)artifactIndex;
            }

            var def = ArtifactCatalog.GetArtifactDef(artifactIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.cachedName);

            return index;
        }

        public static ArtifactIndex ResolveArtifact(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return ArtifactIndex.None;
            }

            if (!context.Resilient)
            {
                return (ArtifactIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var def = ArtifactCatalog.FindArtifactDef(name);
            if (def == null)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find artifact \"{name}\" in ArtifactCatalog");
                return ArtifactIndex.None;
            }

            return def.artifactIndex;
        }

        public static int FromBody(BodyIndex bodyIndex, WriterContext context)
        {
            if (bodyIndex == BodyIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)bodyIndex;
            }

            var name = BodyCatalog.GetBodyName(bodyIndex);
            var index = context.SharedStrings.AddOrIndexOf(name);

            return index;
        }

        public static BodyIndex ResolveBody(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return BodyIndex.None;
            }

            if (!context.Resilient)
            {
                return (BodyIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var index = BodyCatalog.FindBodyIndex(name);
            if (index == BodyIndex.None)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find body \"{name}\" in BodyCatalog");
                return BodyIndex.None;
            }

            return index;
        }

        public static int FromStatField(int statIndex, WriterContext context)
        {
            if (!context.Resilient || statIndex == -1)
            {
                return statIndex;
            }

            var statField = StatDef.allStatDefs[statIndex];
            var index = context.SharedStrings.AddOrIndexOf(statField.name);

            return index;
        }

        public static int ResolveStatField(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var def = StatDef.Find(name);
            if (def == null)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find stat \"{name}\" in StatDef");
                return -1;
            }

            return def.index;
        }

        public static int FromUnlockable(UnlockableIndex unlockableIndex, WriterContext context)
        {
            if (unlockableIndex == UnlockableIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)unlockableIndex;
            }

            var def = UnlockableCatalog.GetUnlockableDef(unlockableIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.cachedName);

            return index;
        }

        public static UnlockableIndex ResolveUnlockable(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return UnlockableIndex.None;
            }

            if (!context.Resilient)
            {
                return (UnlockableIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var def = UnlockableCatalog.GetUnlockableDef(name);
            if (def == null)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find unlockable \"{name}\" in UnlockableCatalog");
                return UnlockableIndex.None;
            }

            return def.index;
        }

        public static int FromMaster(MasterCatalog.MasterIndex masterIndex, WriterContext context)
        {
            if (masterIndex == MasterCatalog.MasterIndex.none)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return masterIndex.i;
            }

            var name = MasterCatalog.GetMasterName(masterIndex);
            var index = context.SharedStrings.AddOrIndexOf(name);

            return index;
        }

        public static MasterCatalog.MasterIndex ResolveMaster(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return MasterCatalog.MasterIndex.none;
            }

            if (!context.Resilient)
            {
                return new MasterCatalog.MasterIndex(sharedIndex);
            }

            var name = context.SharedStrings[sharedIndex];
            var index = MasterCatalog.FindMasterIndex(name);
            if (index == MasterCatalog.MasterIndex.none)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find master \"{name}\" in MasterCatalog");
                return MasterCatalog.MasterIndex.none;
            }

            return index;
        }

        public static int FromItem(ItemIndex itemIndex, WriterContext context)
        {
            if (itemIndex == ItemIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)itemIndex;
            }

            var def = ItemCatalog.GetItemDef(itemIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.name);

            return index;
        }

        public static ItemIndex ResolveItem(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return ItemIndex.None;
            }

            if (!context.Resilient)
            {
                return (ItemIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var index = ItemCatalog.FindItemIndex(name);
            if (index == ItemIndex.None)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find item \"{name}\" in ItemCatalog");
                return ItemIndex.None;
            }

            return index;
        }

        public static int FromEquipment(EquipmentIndex equipmentIndex, WriterContext context)
        {
            if (equipmentIndex == EquipmentIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)equipmentIndex;
            }

            var def = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.name);

            return index;
        }

        public static EquipmentIndex ResolveEquipment(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return EquipmentIndex.None;
            }

            if (!context.Resilient)
            {
                return (EquipmentIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var index = EquipmentCatalog.FindEquipmentIndex(name);
            if (index == EquipmentIndex.None)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find equipment \"{name}\" in EquipmentCatalog");
                return EquipmentIndex.None;
            }

            return index;
        }

        public static int FromDrone(DroneIndex droneIndex, WriterContext context)
        {
            if (droneIndex == DroneIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)droneIndex;
            }

            var def = DroneCatalog.GetDroneDef(droneIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.name);

            return index;
        }

        public static DroneIndex ResolveDrone(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return DroneIndex.None;
            }

            if (!context.Resilient)
            {
                return (DroneIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var index = CatalogHelpers.FindDroneIndex(name);
            if (index == DroneIndex.None)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find drone \"{name}\" in DroneCatalog");
                return DroneIndex.None;
            }

            return index;
        }

        public static int FromBodySkin(BodyIndex bodyIndex, uint skinIndex, WriterContext context)
        {
            if (bodyIndex == BodyIndex.None)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)skinIndex;
            }

            var def = SkinCatalog.GetBodySkinDef(bodyIndex, (int)skinIndex);
            var index = context.SharedStrings.AddOrIndexOf(def.name);

            return index;
        }

        public static uint ResolveBodySkin(int sharedIndex, BodyIndex bodyIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return 0;
            }

            if (!context.Resilient)
            {
                return (uint)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var skinDefs = SkinCatalog.GetBodySkinDefs(bodyIndex);
            for (var i = 0u; i < skinDefs.Length; i++)
            {
                var def = skinDefs[i];
                if (def.name == name)
                {
                    return i;
                }
            }

            ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find skin \"{name}\" in SkinCatalog");
            return 0;
        }

        public static int FromGameMode(GameModeIndex gameModeIndex, WriterContext context)
        {
            if (gameModeIndex == GameModeIndex.Invalid)
            {
                return -1;
            }

            if (!context.Resilient)
            {
                return (int)gameModeIndex;
            }

            var name = GameModeCatalog.GetGameModeName(gameModeIndex);
            var index = context.SharedStrings.AddOrIndexOf(name);

            return index;
        }

        public static GameModeIndex ResolveGameMode(int sharedIndex, ReaderContext context)
        {
            if (sharedIndex == -1)
            {
                return GameModeIndex.Invalid;
            }

            if (!context.Resilient)
            {
                return (GameModeIndex)sharedIndex;
            }

            var name = context.SharedStrings[sharedIndex];
            var index = GameModeCatalog.FindGameModeIndex(name);
            if (index == GameModeIndex.Invalid)
            {
                ProperSavePlugin.InstanceLogger.LogWarning($"Couldn't find game mode \"{name}\" in GameModeCatalog");
                return GameModeIndex.Invalid;
            }

            return index;
        }

        public static int FromString(string str, WriterContext context)
        {
            return context.SharedStrings.AddOrIndexOf(str);
        }

        public static string ResolveString(int sharedIndex, ReaderContext context)
        {
            return context.SharedStrings[sharedIndex];
        }
    }
}
