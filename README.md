# Memory Optimizer

A tool to help you have more effective memory on your vrchat avatars!

## Instructions

1. Go to my [VPM Listing](https://vpm.jetees.dev) and add the memory optimizer to your vcc.
2. Click "Manage project" on your project in vcc and add the Memory Optimizer
3. There should now be a menu under "Tools" called "TES", you can then click "Memory Optimizer"
4. Select the parmaters you want to be effected by the system (read more about what paramters you can use with the system later!)
5. Choose the amount of steps you want to use (keep in mind more steps mean longer time to sync but less effective paramter space taken up)
6. Hit the "Generate button"!

## What paramteres can I optimize with this system?
This system works by only syncing a few paramters at a time and so by the very nature of the system not all paramters can be optimized, that means position syncing systems like [15-Bits-Position-Rotation-Networking](https://github.com/VRLabs/15-Bits-Position-Rotation-Networking) can not be optimized by this system.

And things like facetracking will get notacibly choppier with the "Change detection" turned off and with it on will break the system completely. 

That being said, almost all systems outside of those specific systems should be compatible with my system! (If you have questions about a specifc system feel free to ask me on discord) 

### Things you *can* optimize

- Simple toggles
- Simple radials

### Change detection
Change detection will make my system look for changes in your paramters and then sync those changes first, this way the syncing will be faster. Although, this will also make the system potencially prone to getting stuck on a specific step if any of the paramters are always changing. Specifically if a paramter is changing more often than the total sync time (which you can see when you're generating the system) then issues might arise. Usually this isn't a problem for most systems, but some systems to be aware of that might cause issues are things like:
- Face tracking
- Some RBG toggles that continously change colors
- 
