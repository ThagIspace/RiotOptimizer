# ValorantResFix v2.0

A lightweight utility to adjust VALORANT internal `ScalabilityGroups` settings (such as `ResolutionQuality`, `AntiAliasingQuality`, and graphics properties) to boost FPS for low-end PCs and laptops.

![Preview](https://user-images.githubusercontent.com/35301902/127652037-af681a9b-b11f-4f11-bbc8-13fe69ee4ae2.PNG)

---

## 🚀 What's New in v2.0
- **Modern VALORANT Support:** Automatically scans and detects config files in both modern (`WindowsClient`) and legacy (`Windows`) folders across all player accounts.
- **Crash Fix:** Fixed crash when `sg.ResolutionQuality` was omitted from `GameUserSettings.ini` by default.
- **UI Bug Fixes:** Fixed field mapping bug for `ShadingQuality` (previously bound to `ShadowQuality`).
- **About Window:** Implemented functional About dialog with author credits and project links.
- **Improved Backup & Error Handling:** Safer parsing and graceful backup restoration.

---

## 🛠️ How It Works
Inside your `%LOCALAPPDATA%\VALORANT\Saved\Config` directory, Unreal Engine stores `GameUserSettings.ini`. Some internal graphics scalability properties cannot be changed below default minimums in the in-game UI. 

ValorantResFix provides an easy, clean GUI to adjust these settings safely, set custom resolution scaling (e.g. 60% or 70%), and apply optimal low settings with one click.

---

## 📦 How to Use
1. Close VALORANT before running the tool.
2. Download or extract the release archive (`ValorantResFix_v2.0.zip`).
3. Run `ValorantResFix.exe`.
4. Click **Apply Low Settings** for instant FPS boost, or manually adjust values and click **Save**.
5. Launch VALORANT and enjoy higher FPS!

---

## 🛡️ Is it Safe?
- **100% Safe & Clean:** No malware, no background telemetry.
- **Vanguard Compliant:** Only modifies standard client text configuration files (`.ini`). Does not touch RAM, inject code, or modify game binaries.

---

## 🤝 Credits & Acknowledgments
- **Original Author & Project:** [xFahim](https://github.com/xFahim) ([xFahim/ValorantResFix](https://github.com/xFahim/ValorantResFix))
- **v2.0 Maintained & Updated by:** [ThagIspace](https://github.com/ThagIspace) ([ThagIspace/ValorantResFix](https://github.com/ThagIspace/ValorantResFix))
