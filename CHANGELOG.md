# Changelog

## [0.1.0] - 2024-2-2

### Added

- Uhh everything :)

## [0.1.1] - 2024-2-2

### Fixed

- Expression params marked dirty after uninstall

### Removed

- Some debugging logs

## [0.1.2] - 2024-2-2

### Changed

- No longer uses submodule

## [0.1.3] - 2024-2-2

### Fixed

- WD detection now handles empty layers properly

## [0.1.4] - 2024-2-4

### Fixed

- Smoothing generation would not generate the animations right, now does
- Make the blendtree WD on
- The install would fail when checking for WD in empty controller, should now work
- The entry state was not infact the entry state in the local statemachine
- The int and float syncer params would be bools instead of ints??? No idea how the system worked before LOL
- Clicking "Select all" would also select unsynced params, now the change check fixes this
- Now the labels are labels instead of text fields, so you cant edit them
- More UI checks if theres already an install and will tell you that you need to uninstall 
- All the different param types should work with each other now (such as: Ints synced as bools, Floats synced as ints or bools synced as ints)
- "Add to FX" button now adds the param as a float instead of whatever type it was in the param list

### Changed

- "GENERATE" button is now "Install"
- When changing avatars, will now update the FX layer and param list 
- "Value Changed" state is no longer used and changes now directly transition to the right reset state

## [0.1.5] - 2024-2-5

### Changed

- General code clean up
- New readme

## [0.1.6] - 2024-2-19

### Fixes

- Fix OnChangeUpdate

### Added

- Deselect Prefix button

## [0.1.7] - 2024-2-19

### Added

- Backup feature

### Changed

- Sync Steps are now normally max 4, but this can be unlocked in settings

## [0.1.8] - 2024-2-24

### Changed

- Readme
- Code cleanup
- Unity 2019 compatibility (can't gaurantee it works, I wont test it, but should at least compile)

## [0.1.9] - 2024-2-25

### Added

- Function to change which folder generated assets are saved to
- Ability to edit step delay (not recommended)

## [0.1.10] - 2024-2-25

### Changed

- Menu tabs now use Toolbar instead of buttons lol

## [0.1.11] - 2024-2-28

### Added

- Uninstall now properly deletes blendtrees from controller when uninstalling

### Fixed

- Custom folderpath

### Changed

- Minor UI change in settings

## [0.1.12] - 2024-2-29

### Changed

- Readme
- Param selection now resets on avatar change

### Fixed

- An error would sometimes show up when switching avatars

## [0.1.13] - 2024-3-2

### Changed

- Now displays the total param cost instead of the selected params cost

### Fixed 

- Now properly checks settings without having to open the settings tab

## [0.1.14] - 2024-3-11

### Changed 

- Calculation of param cost before generating 

### Added

- Check if param amount will exceed 255 when generating

## [0.1.15] - 2024-3-14

### Changed

- An error during uninstall no longer blocks you from uninstalling but instead prompts you to visit the discord.

### Fixed

- Deselect prefix now properly updates the UI

## [0.1.16] - 2024-3-17

### Fixed

- Missing null check in the OnChangeUpdate that caused error spam when there was no expression parameters

### Changed 

- Readme

## [0.1.17] - 2024-3-22

### Changed

- Name of Change Check to Change Detection
- Readme

## [0.1.18] - 2024-5-22

### Changed

- Max unsynced parameters now match the vrchat limit again
