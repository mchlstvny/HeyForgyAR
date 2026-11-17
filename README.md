# Hey, Forgy!

An interactive Unity AR campus companion prepared for the AFL 2 submission.
Discover Forgy at three locations on the fifth floor: the Informatics writing
area near the lift, the lounge sofa, and the table-and-chair lounge area.

## Features

- A portrait welcome screen with an illustrated mascot and an **ENTER AR** button.
- A location guide and a compact discovery counter for the three campus spots.
- Vuforia Area Target tracking with proximity-triggered interactions.
- Procedural waving, sofa peeking, flying/cheering, idle motion, and eye expressions.
- Character dialogue bubbles and three prerecorded AI-generated voice clips.
- Vuforia runtime sofa occlusion and ambient-intensity-based character lighting.

The enabled scene order is:

1. `Assets/Scenes/WelcomeScene.unity`
2. `Assets/Scenes/MainScene_AreaTarget.unity`

Other scenes and older scan datasets are retained as development/reference assets;
they are not enabled in the submission scene list. The current discovery guide
opens on entry and on request, rather than reopening when tracking is lost.

## Technologies and dependencies

| Component | Version / configuration |
| --- | --- |
| Unity Editor | **6000.6.5f1**, recorded in `ProjectSettings/ProjectVersion.txt` |
| Vuforia Engine | **11.4.4**, local package at `Packages/com.ptc.vuforia.engine-11.4.4.tgz` |
| Universal Render Pipeline | **17.6.0** |
| Unity Input System | **1.20.0** |
| Unity uGUI / TextMeshPro | **2.6.0** |
| iOS bundle identifier | `com.michelestevany.heyforgy` |
| iOS deployment target | **15.0**, as serialized in Player Settings |

`Packages/manifest.json` and `Packages/packages-lock.json` preserve the complete
dependency configuration, including installed editor and support packages.
Installed AI Assistant, AI Inference, AI Navigation, Timeline, and Visual Scripting
packages do not by themselves establish use of those features in the application.
The animations and interactions are implemented in C#.

## Repository contents

- `Assets/`: application scripts, scenes, model, materials, textures, UI, fonts,
  audio, authoring scan resources, supporting sample content, and Unity `.meta` files.
- `Assets/StreamingAssets/Vuforia/`: the three final `.xml`/`.dat` tracking
  databases and the Sofa Area runtime occlusion mesh.
- `Assets/Editor/Vuforia/`: scan authoring models, preview prefabs, materials,
  textures, and navigation meshes. These are editor resources, not a gameplay
  navigation implementation.
- `ProjectSettings/`: shared Unity, rendering, player, and build configuration.
- `Packages/`: dependency manifests and the pinned Vuforia package archive.
- `Documentation/`: Vuforia setup, asset credits, and submission verification notes.

Generated Unity caches, build exports, profiler captures, local SDK-importer
residue, recovery scenes, credentials, and signing files are excluded. Shared
VS Code configuration is retained; IDE caches are excluded.

## Reopen the project

1. Install Git and **Git LFS**. Install Unity **6000.6.5f1** through Unity Hub,
   including **iOS Build Support**. Building/signing the exported iOS application
   also requires macOS, Xcode, and appropriate Apple signing access.
2. Clone the existing repository and retrieve its large assets:

   ```sh
   git lfs install
   git clone https://github.com/mchlstvny/HeyForgyAR.git
   cd HeyForgyAR
   git lfs pull
   git lfs fsck
   ```

   A GitHub ZIP download is not a substitute for verifying that LFS objects have
   been downloaded. The `.tgz` must be the actual package archive, not an LFS
   pointer file. Its expected SHA-256 is:

   ```text
   47a1a1d41a5268e30910ba4ca9f8650cbe6c1cb3abdcb87163355d02359d7bab
   ```

3. Before opening Unity, create the local Vuforia configuration from the sanitized
   template, only if a live configuration does not already exist:

   ```sh
   mkdir -p Assets/Resources
   cp Documentation/Vuforia/VuforiaConfiguration.asset.example Assets/Resources/VuforiaConfiguration.asset
   ```

   **Do not overwrite an existing configured file.** The template has no working
   license key. See [the Vuforia setup guide](Documentation/Vuforia/README.md).
4. Add this directory in Unity Hub and open it using the pinned Editor version.
   Allow package resolution and asset imports to finish. Registry dependencies
   may require internet access; `Library/` is intentionally regenerated.
5. Open **Window > Vuforia Configuration**, enter your own authorized application
   key, and review/accept the applicable terms. Keep the live configuration and
   its `.meta` file out of Git. Academic submission does not supply a transferable
   Vuforia application license.
6. Open `WelcomeScene`. Confirm that its controller references Jupiter SDF and
   `ForgyWelcome2`, and that **ENTER AR** opens `MainScene_AreaTarget`.

Do not delete or regenerate versioned `.meta` files: Unity scene references rely
on their GUIDs. Restoring the repository's `.meta` file is preferable to allowing
Unity to assign a replacement GUID.

## Build and run on iOS

1. In Unity, open **File > Build Profiles** and select the existing
   `Assets/Settings/Build Profiles/iOS.asset` profile. Switch to iOS if necessary.
2. Confirm the global scene list contains only the two enabled scenes in the
   order shown above. The legacy `MainScene` is disabled.
3. Review Player Settings: bundle identifier, iOS minimum version, camera usage
   description, and signing options. Use a physical, Vuforia-supported iPhone for
   camera tracking; the scene depends on the supplied real campus locations.
4. Select **Build** and export to **`Build/iOS/`**. This directory is ignored by
   Git. The saved profile currently enables Development Build and profiler
   connection; retain these for the documented development configuration or
   deliberately select release options for a separate release build.
5. Open the generated Xcode project/workspace. Select the application target,
   configure **Signing & Capabilities** with your Apple team and provisioning,
   choose a connected iPhone, and build/run. Do not commit certificates, private
   keys, provisioning profiles, exported `.ipa` files, or the Xcode build output.
6. Allow camera access. Visit and scan each of the three captured locations.
   Verify Wave, Sofa Peek, and Cheer, their dialogue/voice, discovery count,
   lighting response, and sofa occlusion.

The repository preserves the tracking datasets; it cannot reproduce the physical
environment, supply personal SDK/signing credentials, or establish device-level
behavior solely from source. See [verification notes](Documentation/SUBMISSION_CHECKS.md)
for the checks actually performed.

## Large files and source control

Git LFS stores the model, image/audio/font assets, Vuforia package archive,
scan `.glb`/`.3dt`/`.dat` files, and generated scan mesh assets. Scene YAML, scripts,
material configuration, dependency manifests, and `.meta` files remain ordinary
Git text. Existing history is retained; no history rewrite is required.

Before committing changes:

```sh
git status --short
git diff --check
git lfs status
git lfs fsck
```

Ensure the live `Assets/Resources/VuforiaConfiguration.asset` remains ignored.

## Asset attribution and redistribution

See [asset credits and license notes](Documentation/ASSET_CREDITS.md). This is an
academic project submission, not a grant of open-source rights over third-party
assets. Vuforia's SDK is proprietary; its existing versioned archive is not
relicensed by this repository. Standalone SDK redistribution permissions need
separate review under PTC's agreement. Artwork ownership, some sample-asset terms,
and the ElevenLabs output license/account tier are not documented in the project.
No blanket license is asserted for them.
