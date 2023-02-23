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