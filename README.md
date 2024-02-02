# Memory Optimizer

A tool to help you have more effective memory on your vrchat avatars!

## Instructions

1. Go to my [VPM Listing](https://vpm.jetees.dev) and add the memory optimizer to vcc.
2. Click "Manage project" on your project in vcc and add the Memory Optimizer
3. There should now be a menu under "Tools" called "TES", you can then click "Memory Optimizer"
4. Select the parmaters you want to be effected by the system (read more about what paramters you can use with the system later!)
5. Choose the amount of steps you want to use (keep in mind more steps mean longer time to sync but less effective paramter space taken up)
6. Hit the "Generate button"!

##

### What paramteres do I select?

This system works by only syncing a few paramters at a time and so by the very nature of the system not all paramters can be optimized, that means some advanced systems like [15-Bits-Position-Rotation-Networking](https://github.com/VRLabs/15-Bits-Position-Rotation-Networking) can not be optimized by this system.

That being said, most things should work just fine!

### Change detection

Change detection will make my system look for changes in your paramters and then sync those changes first, this will make the syncing faster.

**NOTE: You have to have more than 3 sync steps to enable change detection!**

Although, this will also make the system potencially more prone to getting stuck on a specific step if any of the paramters are *always* changing. Specifically if a parameter is changing more often than the total sync time (which you can see when you're generating the system), then issues might arise. Usually this isn't a problem for most systems, but some systems to be aware of that might cause issues are:

- Face tracking
- Some RBG toggles that continously change colors (not all)
-
