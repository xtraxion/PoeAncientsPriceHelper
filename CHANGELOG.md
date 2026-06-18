# Changelog

All notable changes to **Poe Ancients Price Helper** are documented here.
The format is loosely based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.8] — 2026-06-14

### Fixed
- Calibration on **mixed-DPI multi-monitor** setups (e.g. a 100% primary + a 175% secondary).
  Calibrating on the high-DPI monitor used to produce a wrong region — size shrunk by the scale
  factor, origin off-screen — so the overlay showed nothing. The app now declares Per-Monitor-V2 DPI
  awareness at startup and captures the calibration box in true physical screen pixels, so the region
  matches exactly where you drew it. (#8, reported & verified by @ljere)
- Calibration instructions now stay pinned to the primary monitor instead of occasionally rendering
  on the secondary screen.

## [1.1.7] — 2026-06-12

### Changed
- Price fetches now time out after 15s and retry on the next cycle instead of blocking for up to
  100s when poe.ninja is slow or a connection stalls.
- The config file is now written atomically, so a crash or power loss mid-save can no longer corrupt
  it and reset your region/hotkeys to defaults.
- The diagnostic `debug_ocr.png` dump is only written in debug mode, so normal users no longer get a
  file rewritten in their folder while a panel is open.
- Removed dead code and tightened shutdown so an in-flight fetch is cancelled cleanly.

Most of this hardening came out of a community code audit (thanks to @crichmond1989).

## [1.1.6] — 2026-06-12

### Added
- **Uncut gem prices** — uncut skill, spirit, and support gems are now priced by exact gem type
  **and level** (e.g. `Uncut Spirit Gem (Level 19)` priced as a level-19 spirit gem). A row shows
  **?** rather than a guessed price if the type or level can't be read cleanly, then fills in once a
  clean read arrives — neighbouring levels can differ several-fold in value.
- **Update notifications** — on startup the app checks GitHub for a newer release and shows an
  "Update available" link in the window when one exists.

## [1.1.5] — 2026-06-12

### Added
- **Configurable hotkeys** for Start/Stop, Debug overlay, and Calibrate, each independently
  rebindable (defaults unchanged: F5 / F3 / F4). Binding a key already used by another action is
  rejected. (#4, #6)
- **Minimize to tray** — minimizing sends the app to a system-tray icon and keeps scanning in the
  background; double-click (or right-click → Show) to restore, right-click → Exit to quit. (#2)
- **Multi-monitor support** — calibrate the region on any monitor, and the overlay appears on the
  monitor your game runs on instead of only the primary one. (#3)

### Fixed
- Overlay flicker — fixed a post-Escape re-flash where prices briefly reappeared after closing the
  panel, and added brightness hysteresis so the overlay no longer blinks near the detection
  threshold. (#5)

## [1.1.4] — 2026-06-09

### Changed
- Each price now sits on a subtle semi-transparent dark plate behind the icon and number, so values
  stay legible over busy in-game art. The overlay was reworked to true per-pixel transparency.

## [1.1.3] — 2026-06-08

### Added
- Configurable Start/Stop hotkey — F5 is now just the default; rebind it in-app and it takes effect
  immediately and persists.

### Changed
- Start/Stop now uses the same global key listener as the other hotkeys, so it no longer clashes with
  other apps that grab the same key.

## [1.1.2] — 2026-06-08

### Added
- **Hardcore league pricing** — the League dropdown now offers **HC Runes of Aldur**, with prices
  pulled from the matching poe.ninja economy and correctly denominated (softcore in divine, hardcore
  in exalted).
- The running version is now shown in the bottom-left of the window.

## [1.1.1] — 2026-06-08

### Added
- F5 hotkey to start/stop scanning without clicking the window.

### Fixed
- Prices now always use a `.` decimal separator on every system locale (previously showed e.g.
  `0,1` on comma-decimal regions).

## [1.1.0] — 2026-06-08

First public release.

### Added
- Live poe.ninja prices overlaid on the in-game currency list.
- One-time calibration; stack-aware pricing (e.g. `2 (0.5 each)`).
- Self-contained Windows x64 build.

[1.1.8]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.8
[1.1.7]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.7
[1.1.6]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.6
[1.1.5]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.5
[1.1.4]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.4
[1.1.3]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.3
[1.1.2]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.2
[1.1.1]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.1
[1.1.0]: https://github.com/pedro-quiterio/PoeAncientsPriceHelper/releases/tag/v1.1.0
