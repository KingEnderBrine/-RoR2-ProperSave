**3.0.4**
* Fixed an issue where some issue with unlockables would prevent saving.

**3.0.3**
* Fixed an issue where loading a save after completing artifact trial for `Artifact of Metamorphosis` you would spawn as the character you started the game with, instead of the character you got during trial.
* Fixed an issue where an error during saving would leave a broken save file that you can't load.

**3.0.2**
* Fixed an issue where loading would break halfway when some mods are present.

**3.0.1**
* Fixed an issue where old save file wouldn't be overwritten after starting a new run without loading.

**3.0.0**
* Changed save file format from json to binary, existing saves should automatically migrate to the new format.
* Added config option to change save type.
* Fixed an issue where transitioning a scene while having broken operator drones would result in non-reparable drones after loading.

**2.13.3**
* Fixed an issue where loading a run in `Conduit Canyon` as the first run after launching the game will cause teleporter to become non-interactable.

**2.13.2**
* Fixed an issue where going to next stage after someone disconnected would cause a black screen and constant error spam.

**2.13.1**
* Fixed an issue where `Artifact of Prestige` data wouldn't save.

**2.13.0**
* Fixes for `Alloyed Collective` update.
* Eclipse win should now count towards the character you started the save with, not the one you had selected before save loading. 
* Added `ps_force_load` command for debug purposes.

**2.12.1**
* Added Japanese translation, thanks `WakefulSpect`.

**2.12.0**
* Fixes for 1.3.6 update.

**2.11.2**
* Fixed an issue where Prayer beads' stats bonuses wouldn't save/load.
* Fixed Ukrainian localization, thanks `Damglador`.

**2.11.1**
* Fixed an issue where Equipment cooldown would be run time + leftover cooldown time.

**2.11.0**
* Fixes for `Seekers of the Storm` update.
* Fixed an issue where save files wouldn't be deleted on gameover when using cloud store unless you restarted the game after save was made.

**2.10.0**
* Fixes for `Devotion` update.
* Added support for `Devotion` artifact

**2.9.1**
* Updated French translation, thanks `NorthBlue333`.

**2.9.0**
* Added config option to enable Steam/Epic games cloud storage support, allowing you to synchronize saves between devices.
* Added config option to change saves directory.

**2.8.11**
* Removed unnecessary logging from minion inventory change after loading, which caused lag with a big enough mod list.

**2.8.10**
* Added Ukrainian localization, thanks `Damglador` for providing the translation. (If you have https://thunderstore.io/package/RoR2_UA/Risk_of_Rain_2_Ukrainian/)

**2.8.9**

* Fixed an issue where broken save would prevent you from starting `Simulacrum` run (maybe `Eclipse` too?)

**2.8.8**

* Fixed an issue where loading a save would load minions inventory too early resulting in adding extra hidden items (e.g. `EquipmentDrone` was given `BoostEquipmentRecharge`)

**2.8.7**

* Fixed an issue where having `ProperSave`, `EphemeralCoins`, `BiggerBazaar`, `Risky_Artifacts` and `Enforcer` would result in a cyclic dependency, which meant that no mods were loaded at all.

**2.8.6**

* Fixed an issue where the mod wouldn't work correctly for `EpicGames` users.

**2.8.5**

* Added Simplified Chinese localization, thanks `mchobbylong` for providing the translation.

**2.8.4**

* Added check for loaded content. Now you will see a message in save description if the game content is different from what it has been during save. You can still load that save, it's more like a warning that something might go wrong but not necessarily will.
* Fixed an issue where `Benthic Bloom` would transform items differently when you load a run vs when you got to the stage if you were playing on any difficulty other than `Rainstorm`.

**2.8.3**

* Added French localization, thanks `ClEeVEeRYT` for providing the translation.

**2.8.2**

* Fixed an issue where `Benthic Bloom` would be activated twice when you load the game.
* Storing `Benthic Bloom` rng, so that the same items would be converted when you load a save.
* Fixed a typo in `ProperSave.SaveFile.OnGatherSaveData` event name.
* Added more checks so the old broken saves wouldn't cause issues in lobby.

**2.8.1**

* Fixed an issue where loading a run right after launching the game would result in incorrect prices for chest, terminals, etc.
* Fixed an issue where info about disconnected players wouldn't be saved resulting in, essentially, lost runs for multiplayer with friends 

**2.8.0**

* Fixes for `Survivors of the Void` update.
* `Simulacrum` support

**2.7.0**

* Remove r2api dependency

**2.6.1**

* Fixed time display in lobby save info.

**2.6.0**

* Added tooltip with short save info when hover over `Load` button or hold load button on a gamepad.

**2.5.3**

* Added text to a quit confirmation dialog which informs you when the game was saved last time.
* Updated chat message, now it includes a stage name at which the game was saved.

**2.5.2**

* Fixed an issue where the mod was trying to save while being a client in a multiplayer game.

**2.5.1**

* Fixed an issue where after loading Monsoon run you sometimes didn't get mastery skin.

**2.5.0**

* Added mods support. Now any other dev can add data they want to be saved from their mods. 
* A lot of refactoring.

**2.4.5**

* Fixed `StartingItemsGUI` support. Hopefully, this is the last time. Thanks `Phedg1` for making changes to `StartingItemsGUI` that will make my mod support last longer.

**2.4.4**

* Fixed `StartingItemsGUI` support.

**2.4.3**

* Fixed an issue where the game would be saved in the ending cutscene and you could load in it if you restart the game.

**2.4.2**

* Changed artifacts saving. Now they are saved at the start of the stage (previously they were saved at the start of the run).

**2.4.1**

* Fixed `StartingItemsGUI` support.

**2.4.0**

* Updated for `1.0.1.1` version. Old saves are not compatible with new version!

**2.3.2**

* Fixed `StartingItemsGUI` support. (This time for real)

**2.3.1**

* Fixed `TemporaryLunarCoins` support.
* Fixed `StartingItemsGUI` support.

**2.3.0**

* Added `ShareSuite` support. (No longer resetting gold to 0 when loading run).
* Added `BiggerBazaar` support.

**2.2.2**

* Fixed `Load` button being active when returned to lobby after death.

**2.2.1**

* Updated language stuff

**2.2.0**

* Updated for `RoR2` release.
* Removed `Continue` button from the main menu (for game modes support).
* Game modes support.

**2.1.1**

* Fixed a bug: when entering the lobby when using a gamepad is causing the lobby glitches.

**2.1.0**

* Added `StartingItemsGUI` support. (Items adding disabled while loading the game). Requested by `Thunderer1101` on GitHub.

**2.0.0**

* Changed save files structure, because of that, old version saves would be ignored, so consider end saved runs before updating.
* Added multiplayer support.
* Fixed an issue with `lockbox` from `Rusted key` not spawned when loading game.
* Saving some artifacts info for consistent gameplay.
* Fixed an issue for characters added by mods, when their loadouts weren't saved.
* Some minor fixes

**1.1.0**

* Added `TemporaryLunarCoins` mod support. When loading game lunar coins will be restored.

**1.0.1**

* Fixed crash when the mod was installed using mod managers.
* Saving lunar coins drop chance.

**1.0.0**

* Mod release.