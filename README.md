# VPM Package Template [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/JSLogo.png" width="30" height="30">](https://vrc.sleightly.dev/ "JustSleightly") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Discord.png" width="30" height="30">](https://discord.sleightly.dev/ "Discord") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/GitHub.png" width="30" height="30">](https://github.sleightly.dev/ "Github") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Store.png" width="30" height="30">](https://store.sleightly.dev/ "Store")

[![GitHub stars](https://img.shields.io/github/stars/JustSleightly/VPM-Package-Template)](https://github.com/JustSleightly/VPM-Package-Template/stargazers) [![GitHub Tags](https://img.shields.io/github/tag/JustSleightly/VPM-Package-Template)](https://github.com/JustSleightly/VPM-Package-Template/tags) [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/JustSleightly/VPM-Package-Template?include_prereleases)](https://github.com/JustSleightly/VPM-Package-Template/releases) [![GitHub issues](https://img.shields.io/github/issues/JustSleightly/VPM-Package-Template)](https://github.com/JustSleightly/VPM-Package-Template/issues) [![GitHub last commit](https://img.shields.io/github/last-commit/JustSleightly/VPM-Package-Template)](https://github.com/JustSleightly/VPM-Package-Template/commits/main) [![Discord](https://img.shields.io/discord/780192344800362506)](https://discord.sleightly.dev/)

A stripped down version of the official VRChat [VPM Package Template](https://github.com/vrchat-community/template-package) that excludes all of the extra website/project bloat.

This is modified from [Dreadrith's template](https://github.com/Dreadrith/Listed-VPM-Template) to support a slightly different workflow, with inspiration from [bd_](https://github.com/bdunderscore/modular-avatar) and [Razgriz](https://github.com/rrazgriz/RATS).

## Features

- Can clone multiple VPM package templates into one Unity project
- Automatically builds a GitHub release with a `.unitypackage`, `.zip`, and `package.json` upon pushing a commit to the `package.json` only if there is no existing release tag for the pushed version number when `package.json` is updated
- Automatically adds an icon to the `.unitypackage` that is displayed when imported
- Can automatically trigger an update/action to an external repository like a [VPM Package Listing](https://github.com/JustSleightly/VPM-Package-Listing-Template)

## Instructions

1. Create a new repository using this button: [<img src="https://user-images.githubusercontent.com/737888/185467681-e5fdb099-d99f-454b-8d9e-0760e5a6e588.png" height="25">](https://github.com/JustSleightly/VPM-Package-Template/generate/ "Use this template")
2. Clone your new repository onto your PC within an **existing Unity project** under `Packages/` with any directory name
    - This will generate a fresh set of GUIDs for each file within this package template and prevent conflicts with other packages
3. Modify the cloned files for your new package
    - Replace or remove `.github/thumbnail.png` with your own `.unitypackage` import thumbnail
    - Edit `.github/workflows/release.yml`
        - Line 10 (packageName in [Unity official name](https://docs.unity3d.com/Manual/cus-naming.html) format)
        - Line 11 (Packages/packageName)
        - Line 12 (Example: JS-Templatev1.0.0.unitypackage)
        - Line 13 (Example: VPM Package Template v1.0.0)
        - Line 14 (Read [Trigger Repo Update](https://github.com/JustSleightly/VPM-Package-Template#trigger-repo-update) section below)
    - Edit `.github/workflows/trigger-repo-update.yml`
        - Read [Trigger Repo Update](https://github.com/JustSleightly/VPM-Package-Template#trigger-repo-update) section below
    - Rename and Edit `Documentation~/dev.sleightly.template.md` if used
    - Rename and Edit `Editor/dev.sleightly.template.Editor.asmdef` if used
        - "name"
        - "references"
    - Rename and Edit `Runtime/dev.sleightly.template.asmdef` if used
        - "name"
    - Edit `CHANGELOG.md` ([Recommended Format](https://keepachangelog.com/en/))
    - Edit `LICENSE.md` ([Need help?](https://choosealicense.com/))
    - Edit `package.json`
        - Use [VRChat's](https://vcc.docs.vrchat.com/vpm/packages#vpm-manifest-additions) and [Unity's](https://docs.unity3d.com/2019.4/Documentation/Manual/upm-manifestPkg.html) documentation for reference
4. Add any necessary scripts, resources, [samples](https://docs.unity3d.com/2019.4/Documentation/Manual/cus-samples.html), and other files
5. Remove `Documentation~`, `Editor`, `Runtime`, `CHANGELOG.md`, and `LICENSE.md` if unused

## Trigger Repo Update

If you have a [VPM Package Listing](https://github.com/JustSleightly/VPM-Package-Listing-Template) (or another repository) you'd like to trigger a workflow for, after building/publishing/modifying a release in this package repository, conduct the following steps as well. Otherwise, skip these steps.

1. Edit `.github/workflows/trigger-repo-update.yml`
    - Line 12 (Set to `true` to enable triggering a repo update when a release is manually published/modified)
    - Line 13 (Owner of target repository to trigger)
    - Line 14 (Name of target repository to trigger)
    - Line 15 (Branch of target repository to trigger)
    - Line 16 (File name of target workflow to trigger)
    - Line 17 (Replace `VPM_TOKEN` with name of [Personal Access Token](https://github.com/JustSleightly/VPM-Package-Template#setting-a-personal-access-token) secret added to this repository)
2. Edit `.github/workflows/release.yml`
    - Line 14 (Set to `true` in order for the automatic build to trigger `.github/workflows/trigger-repo-update.yml`)

### Setting a Personal Access Token

To trigger a remote repository you must create a Personal Access Token (PAT) with the repo scope and store it as a secret.

1. [Create a new fine-grained personal access token (beta)](https://github.com/settings/personal-access-tokens/new)
    - Token name (Can be anything, and is different than the name used in `trigger-repo-update.yml`)
    - Expiration (Can be anything up to a year)
    - Repository Access - Only Select Repositories
        - Select your VPM Listing/Target repository, not your package repository
    - Permissions - Repository Permissions
        - Actions - Read and Write
        - Metadata - Read-only (Set by default when granted Actions permissions - Mandatory)
    - Press `Generate Token`
        - **Copy and Save** the token on the following screen as it will not be displayed again
        - This can be used for any package repository that you may want to trigger your listing repository in the future
2. [Add your PAT as a secret to your package repository](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions#creating-secrets-for-a-repository)
    - Navigate to your package repository (not your listing/target repository) and press the **Settings** tab
    - Navigate to `Secrets and Variables > Actions` under the **Security** tab in the sidebar
    - Press `New repository secret`
    - Add a `Name` for the secret
        - This is the name used in `trigger-repo-update.yml` which the template has named `VPM_TOKEN` by default
        - Consider naming it based on your listing/target repository in case you have multiple listings in the future
    - Copy the token from step 1 into the `Secret` field
    - Press `Add Secret`

## Notes

Folders that end in `~` such as `Documentation~` or `Samples~` are hidden from the Unity Project view within the editor.

Therefore, when the package is exported as a `.unitypackage` whether manually or via automated GitHub workflow, such folders are omitted. The folders are still present in the `.zip` export and when imported through the [VRChat Creator Companion](https://vcc.docs.vrchat.com/vpm/packages/#community-packages)
