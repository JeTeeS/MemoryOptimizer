<div align="center">

# MemoryOptimizer

[![Generic badge](https://img.shields.io/github/downloads/JeTeeS/MemoryOptimizer/total?label=Downloads)](https://github.com/JeTeeS/MemoryOptimizer/releases/latest)
[![Generic badge](https://img.shields.io/badge/Unity-2022.3.6f1-lightblue.svg)](https://unity3d.com/unity/whats-new/2022.3.6)
[![Generic badge](https://img.shields.io/badge/SDK-AvatarSDK3-lightblue.svg)](https://vrchat.com/home/download)

Make your VRChat Avatar's memory more efficient with one click

### 📦 [Add to VRChat Creator Companion](https://vpm.jetees.dev)

</div>

---

## How it works

- *It just does*

## Install guide

- Add [MemoryOptimizer](https://vpm.jetees.dev) to the VRChat Creator Companion (VCC)
  - For a guide on how to add it to VCC, [see this guide](https://notes.sleightly.dev/community-repos/)
- Add MemoryOptimizer to the Unity Project using the "Manage Project" option in VCC
- Select `Tools > TES > MemoryOptimizer` from the top toolbar to open the MemoryOptimizer window
- Click **Optimize** next to each parameter to be optimized
  - See [**How to use**](https://github.com/JeTeeS/MemoryOptimizer#parameters-selection) for what kind of parameters to select
- Select an amount of syncing steps to generate with
  - More steps save more memory at the expense of how long syncing takes
- Select whether to enable/disable [Change Check](https://github.com/JeTeeS/MemoryOptimizer#change-detection)
- Click the `GENERATE` button!

## How to use

### Parameters Selection

This system works by only syncing a few parameters at a time and so by the very nature of the system not all parameters can be optimized, that means some advanced systems like [15-Bits-Position-Rotation-Networking](https://github.com/VRLabs/15-Bits-Position-Rotation-Networking) can not be optimized by this system.

That being said, most things should work just fine!

### Change Detection

Change Detection will detect and prioritize recently changed parameters before continuing to resync other parameters for more responsive interactions.

> [!NOTE]
> Enabling Change Detection requires at least 3 sync steps

It is not advised to enable Change Detection for parameters that are constantly updated.

If a parameter is changing more often than the total sync time, the MemoryOptimizer system may stay frozen updating that parameter due to Change Detection.

Examples of parameters that are frequently updated include:

- Parameters updated via OSC such as Face Tracking
- Continuously incrementing parameters such as RGB floats
