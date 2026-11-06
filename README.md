# 🍃 Leaflet

> A high-performance, local-first Markdown note-taking application built natively in .NET using Avalonia UI and AvaloniaEdit.

![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet?style=flat-square)
![Avalonia UI](https://img.shields.io/badge/Avalonia-UI-blue?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

---

## ✨ Overview

**Leaflet** is designed for speed, privacy, and reliability. By keeping everything local-first and avoiding heavy web-wrapper bloat (like Electron), Leaflet provides a blazing-fast Notion-style live-preview editing experience alongside professional-grade desktop features.

---

## 🚀 Key Features

### 1. ⚡ Local-First Vault Storage (`VaultStore`)
* **File-System Native:** Direct integration with your local machine's directories (`LeafletVault`). No cloud lock-in, your notes live safely on your disk.
* **Instant Scanning:** Automatically indexes and manages markdown files upon startup.

### 2. 📝 Interactive Live-Preview Editor
* **Caret-Aware Styling:** Lines transform dynamically as your cursor moves. The line you are actively editing remains in raw Markdown for easy typing, while other lines render rich syntax styling (headers, bold, italics).
* **Interactive Checkboxes:** Native interactive checkboxes (`- [ ]` / `- [x]`) rendered via custom element generators. Clicking a checkbox instantly toggles its state and updates the underlying file in real-time.

### 3. 📄 Headless SkiaSharp Vector PDF Export
* **Native Vector Engine:** Bypasses heavy headless browsers by utilizing **SkiaSharp** directly for lightning-fast, high-fidelity A4 vector PDF generation (`Ctrl + P`).
* **Smart Word-Wrapping & Pagination:** Automatically handles multi-page document layout, font sizes, margins, and clean section breaks.
* **Direct Export:** Automatically compiles and exports documents straight to your system's **Downloads** folder.

### 4. 🛡️ Single-Instance Management
* **OS Mutex & Named Pipes:** Ensures only one instance of Leaflet runs per vault, automatically routing subsequent launch arguments and focusing the active window.

### 5. 💾 Autosave Drafts & Crash Recovery
* **Debounced Background Drafts:** Automatically captures background snapshots 1.5 seconds after typing ceases.
* **Crash Detection & Restoration:** Uses startup lock files (`.leaflet.lock`) to detect ungraceful shutdowns or unexpected process terminations, automatically restoring your work upon relaunch.

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
| :--- | :--- |
| `Ctrl + B` | Bold selected text (or insert asterisks) |
| `Ctrl + I` | Italicize selected text |
| `Ctrl + P` | Export active note to PDF (saved to Downloads) |

---

## 🛠️ Tech Stack

* **Framework:** [.NET](https://dotnet.microsoft.com/)
* **UI Toolkit:** [Avalonia UI](https://avaloniaui.net/)
* **Text Editing Engine:** [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
* **Graphics & PDF Rendering:** [SkiaSharp](https://github.com/mono/SkiaSharp)

---

## 🚀 Getting Started

### Prerequisites
* [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Running Locally
1. Clone the repository:
   ```bash
   git clone https://github.com/preshstoner/Leaflet-Note.git
   cd leaflet/Leaflet
Run the application:

Bash
dotnet run
📄 License
Distributed under the MIT License. See LICENSE for more information.
