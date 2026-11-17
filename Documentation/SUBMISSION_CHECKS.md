# AFL 2 submission checks

Review date: **November 17, 2026 (WITA)**. Editor: **6000.6.5f1**.

## Repository and source checks

- Preserved the current welcome screen, discovery HUD, scenes, interaction
  behavior and visual design. Existing local application changes are included.
- Restored `ResetButtonSelection.cs` and its original `.meta` from the previous
  committed revision because the final scene still references that script.
  Its implementation was not rewritten.
- Included application and retained reference assets, with their `.meta` files,
  under `Assets/`; shared `ProjectSettings/`; and the manifest, dependency lock
  and pinned Vuforia archive under `Packages/`.
- Checked file/folder metadata and duplicate asset GUIDs. Added hidden `.gitkeep`
  files for empty Unity asset folders so the folders and existing GUIDs survive
  a Git checkout without publishing the local Vuforia configuration.
- Confirmed that final tracking datasets `Informatika_Writing_Area`, `Sofa_Area`
  and `Table_Chair_Lounge_Area_1`, their metadata, and the Sofa Area runtime
  occlusion mesh are included. Sofa runtime occlusion is enabled; the other two
  targets have runtime occlusion disabled in the final scene.
- Validated both dependency JSON files and the local Vuforia package reference.
- Verified all 186 LFS-managed files in the staged snapshot: pointer syntax,
  stored object size/SHA-256, and matching materialized working-file contents.
  Normalized scan geometry/databases/generated meshes and the existing URP image
  into LFS without rewriting old commits or modifying asset contents.
- Scanned staged text for known local Vuforia key values and common secret/private
  key patterns: no matches. This is a targeted check, not a guarantee that every
  possible credential format can be detected.
- Confirmed no generated cache/build directories or ignored credentials are
  tracked. No tracked generated files needed removal; local files were retained.
- Expanded `.gitignore` for generated iOS/Xcode exports, profiling/tool caches,
  signing material and environment secrets. `Packages/` is not excluded.
- Excluded the one-time Vuforia SDK migration/import helper and recovery backups;
  the resolved package and application source are included.
- Left Unity-generated trailing spaces and third-party sample EOF formatting
  unchanged. The diff check passes with generated trailing/EOF blanks ignored.

## Unity validation

Validation runs in a disposable project, rather than building into the working
repository. Both enabled scenes opened successfully with **zero missing scripts**:

```text
AFL2_SCENE_CHECK Assets/Scenes/WelcomeScene.unity missingScripts=0
AFL2_SCENE_CHECK Assets/Scenes/MainScene_AreaTarget.unity missingScripts=0
AFL2_SCRIPT_CHECK Assets/Scripts/ForgyWelcomeScreen_Final.cs class=ForgyWelcomeScreenFinal
AFL2_SCRIPT_CHECK Assets/Scripts/ResetButtonSelection.cs class=ResetButtonSelection
```

A first run imported the source without an existing project Library and passed
these checks. Its subsequent iOS export failed when the host ran out of disk
space. Only generated files in the disposable project were removed. A second
run uses cloned local import caches to reduce disk usage.

The retry completed successfully and generated `Unity-iPhone.xcodeproj`:

```text
AFL2_BUILD_RESULT Succeeded errors=0 warnings=350
AFL2_VERIFICATION_SUCCEEDED
```

Unity exited with code **0**. This verifies Unity's iOS/Xcode export, not a signed
Xcode build or device installation. The build retained 350 reported warnings;
examples include a deprecated API in the older welcome script, Metal shader
warnings in the installed AI Inference package, and generated TextMeshPro C++
compilation notices. No application behavior or design was
changed to silence warnings.

Vuforia regenerated editor preview/navigation prefabs and checksums in the
disposable copy, and TMP updated its font asset during import/build. Application
scene/script and dependency-manifest source contents remained unchanged. These
generated authoring changes were not copied back into the working project.

## Limits and external prerequisites

- A fresh checkout requires Git LFS downloads, package-registry access when
  packages are not cached, and the pinned Editor with iOS Build Support.
- The Vuforia template deliberately contains no working application key. Users
  must provide their own authorized key before expecting working AR tracking.
- Apple signing/provisioning, physical-device tracking, voice playback and visual
  acceptance checks require the documented iOS/device workflow. They are not
  established solely by the repository checks.
- The existing Vuforia SDK archive and other asset redistribution/licensing gaps
  are explicitly identified in `ASSET_CREDITS.md`; this technical review does not
  grant or certify those rights.
