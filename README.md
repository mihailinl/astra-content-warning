# Astra for Content Warning

A game integration for Astra on the [Astra Unity foundation](https://github.com/mihailinl/astra-bepinex): it makes Astra better in Content Warning.

## Install with Astra

Paste this repository's address into Astra's **Games** tab:

```
https://github.com/mihailinl/astra-content-warning
```

Astra fetches the latest release's `astra-gi.zip`, installs it into a profile of its own and
launches Content Warning with it when you press Play. Integrations are not reviewed by Astra and
are always shown as **Experimental**.

## Build

No game folder needed: every game type (`Player`, `PlayerData`, `MainCamera`, `VideoCamera`,
`Mirror`) is reached by NAME at run time through the Astra SDK's `Astra.Sdk.GameType`, never
compiled against — this builds from public packages alone (the Astra SDK, `BepInEx.Core`,
`UnityEngine.Modules`).

```bash
dotnet build -c Release
```

A tagged push (`v*`) builds and publishes `astra-gi.zip` to a GitHub release through
`.github/workflows/release.yml`. To build the same zip locally (no CI, for testing the layout):

```bash
# once: unpack the pinned BepInEx release beside this repo's own Release build
curl -LO https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip
unzip BepInEx_win_x64_5.4.23.5.zip -d bepinex-dist
# build the Astra foundation once (astra-bepinex/tools/pack.sh), then:
tools/make-astra-gi.sh
```

To build against a local, unreleased Astra SDK before it is on nuget.org: add a
`GameDir.props.user` (git-ignored) setting `RestoreAdditionalProjectSources` to the SDK's local
feed path, or uncomment the `astra-local` source in `nuget.config`.

## Install (by hand, until the Astra marketplace installs it)

Copy `bin/Release/*/AstraContentWarning.dll` and `astra-item.json` into
`<game>/BepInEx/plugins/AstraContentWarning/`. The game also needs BepInEx and the Astra foundation in
`BepInEx/plugins/Astra/`. **Never ship the foundation's DLLs with this plugin**: it is installed once
per game and updates by itself.

## Rules of the road

- Send raw facts (`climbing = true`), never animation names for movement: her animation set decides
  what a fact looks like, and a pack author can change it without touching code.
- Never touch rendering, the socket or the frame ring. The foundation owns them; if you need
  something from them, open an issue on the foundation.
- Single-player and co-op games without anti-cheat only.

License: MIT.
