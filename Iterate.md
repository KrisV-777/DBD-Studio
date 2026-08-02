# DBD Studio – Core Architecture & Domain Specification

## Purpose

This document defines the core architecture and domain model of **DBD Studio**.

The UI prototype is considered complete.

This implementation phase establishes the backend architecture that all future features will build upon.

The goals of this phase are:

* Establish a shared domain model.
* Replace mock form data with a Mutagen-backed implementation.
* Create a fast searchable form database.
* Prepare the rule engine.
* Keep the UI completely independent from backend implementation details.

This document supersedes previous backend implementation notes where they conflict.

---

# Design Principles

The following principles are mandatory.

## UI Independence

Avalonia Views and ViewModels must **never** depend directly on Mutagen types.

The UI should communicate exclusively through domain models and service interfaces.

Correct architecture:

```
Avalonia Views

        ↓

ViewModels

        ↓

Application Services

        ↓

Domain Models

        ↓

Mutagen
```

Mutagen must remain an implementation detail.

---

## MVVM

Continue using MVVM.

Avoid code-behind except for unavoidable Avalonia-specific behavior.

---

## Dependency Injection

All services should be registered through dependency injection.

Avoid static classes.

Avoid global state.

---

## Shared Domain

The application should expose a single shared domain model.

Every ViewModel should operate on the same shared objects.

The following pages should never duplicate data:

* Load Order Explorer
* Rule Editor
* Rule Preview
* Texture Packs
* BodySlide Presets
* RaceMenu Presets

---

# Solution Structure

Recommended project layout:

```
DBDStudio.sln

DBDStudio.Core
    Models/
    Interfaces/
    Services/

DBDStudio.Infrastructure
    Mutagen/
    AssetScanning/
    Persistence/

DBDStudio.UI
    Views/
    ViewModels/
    Controls/
```

The Core project must not depend on Avalonia.

Infrastructure contains all interaction with external libraries.

The UI depends only on Core.

---

# Supported Games

DBD Studio exclusively targets:

* Skyrim Special Edition
* Skyrim Anniversary Edition
* Skyrim VR

No support is required for other Bethesda titles.

---

# Skyrim Environment

Users are expected to launch DBD Studio through Mod Organizer 2.

The application should not attempt to detect or interface with MO2.

The application should simply operate on the filesystem that is visible to the current process.

Continue using user-configurable paths:

* Skyrim Data Folder
* Mods Folder
* BodySlide Presets Folder
* RaceMenu Presets Folder

---

# Domain Model

## Workspace

The workspace represents the editable project.

Suggested model:

```
Workspace

    Settings

    TexturePacks

    BodySlidePresets

    RaceMenuPresets

    Rules
```

The workspace contains editor-specific information.

---

## Workspace File

The application should save its editable state inside a dedicated workspace file.

Suggested extension:

```
*.dbdproj
```

The exact serialization format is flexible.

The workspace should contain:

* Settings
* Registered texture packs
* Imported assets
* Rules
* Editor state (optional)

The workspace is intended for DBD Studio only.

---

# Export Model

DBD does **not** consume workspace files.

Export produces DBD-compatible YAML.

Export output:

```
Data/

    SKSE/

        DBD/

            Rules/

                *.yaml
```

Each rule exports as one YAML file.

Texture packs export independently.

---

# Texture Packs

Texture packs are independent assets.

Each texture pack has:

```
Name

Mappings
```

A mapping consists of:

```
Vanilla Texture

↓

Replacement Texture
```

Mappings must be injective.

Each vanilla texture may map to at most one replacement.

---

## Mapping Resolution

When DBD loads a texture pack:

If:

```
config.yaml
```

exists,

explicit mappings are used.

Otherwise,

implicit mappings are generated:

```
textures/foo.dds

↓

textures/dbd/<TexturePack>/foo.dds
```

---

## Export

DBD Studio should always generate:

```
config.yaml
```

inside each texture pack.

The generated mapping should explicitly contain every texture mapping.

---

# BodySlide Presets

BodySlide presets are opaque identifiers.

DBD does not inspect preset contents.

A preset is uniquely identified by:

```
XML File

+

Preset Name
```

Example:

```
CBBE.xml:CBBECurvy
```

---

# RaceMenu Presets

RaceMenu presets are independent assets.

Required:

```
JSLOT

Sex
```

Optional:

```
DDS

NIF
```

The preset identifier is the JSLOT filename.

Sex is mandatory.

DBD requires this information to prevent applying male presets to female actors and vice versa.

---

# Rules

A Rule is the primary export unit.

Each exported YAML file represents one Rule.

A Rule contains:

```
Conditions

Texture Candidates

BodySlide Candidates

RaceMenu Candidates
```

Each candidate list may contain:

```
0

1

many
```

entries.

---

# Assignments

Texture Packs

BodySlide Presets

RaceMenu Presets

are independent assignment categories.

The rule engine must never assume one global winner.

Instead implement three independent assignment pipelines.

```
Texture Resolver

BodySlide Resolver

RaceMenu Resolver
```

Each resolver evaluates the same conditions independently.

---

# Candidate Selection

If a Rule wins for a category:

and multiple candidates exist:

```
Texture A

Texture B

Texture C
```

DBD randomly selects one candidate.

Randomization only occurs inside the winning Rule.

Rules themselves are never selected randomly.

---

# Rule Evaluation

Rules evaluate as:

```
Condition Groups

AND

Condition Groups

AND

...
```

Each group contains one or more OR-connected conditions.

Example:

```
Condition A

OR

Condition B

AND

Condition C
```

evaluates as:

```
(A || B) && C
```

No additional expression syntax exists.

No arbitrary nesting is required.

---

# Conditions

A Condition contains:

```
Type

Comparator

Value
```

All Conditions evaluate to:

```
true

false
```

---

## Supported Conditions

The implementation should support future conditions.

Do not hardcode the UI around today's condition set.

Current conditions:

| Condition   | Priority | Value Type     |
| ----------- | -------: | -------------- |
| Race        |        0 | Form           |
| Sex         |        0 | Boolean        |
| Level       |        0 | Integer        |
| Keyword     |        1 | Form           |
| Faction     |        2 | Form           |
| FactionRank |        3 | Form + Integer |
| ActorBase   |        4 | Form           |
| ReferenceID |        5 | Form           |

---

## Comparison Operators

All conditions support the following operators:

```
<

<=

==

>=

>

!=
```

No validation should prohibit operators that are not semantically meaningful.

The UI should expose every operator uniformly.

---

# Condition Registry

Introduce a Condition Registry.

The application should not hardcode condition-specific UI logic.

Suggested model:

```
ConditionDefinition

Name

DisplayName

Priority

ValueType

EditorType

UsesFormSearch
```

Examples:

```
Race

Priority:
0

ValueType:
FormReference

Editor:
FormPicker
```

```
Level

Priority:
0

ValueType:
Integer

Editor:
IntegerEditor
```

The Rule Editor should construct condition editors dynamically from this registry.

Adding a new condition in the future should require only:

* Register definition
* Implement evaluator

No UI modifications.

---

# Rule Priority

Rules do not explicitly store priority.

Priority is derived.

```
Rule Priority

=

Maximum Condition Priority
```

Example:

```
Race

Priority 0

Faction

Priority 2

ReferenceID

Priority 5
```

Rule priority becomes:

```
5
```

---

# Rule Resolution

Resolution for each assignment category:

1. Evaluate matching Rules.
2. Determine highest Rule priority.
3. If multiple Rules share the same priority:

   * Select the Rule whose filename is lexically last.
4. If the winning Rule contains multiple candidates:

   * Randomly select one candidate.

This process is repeated independently for:

* Texture Packs
* BodySlide Presets
* RaceMenu Presets

---

# Form Database

Create a shared Form Database service.

Every Form lookup should use this service.

Consumers include:

* Load Order Explorer
* Rule Editor
* Rule Preview
* Validation
* Future autocomplete

---

# Mutagen Integration

Mutagen has already been added to the solution.

Implement:

```
ILoadOrderService

IFormDatabaseService
```

---

# Database Goals

The database should expose every record.

However,

startup performance and UI responsiveness take precedence.

Do not fully materialize every record.

Instead:

```
Load Plugins

↓

Create lightweight metadata index

↓

Resolve records lazily
```

---

# Form Metadata

Each indexed record should minimally expose:

```
Display Name

EditorID

FormID

Plugin

FormKey

Record Type
```

This information should be sufficient for search results.

---

# Search

Supported search fields:

* Name
* EditorID
* FormID
* Plugin

Search must remain responsive on large load orders.

---

# Important Record Types

All records should be indexed.

The following receive special attention because they are used by conditions:

* NPC
* Race
* Faction
* Keyword

The architecture must not assume these are the only supported types.

---

# Threading

Plugin loading and indexing must not block the UI thread.

Expected workflow:

```
Application Startup

↓

Load Settings

↓

Background Plugin Scan

↓

Populate Shared Database

↓

Notify UI
```

---

# Caching

The database should be designed to support future caching.

Potential cache invalidation:

* Plugin timestamp changed
* Plugin size changed
* Load order changed

Implementation is optional during this phase.

The architecture should support it naturally.

---

# Future Development

After completion of this phase, future implementation should proceed roughly as follows:

1. Asset scanning
2. Rule evaluation engine
3. YAML serialization
4. Workspace persistence
5. Validation and conflict analysis
6. Export pipeline

The architecture created during this phase should require minimal refactoring to support those features.
