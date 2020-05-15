# Description
Saves your progress at the start of each stage. You can leave run as soon as the stage started.
Only 1 save file for each profile for a singleplayer. If you die, the save will be deleted.

# Multiplayer support
It's finally here.
Only the host must have this mod for it to work.
For multiplayer support, I added `Load` button to the character selection screen (it will appear in both, multiplayer and singleplayer).
![LoadButton](https://cdn.discordapp.com/attachments/706089456855154778/706876815091826809/unknown.png)
A button will be active if you are host and was found suitable save file (if save file has the same set of players as in the current lobby).

# Installation
- Install dependencies.
- Put `ProperSave` folder into `plugins` folder.

# Known issues
I tried to save all necessary data so that when you load the game would continue as it should have been without saving.

- Minions would respawn at different positions each time you load the same save file. It's not big of an issue, and there is nothing I can do about it.
- I've not tested this mod much with achievements unlocking, but for most, if not all cases, it should be working as intended. 

# Changelog
**2.0.0**
* Changed save files structure, because of that, old version saves would be ignored, so consider end saved runs before updating.
* Added multiplayer support.
* Fixed an issue with `lockbox` from `Rusted key` not spawned when loading game.
* Saving some artifacts info for consistent gameplay.
* Fixed an issue for characters added by mods, when their loadouts weren't saved.
* Some minor fixes

**1.1.0**
* Added TemporaryLunarCoins mod support. When loading game lunar coins will be restored.

**1.0.1**
* Fixed crash when the mod was installed using mod managers.
* Saving lunar coins drop chance.

**1.0.0**
* Mod release.