# Asset credits and license notes

Prepared for the AFL 2 submission from project files, package notices, Git history,
and embedded media metadata. A Git author or asset filename does not establish
original ownership. "Not documented" means no supporting record was found.

| Asset group | Location | Source / documented terms | Application use |
| --- | --- | --- | --- |
| Vuforia Engine 11.4.4 | `Packages/com.ptc.vuforia.engine-11.4.4.tgz` | PTC; proprietary Vuforia Developer Agreement. Package includes `LICENSE.md` and third-party notices. | Active AR tracking, illumination and sofa occlusion. |
| URP/Core, uGUI/TMP runtime, Input System | Unity packages listed in manifest/lock | Unity Companion License, as stated in installed package license files. Copied asset-specific notices still apply. | Active rendering, text/UI and input. |
| Jupiter | `Assets/Unity UI Samples/Fonts/Jupiter/` | Isurus Labs; supplied `LICENSE.txt` states public domain. | Assigned final UI font; TMP SDF atlas included. |
| Liberation Sans | `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` and TMP resources | Embedded font metadata: SIL Open Font License 1.1; copyright Google/Red Hat. | TMP default/fallback resources. |
| Roboto Bold | `Assets/TextMesh Pro/Examples & Extras/Fonts/` | Google; Apache License 2.0 in embedded metadata and supplied license text. | Retained sample content. |
| SF Window and other UI samples | `Assets/Unity UI Samples/` | Unity UI Samples collection. Individual authorship/license for SF Window: Not documented. Preserve supplied notices. | SF Window is the final dialogue panel sprite; most other samples are development references. |
| NASA sample background | `Assets/Unity UI Samples/Textures and Sprites/SF UI/Background/` | Supplied license states NASA/public domain. | Retained sample; not a final scene background. This notice does not cover other sample images. |
| Forgy model | `Assets/Models/forgy.fbx` | Metadata identifies Blender export and native filename `forgy_test.blend`; author field empty. Original designer, ownership and asset license: Not documented. | Three final character instances. |
| Character texture/emblem | `Assets/Textures/texture forgy.png` | Creator, emblem source and license: Not documented. | Final character body material. |
| Current welcome portrait | `Assets/Textures/ForgyWelcome2.png` | Creator, license and AI involvement: Not documented. | Final welcome illustration. |
| Voice clips | `Assets/Audio/{WaveForgy,CheerForgy,BreakForgy}.mp3` | Embedded C2PA metadata identifies ElevenLabs and trained algorithmic media. Voice/model, generation settings, account tier and output license: Not documented. | Three final interaction voices. |
| Earlier welcome illustration | `Assets/Textures/ForgyWelcomeImage.png` | Embedded C2PA metadata identifies ChatGPT/gpt-image and trained algorithmic media. Prompt and output license: Not documented. | Experimental `WelcomeScene_Holo` only. |
| Scans, preview textures and occlusion geometry | `Assets/Editor/Vuforia/`, `Assets/StreamingAssets/Vuforia/` | Vuforia Area Target export format. Capture device/app/operator, ownership and asset license: Not documented. | Three final datasets; sofa runtime occlusion; older scans retained. |
| Project scripts, materials and custom shader | `Assets/Scripts/`, `Assets/Materials/`, `Assets/Shaders/` | Project-specific implementations/configurations. Sole authorship, AI assistance and explicit license: Not documented. | Procedural animation, expressions, interaction, UI, lighting. Custom occlusion shader is experimental. |

Media provenance assertions were inspected; their cryptographic signatures were
not independently validated. Installed Unity AI packages alone are not evidence
of AI authorship of scripts or artwork.

## Redistribution review

- [Vuforia Developer Agreement](https://developer.vuforia.com/legal/vuforia-developer-agreement)
  sections 3 and 4 distinguish permitted distribution of runtime components
  integrated into an application from distribution of the standalone SDK. The
  SDK archive already exists in this repository's history. This submission does
  not establish separate permission to redistribute that archive or relicense it.
- Each person reopening the project must obtain their own applicable Vuforia
  authorization/key. The live key and Apple/Android signing credentials are not
  submission assets.
- Rights to redistribute the model, emblem, current portrait, some imported sample
  assets, captured environments and generated audio are not established by local
  documentation. Obtain/retain original licenses, ownership records and output
  terms before relying on broader redistribution rights.
- Preserve all existing package, font and sample notices. No blanket MIT, GPL,
  or other project-wide license has been added over these materials.
- The package archive is pinned for reproducibility. Its presence is not evidence
  that proprietary SDK or third-party asset redistribution rights have been cleared.

## Installed support packages

Local package notices identify Unity Terms of Service for AI Assistant/AI Inference;
Unity Companion License for AI Navigation, Timeline, Test Framework, App UI and
Shader Graph; Unity Package Distribution License for Visual Scripting/Version
Control; and MIT for the Rider/Visual Studio editor integrations. Their presence
does not establish an active runtime feature. The exact dependency graph is
recorded in `Packages/packages-lock.json`.
