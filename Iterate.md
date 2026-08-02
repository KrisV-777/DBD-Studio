# DBD Studio – Mutagen Integration Specification

## Objective

Implement the backend foundation for Skyrim plugin inspection using Mutagen.

The goal is to create a fast, searchable, shared form database that can be used by:

- Load Order Explorer
- Form Search controls
- Rule Editor
- Rule Preview
- Future validation systems

This implementation replaces mock form data with real Skyrim data.

---

# Supported Games

DBD Studio exclusively supports:

- Skyrim Special Edition
- Skyrim Anniversary Edition
- Skyrim VR

No support is required for:

- Skyrim Legendary Edition
- Fallout games
- Other Bethesda games

The implementation should use Mutagen's Skyrim support.

---

# Design Principles

## UI Responsiveness Is Highest Priority

The database must be optimized for UI usage.

Do not load every complete record into memory at startup.

The application will likely inspect thousands of records, but only a small fraction will ever be opened in detail.

The system should therefore use:

```
Fast searchable index
        +
Lazy record resolution
```

---

# Architecture

Mutagen must not be exposed directly to the UI.

The architecture should be:

```
Avalonia UI

    ↓

ViewModels

    ↓

Application Services

    ↓

DBD Studio Domain Models

    ↓

Mutagen Implementation

    ↓

Skyrim Plugins
```

The UI must never directly access:

- Mutagen record types
- Plugin files
- Load order APIs

---

# Required Services

Create interfaces in the Core project.

---

## ILoadOrderService

Responsible for discovering the current Skyrim environment.

Responsibilities:

- Locate Skyrim Data folder
- Locate installed plugins
- Read load order
- Create Mutagen load order

Example API:

```
Initialize(gamePath)

GetLoadedPlugins()

Refresh()
```

---

## IFormDatabaseService

Primary interface used by the rest of the application.

Responsibilities:

- Search forms
- Resolve FormKeys
- Retrieve metadata
- Provide lazy record access

Example API:

```
Search(query)

Get(FormKey)

GetByEditorID(editorID)

GetByFormID(formID)
```

---

# Form Database Design

The database must contain information about every record.

However, records should be indexed minimally.

The default database entry should contain:

Required fields:

```
Display Name

Editor ID

Form ID

Plugin

Form Type

FormKey
```

Example:

```
Name:
Lydia

EditorID:
HousecarlWhiterunLydia

FormID:
000A2C8E

Plugin:
Skyrim.esm

Type:
NPC
```

---

# Form Identity

Never store FormIDs without plugin context.

Invalid:

```
000A2C8E
```

Correct:

```
Skyrim.esm | 000A2C8E
```

Use Mutagen's FormKey concept internally.

The domain model should wrap this.

Example:

```
FormReference

    Plugin

    FormID

    EditorID

    Type
```

---

# Indexing Strategy

At startup:

Do NOT fully parse every record.

Instead:

Create lightweight metadata entries.

Example:

```
Load Plugin
    |
    |
Enumerate Records
    |
    |
Create FormIndexEntry
    |
    |
Store searchable metadata
```

---

# Lazy Record Loading

Full records should only be loaded when required.

Examples:

User opens:

```
Lydia
```

Then:

```
FormIndexEntry
        |
        |
Resolve NPC record
```

Do not resolve every NPC during startup.

---

# Important Record Types

The database must support all record types.

However, the following are especially important.

---

## NPC_

Required for:

- ActorBase conditions
- Unique NPC rules
- Reference previews

Store:

- Name
- EditorID
- FormID
- Plugin

---

## RACE

Required for:

Conditions:

```
Race == Nord
```

---

## FACT

Required for:

Conditions:

```
Faction == Companions
```

---

## KYWD

Required for:

Keyword-based conditions.

---

## All Other Records

All record types should still be indexed.

Examples:

- Armor
- Head Parts
- Texture Sets
- Weapons
- Quests
- Globals
- Spells
- etc.

The system should not have a hardcoded limitation to only important types.

---

# Search System

Search must be optimized for interactive UI use.

Supported search:

- Display Name
- Editor ID
- Form ID
- Plugin

Examples:

```
Lydia

HousecarlWhiterunLydia

000A2C8E

Skyrim.esm
```

---

# Search Result Model

Do not return raw Mutagen records.

Return domain objects.

Example:

```
FormSearchResult

    DisplayName

    EditorID

    FormID

    Plugin

    RecordType
```

---

# Load Order Handling

Mutagen should use the currently visible Skyrim installation.

Do not implement:

- MO2 profile discovery
- MO2 API integration
- Virtual filesystem handling

MO2 support is provided by launching DBD Studio through MO2.

The application should behave like:

```
MO2

    ↓

DBD Studio.exe

    ↓

Mutagen scans visible Data folder
```

This matches the workflow used by tools such as BodySlide.

---

# File Paths

Continue using explicit user-configured paths.

Required paths:

```
Skyrim Data Folder

Mods Folder

BodySlide Presets

RaceMenu Presets
```

The Mutagen implementation should consume these paths.

Do not attempt automatic discovery.

---

# Caching

The database should support caching.

Goal:

Avoid rescanning plugins unnecessarily.

Possible cache invalidation:

- Plugin timestamp changed
- Plugin size changed
- Load order changed

The exact cache implementation is flexible.

---

# Threading

Database loading must not block the UI thread.

Required:

```
Application Start

        |

Background Load

        |

Database Ready Event

        |

UI Updates
```

The UI should remain responsive while loading.

---

# Error Handling

Handle:

Missing plugins

Broken plugins

Invalid paths

Missing Skyrim installation

Show useful errors.

Do not crash the application.

---

# Future Requirements

The architecture should support future features:

- Rule validation
- Form conflict detection
- YAML generation
- Actor preview
- Condition autocomplete
- Asset dependency checking

---

# Implementation Order

Implement in this order:

## Step 1

Create domain models:

```
FormReference

FormSearchResult

FormRecordMetadata
```

---

## Step 2

Create interfaces:

```
ILoadOrderService

IFormDatabaseService
```

---

## Step 3

Implement Mutagen-backed services.

---

## Step 4

Create indexing pipeline.

---

## Step 5

Connect Load Order Explorer to the real database.

---

## Step 6

Replace all mock Form Search controls.

---

# Completion Criteria

The implementation is complete when:

- DBD Studio can load a Skyrim SE/AE/VR installation
- Plugins are discovered through configured paths
- All records are searchable
- Search is responsive
- Form metadata is displayed correctly
- Load Order Explorer uses real data
- Form Search controls use the shared database
- UI remains responsive during scanning
- Mutagen types are isolated from the UI layer