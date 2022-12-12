# Description
Saves your progress at the start of each stage. You can leave run as soon as the stage started.
Only 1 save file for each profile for a singleplayer. If you die, the save will be deleted.

# Multiplayer support
Only the host must have this mod for it to work.
A `Load` button will be active if you are host and was found suitable save file (if save file has the same set of players as in the current lobby).

# Game modes support
Each game mode (`Classic`, `Eclipse`, `Simulacrum`) has it's own save files, so that you can switch between game modes and not losing your progress.

# For mod developers
#### Saving
To save data you need to subscribe to `ProperSave.SaveFile.OnGatherSaveData`. It will be called every time the game is saved (this happens on `RoR2.Stage.onStageStartGlobal`) to gather info from mods that needs to be saved. You can add any value with any key, but remember that other mods can do the same thing, so keep keys unique (maybe add a mod name in front or something). 
I would suggest adding only one object per mod because the type of the object is also stored to be able to deserialize objects, and it can take a lot of space in comparison with stored value. 
An object that you add in the dictionary will be serialized to JSON. Here is some info about serialization:

* Only public properties/fields will be serialized.
* You can add `[DataMember()]` attribute from `System.Runtime.Serialization` to specify custom name for property/field in json file.
* You can add `[IgnoreDataMember]` attribute from `System.Runtime.Serialization` to specify that this public property/field should be ignored on serialization.

#### Loading
Once save file is loaded you can get data you've previously saved and apply it anytime you want. Here are some things that will help you with that:

* `ProperSave.Loading.IsLoading` - you can use this to check if the `ProperSave` is loading.
* `ProperSave.Loading.FirstRunStage` - you can use this to check if run is starting. Is set to `true` on `RoR2.Run.Start`, is set to `false` on `RoR2.Stage.onStageStartGlobal`
* `ProperSave.Loading.OnLoadingStarted` - event, fired when `IsLoading` set to true (this happens after save file is loaded into memory, before run started).
* `ProperSave.Loading.OnLoadingEnded` - event, fired when `IsLoading` set to false (this happens after `RoR2.TeamManager.Start` because this is the last step of loading process).
* `ProperSave.Loading.CurrentSave` - current save file, you can access it after `OnLoadingStarted`. Will be overwritten every time game is saved.
* `CurrentSave.GetModdedData<Type>("")` - use this method to get data that you've saved.
* `ProperSave.Data` - under this namespace you can find classes used to save some of vanilla data. 

# Known issues
I tried to save all necessary data so that when you load the game would continue as it should have been without saving.

- Minions would respawn at different positions each time you load the same save file. It's not big of an issue, and there is nothing I can do about it.
- I've not tested this mod much with achievements unlocking, but for most, if not all cases, it should be working as intended. 

# Changelog
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
* Fixed a type in `ProperSave.SaveFile.OnGatherSaveData` event name.
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

* Added tooltip with short save info when hower over `Load` button or hold load button on a gamepad.

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

* Added `ShareSuite` support. (No longer reseting gold to 0 when loading run).
* Added `BiggerBazaar` support.

**2.2.2**

* Fixed `Load` button being active when returned to lobby after death.

**2.2.1**

* Updated langauge stuff

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