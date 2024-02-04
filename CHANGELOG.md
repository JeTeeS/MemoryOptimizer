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


## [0.1.3] - 2024-2-4

### Fixed

- Smoothing generation would not generate the animations right, now does
- Make the blendtree WD on
- The install when checking for WD in empty controller, should now work
- The entry state was not infact the entry state in the local statemachine
- The int and float syncer params would be bools instead of ints??? No idea how the system worked before LOL
- Clicking "Seelct all" would also select unsynced params, now the change check fixes this
- Now the labels are labels instead of text fields, so you cant edit them
- More UI checks if theres already an install and will tell you that you need to uninstall 
- All the different param types should work with each other now (such as: Ints synced as bools, Floats synced as ints or bools synced as ints)
- "Add to FX" button now adds the param as a float instead of whatever type it was in the param list

### Changed

- "GENERATE" button is now "Install"
- When changing avatars, will now update the FX layer and param list 
- "Value Changed" state is no longer used and changes now directly transitions to the right reset state