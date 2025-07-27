# SnapX Plugin Runtime (Experimental, known as Drift)

This project serves as an experimental JavaScript runtime designed for the SnapX application. It is used to evaluate and run plugin scripts written by the community, using modern JavaScript syntax and features.

## Purpose

SnapX is extending its capabilities with a flexible plugin system. This runtime allows plugins to be written in modern ECMAScript (such as ES2025), and executed inside the SnapX environment. This project will eventually support all SnapX plugins and provide a consistent execution context across all platforms.

## JavaScript Support

Plugins are expected to be written in modern ECMAScript (up to ES2025), and then compiled/transpiled down to ECMAScript 2023 before distribution. This allows plugin authors to write future-proof code while ensuring compatibility with the runtime.

It should support:
- ECMAScript 2023 features (e.g., `findLast`, `toSorted`, `Array.prototype.with`, error cause propagation)
- Standard OOP coding with `import` statements across multiple files
- While Jint supports sandboxing, it's unknown how I'd design it yet.
- I hopefully want to make sure plugin behavior can be audited by end users.

## Node.js Compatibility

The goal is to implement a subset of the Node.js core APIs (such as `fs`, `path`, `process`, etc.) to allow bundled modules compiled with tools like Webpack, esbuild, or Rollup to work as expected. This means authors can use standard Node.js libraries as long as they are bundled for runtime execution.

> ⚠️ Not all modules will be implemented, and some may be **polyfilled** or **stubbed**. Use common libraries that gracefully degrade if possible.


## Plugin Requirements

All SnapX plugins must adhere to the following guidelines:

- **License**: All plugins must be licensed under **GPL v3 or later**
- **Source Code**: A link to the plugin’s source code must be provided (preferably hosted on GitHub, GitLab, or Bitbucket)
- All plugin code **must be bundled and transpiled** to **ECMAScript 2023** (or lower).
- Your plugin **_MUST!!!_** be written in TypeScript in order to keep your sanity.

## 🔐 Plugin Verification & Trust

To ensure the safety and authenticity of plugins in SnapX, all plugins must be **cryptographically verifiable**.

---

### 📄 File Integrity & Signing Requirements

Every plugin must meet the following criteria:

- ✅ **Each file inside the plugin ZIP must:**
    - Have a corresponding `.sha512` file (e.g. `main.js` → `main.js.sha512`).
    - Have a detached `.asc` OpenPGP signature (e.g. `main.js` → `main.js.asc`) created using your trusted key.

> ⚠️ This applies to **every single file** — not just the main files. Even metadata files or assets must be signed and hashed.

---

### ⚠️ What Happens Without Verification?

If a plugin lacks proper signatures and checksums:

**Users will see this warning:**

⚠️ WARNING: This plugin is unsigned and may be unsafe. SnapX cannot verify who made it or if it was tampered with.
Proceeding is NOT recommended unless you fully trust the source.

> 🛑 **This is not like a typical unsigned executable warning you might see on Windows.**  
> In SnapX, developers can **freely prove their identity** using cryptographic keys tied to platforms like GitHub, Keybase, Mailvelope, or email.  
> If they haven’t taken this **basic verification step**, we strongly discourage trusting or installing their plugin — especially if it's distributed outside of official channels or our plugin registry.

### 🧾 Ways to Reduce the Warning Severity

The scary warning will become more **neutral** if:

- The plugin is signed using a **Keybase** account that has proof of owning the OpenPGP Key.
- The author's OpenPGP key is **published on Mailvelope’s keyserver** (https://keys.mailvelope.com).
- The plugin follows all structural guidelines and hash signing expectations.

The warning will look like this:

> ℹ️ **This plugin is signed but the publisher’s identity is not fully verified.**  
> The plugin is safe from tampering, but please only install it if you trust the source.

### 🌐 Verified Publishers

Developers can **upgrade to Verified Publisher** status by submitting their identity for review.

- **One-time fee:** $20 USD
- **Privacy-friendly:** We use a minimal and ethical identity verification method.
- **Perks:**
    - No warnings shown during plugin installation.
    - A **checkmark badge** will appear next to the plugin and author name, signaling trustworthiness to users.
    - Listed in the SnapX registry as a **Trusted Publisher**.
    - Plugins are favored in search results and trending lists. (ranked by a scoring system that balances Verified status with user ratings, recent updates, and overall popularity to ensure fairness)
    - Verified Publishers can formally contest and respond to reviews on their plugins, with contested reviews verified by moderators to ensure fairness.
    - Given Trusted Publisher role in SnapX Discord server.
    - Helps offset **infrastructure and administrative costs** to keep SnapX free and community-focused.

## Plugin Structure

Plugins may consist of multiple `.js` files. There's no requirement to bundle everything into one file, though bundling is allowed.

The entry point (e.g., `main.js`) and related files should be declared clearly. SnapX will evaluate all relevant files in order.

```
plugins/
└── hello-world.zip
    ├── main.js                ← Compiled ES2023 JavaScript
    ├── main.js.sig            ← Signature for main.js
    ├── main.js.sha512         ← SHA-512 checksum of main.js
    ├── metadata.json5         ← Metadata: name, description, source link, license, icon, etc.
    ├── metadata.json5.sig     ← Signature for metadata.json5
    ├── metadata.json5.sha512  ← SHA-512 checksum of metadata.json5
    ├── icon.png               ← Plugin icon (recommended size e.g. 128x128 or 256x256)
    ├── icon.png.sig           ← Signature for icon.png
    ├── icon.png.sha512        ← SHA-512 checksum of icon.png
```

Example manifest

```json5
{
  displayName: "SnapX Hello World",
  id: "@BrycensRanch/snapx",
  version: "1.0.0",                // Semver format is required (https://semver.org/)
  author: {
    name: "Brycen G",
    contact: "brycengranville@outlook.com"
  },
  // Optional help/documentation resources
  help: {
    url: "https://github.com/BrycensRanch/SnapX",
    wiki: "https://github.com/BrycensRanch/SnapX/wiki",
    donate: "https://ko-fi.com/BrycensRanch"
  },
  // Although discouraged, you are always able to restrict your plguin from loading on certain platforms.
  // platforms: ["linux", "windows", "macos", "freebsd"],
  // Plugins rely on GPLv3 (or later) licensed code, so they must be licensed under GPL-3.0-or-later or a compatible license.
  // Recommended licenses are GPL-3.0-or-later, LGPL-3.0-or-later, or AGPL-3.0-or-later.
  
  // Use of permissive licenses (e.g., MIT, BSD-3-Clause, Apache-2.0) is discouraged!
  // They don’t fully protect the user freedoms that copyleft licenses ensure.
  // Choosing a strong copyleft license helps preserve these freedoms and maintains consistency with SnapX.
  license: "GPL-3.0-or-later", // Required. Must be a valid SPDX license identifier.
  
  // Optional but strongly recommended. Should point to the plugin’s public source code repository.
  source: "https://github.com/BrycensRanch/SnapX",

  // Localized descriptions - keys are BCP 47 language tags
  // All description fields support Markdown.
  description: {
    en: "A simple Hello World plugin for SnapX.",
    es: "Un plugin Hola Mundo sencillo para SnapX.",
    fr: "Un plugin Bonjour le Monde simple pour SnapX.",
    zh: "SnapX 的一个简单问候插件。"
  },

  // Icon file relative path inside the plugin ZIP (use POSIX-style forward slashes `/`).
  // Backslashes (`\`) from Windows paths will be normalized to forward slashes for cross-platform compatibility.
  icon: "icon.png",

  // Path to the entry point file inside the zip archive.
  // Always use forward slashes (e.g. "src/main.js"), even on Windows.
  main: "main.js",

  // Permissions required by plugin (for future use)
  permissions: [
    "read_files",
    "write_files",
    "network_access"
  ],

  // Compatible SnapX runtime versions (semver range)
  engineVersion: ">=1.0.0 <2.0.0",

  // Categories/tags to aid discovery
  categories: ["utility", "example", "hello-world"],

  // Added automatically at the compilation step,
  build: {
    timestamp: "2025-07-26T20:00:00Z",
    // If builder machine sha512 matches your local sha512 machine id, it will not show warnings about untrusted dev
    // SHA-512 was chosen for its strong cryptographic security and resistance to collision attacks.
    // Its 512-bit output provides a large security margin against current and foreseeable quantum computing attacks.
    builderMachineIdSha512: "b109f3bbbc244eb82441917ed06d618b9008dd09b3befd1bde6d06cf5df74f6a3c8ae3b30a65812bb0db7357b7d158a104b8ff4b3f95d8ffaf02393e71e9ee97"
  },
/*
// Telemetry section describes what usage data the plugin may collect if telemetry is enabled.
// If omitted, it is assumed the plugin does not collect any telemetry data.
// IMPORTANT: If telemetry is disabled globally in SnapX, telemetry is disabled for all plugins automatically.
// Additionally, users can disable telemetry individually for each plugin via SnapX settings.
telemetry: {
  description: "Collects anonymous usage data to improve plugin features.",
  url: "https://snapx.org/privacy",
  // The `level` field indicates the sensitivity of telemetry data:
  // - "none": no telemetry collected.
  // - "basic": anonymous usage stats only (e.g., feature usage counts, error rates).
  // - "detailed": collects more comprehensive diagnostic and environment information,
  //    such as OS version, SnapX version, plugin configuration settings, performance metrics,
  //    and anonymized hardware details.
  //    This data helps developers diagnose issues and improve plugin compatibility,
  //    but it does NOT include personal user data or content.
  // - "personal": may collect personal or identifiable information (heavily discouraged!).
  level: "basic",
  sellsData: false, // true if data is sold or shared. Automatically excludes you from the plugin registry.
  salesDescription: "" // required if sellsData is true, describing what data is sold and to whom
},
*/
  
// List of primary files included in the plugin ZIP archive (use POSIX-style forward slashes).
// These should be the core plugin files required for loading and execution.
// Do NOT include signature (.sig) or checksum (.sha512) files — those are handled automatically.
files: [
  "main.js",
  "metadata.json5",
  "icon.png"
],
// Plugin type: "gui", "core", or other recognized roles
type: "gui",
// Plugins that must be loaded before this one
loadBefore: [],
// List of plugins that are incompatible with this one
incompatibleWith: ["legacy-annotator"],
hooks: {
  onAppStart: {
    handlers: ["initAnalytics"],
    explanation: "Initialize analytics and cache plugin data when the app starts."
  },
  beforeUpload: {
    handlers: ["resizeImage", "addWatermark"],
    explanation: "Optimize image size and apply watermark before uploading."
  },
  onUploadComplete: {
    handlers: ["notifyUser"],
    explanation: "Notify user and copy the uploaded image URL to clipboard."
  }
},
// Declares what settings this plugin contributes to which config structures
configExtensions: [
  {
    target: "ApplicationConfig",
    settings: [
      {
        name: "advancedAnnotation",
        type: "boolean",
        default: false,
        description: "Enables enhanced annotation tools in the editor."
      },
      {
        name: "cloudBackup",
        type: "string",
        default: "https://backup.snapx.org",
        description: "Optional endpoint to back up plugin data."
      }
    ]
  },
  {
    target: "UploadersConfig",
    settings: [
      {
        name: "autoWatermark",
        type: "boolean",
        default: true,
        description: "Automatically apply watermark to uploads."
      }
    ]
  }
]
}
```

## 🧪 Plugin Publishing Experiment for SnapX

## 🧠 Core Design Goals

- Prevent name collisions and impersonation
- Enable decentralized fallback and offline-first usage
- Detect and block suspicious or malicious plugins
- Keep publishing simple and traceable for developers

---

## 🔐 Auth-Based Identity (Planned)

Plugin authors must log in with:

- ✅ GitHub
- ✅ Discord

No passwords. All plugin uploads will be associated with a verifiable identity.

---

## 🏛 Central Plugin Registry (Planned)

A central registry will serve as the **source of truth** for plugin metadata and name ownership.

- Enforces **unique global names**
- Supports **namespaced publishing** (e.g., `@BrycensRanch/my-plugin`)
- Stores plugin metadata, version history, digital signature info
- Validates uploaded ZIPs and maintains a searchable index

---

## 📛 Plugin Naming Rules (Proposed)

| Name Type | Rule                                               |
|-----------|----------------------------------------------------|
| Global Name | Must be unique (e.g., `awesome-ocr`)               |
| Namespaced Name | Always allowed (e.g., `@BrycensRanch/awesome-ocr`) |

---

## 📌 Name Reservation

Plugin names can be **reserved** for 72 hours during development, to avoid last-minute name sniping. If the name is unavailable, you can still publish under your own namespace.

---

## 📦 Plugins as ZIP Files

All plugins are bundled as standard `.zip` archives.

Each plugin ZIP contains:
- A signed manifest (`manifest.json5`)
- Signed Files (main.js, png, etc.)

This format makes distribution portable and easy to verify.

---

## 🛰 Offline Plugin Manifest Caching

When SnapX is **built with an internet connection**, it will:

- Download and embed a **snapshot of the plugin registry** into the build
- This cached manifest enables **offline plugin browsing**, including names, authors, descriptions, and download URLs

> If our servers go offline, SnapX users can still discover and install plugins that are hosted elsewhere — like GitHub — as long as they’re listed in the last cached manifest.

---

## 🔏 Optional Plugin Signing

Plugin authors may optionally sign plugin archives using a public/private keypair.

- The plugin registry binds your **public key** to your plugin
- SnapX will verify the signature at install time
- Prevents tampering and spoofed updates

---

## 🛡️ Plugin Security & Malware Detection (Planned)

To protect users, all uploaded plugins will be scanned for suspicious behavior patterns, including:

- ❗ Minified or obfuscated code without source maps
- ❗ Use of `eval`, `Function`, or dynamic code execution
- ❗ Extrapolated strings or suspicious escape sequences
- ❗ Known malicious payloads or telemetry abuse

> Suspicious plugins may be blocked or flagged for review before being listed in the registry.

---

## 👮 Moderation & Disputes

- Community reports can flag plugins for review
- Plugin takedowns may occur in cases of impersonation, malware, or broken functionality
- Plugin name disputes will be resolved based on first-claim or reservation ownership


## 🧪 Current Status

This is a **design experiment** — not a production service (yet). Everything here is subject to change based on testing, feedback, and community needs.

---

## 💬 Feedback Welcome

Got thoughts or want to help shape this system? Open an issue or chat with us on Discord.

We’re building SnapX to be **open, secure, and community-first.**