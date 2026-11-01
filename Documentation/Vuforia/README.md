# Vuforia configuration setup

The live `Assets/Resources/VuforiaConfiguration.asset` and its `.meta` file are
local files ignored by Git because the configuration contains license keys.
`VuforiaConfiguration.asset.example` preserves this project's nonsecret settings
with both license-key fields empty. Keep the example outside Unity `Resources`
folders; it is not a runtime configuration.

After cloning:

1. Download Git LFS files, including the Vuforia package (`git lfs pull`), before
   opening the project. Use the Unity version in `ProjectSettings/ProjectVersion.txt`.
2. With Unity closed, copy `Documentation/Vuforia/VuforiaConfiguration.asset.example`
   to `Assets/Resources/VuforiaConfiguration.asset`, removing only the `.example`
   extension. Create `Assets/Resources` if needed. If a live configuration already
   exists, keep it and review its settings instead of overwriting it.
3. Open Unity and let package resolution and asset import finish. Unity creates
   the local `.meta` file. Open **Window > Vuforia Configuration** and enter your
   own authorized Vuforia application license key in **App License Key**. Obtain
   it from the Vuforia Engine Developer Portal's **Plan & Licenses** section.
   Let Vuforia manage its internal license fields; do not reuse another person's
   serialized keys. Review and accept applicable terms yourself if prompted.
4. Save the configuration and check that Vuforia initializes in Play Mode and on
   the target device. An empty example does not provide a working license.
5. Before committing, run `git check-ignore -v -- Assets/Resources/VuforiaConfiguration.asset Assets/Resources/VuforiaConfiguration.asset.meta`
   and confirm both local files are ignored. Do not force-add either file or paste
   license keys into source files, this example, logs, screenshots, or GitHub issues.

The example retains the source configuration's serialized settings, including
version and acceptance metadata; it does not grant a license or replace accepting
Vuforia's terms. When sharing updated settings, refresh the example only after
clearing both `vuforiaLicenseKey` and `ufoLicenseKey` values.

References: [Vuforia license management](https://developer.vuforia.com/library/vuforia-engine/getting-started/engine-developer-portal/vuforia-license-manager/)
and [Vuforia configuration](https://developer.vuforia.com/library/vuforia-engine/getting-started/vuforia-engine-api/vuforia-engine-api-unity/).
