<div align="center">

# MemoryOptimizer

[![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/JeTeeS/MemoryOptimizer/total?style=for-the-badge)](https://github.com/JeTeeS/MemoryOptimizer/releases/latest)
[![Static Badge](https://img.shields.io/badge/Unity-2022.3.6f1-lightblue?style=for-the-badge&logoColor=lightblue)](https://unity3d.com/unity/whats-new/2022.3.6)
[![Static Badge](https://img.shields.io/badge/SDK-AvatarSDK3-lightblue?style=for-the-badge&logoColor=lightblue)](https://vrchat.com/home/download)
[![Discord](https://img.shields.io/discord/875595847688155136?style=for-the-badge&logo=Discord)](https://discord.gg/N7snuJhzkd)

Make your VRChat Avatar's memory more efficient with one click

![MemoryOptimizer](https://github.com/JeTeeS/MemoryOptimizer/raw/main/Media/Preview.png)

### 📦 [Add to VRChat Creator Companion](https://vpm.jetees.dev)

</div>

---

## How it works

MemoryOptimizer allows you to surpass the typical parameter memory limits of the VRCSDK, by marking existing parameters as unsynced and using its own set of parameters to cyclically sync the unsynced parameters using parameter drivers. With Change Detection enabled, parameters that that have recently changed in value will be resynced first for faster perceived syncing.

This does have some limitations, as the syncing time will be delayed compared to if you would sync the parameter normally, so it's not ideal for time-sensitive applications. Also, with the change detection option enabled, the system can get stuck on a certain step of the cycle and not sync the other parameters (See [**Change Detection**](https://github.com/JeTeeS/MemoryOptimizer#change-detection)).

## Install guide

- Add [**MemoryOptimizer**](https://vpm.jetees.dev) to the VRChat Creator Companion (VCC)
  - For a guide on how to add it to VCC, [see this guide](https://notes.sleightly.dev/community-repos/)
- Add MemoryOptimizer to the Unity Project using the "Manage Project" option in VCC
- Select `Tools > TES > MemoryOptimizer` from the top toolbar to open the MemoryOptimizer window
- Click **Optimize** next to each parameter to be optimized
  - See [**How to use**](https://github.com/JeTeeS/MemoryOptimizer#parameters-selection) for what kind of parameters to select
- Select an amount of syncing steps to generate with
  - See [**Sync Steps**](https://github.com/JeTeeS/MemoryOptimizer#sync-steps), most of the time this can be left at 2 or 3 steps
- Select whether to enable/disable [**Change Detection**](https://github.com/JeTeeS/MemoryOptimizer#change-detection)
- Click the `Install` button!

<img src="https://github.com/JeTeeS/MemoryOptimizer/raw/main/Media/Preview.gif" height="640" alt="MemoryOptimizer Installation">

## How to use

### Parameter Selection

This system works by only syncing a few parameters at a time, so by the very nature of the system not all parameters can be optimized. Consequently, some advanced systems like [15-Bits-Position-Rotation-Networking](https://github.com/VRLabs/15-Bits-Position-Rotation-Networking) can not be optimized by this system. That being said, most things should work just fine!

To select a parameter, simply click the `Optimize` button.

- 🔴 If the button is red, that means the parameter is not selected.
- 🟡 If it's yellow, the parameter is selected, but will not be optimized.
- 🟢 If it's green, the parameter will be optimized when you click `Install`.

You can also use the `Select All` and `Deselect Prefix` buttons to quickly select the parameters you need.

Try changing the amount of [**Sync Steps**](https://github.com/JeTeeS/MemoryOptimizer#sync-steps) to get as many parameters optimized as possible!

### Change Detection

Change Detection will detect and prioritize recently changed parameters before continuing to resync other parameters for more responsive interactions.

> [!NOTE]
> Enabling Change Detection requires at least 3 sync steps

It is not advised to enable Change Detection for parameters that are constantly updated.

If a parameter is changing more often than the total sync time, the MemoryOptimizer system may stay frozen updating that parameter due to Change Detection.

Examples of parameters that are frequently updated include:

- Parameters updated via OSC such as Face Tracking
- Continuously incrementing parameters such as RGB floats

> [!WARNING]
> This option may have a significant performance impact at higher parameter counts, so it is better decrease step count to improve sync latency if possible.

### Sync Steps

Sync steps are the amount of steps the system divides your params into to sync them. A higher number will take longer to sync, but will take up less parameter space (depending on how many parameters are selected of each type).

It is generally recommended to have as few sync steps as possible, as the more steps you have the longer the system will take to completely sync. Try changing this number to see if you save significantly more or less space with your selected parameters.

> [!TIP]
> By default the sync steps slider is limited to 4 steps, but this can be unlocked in the settings.
