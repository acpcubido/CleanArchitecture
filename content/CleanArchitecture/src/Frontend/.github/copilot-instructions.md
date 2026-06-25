## Tech Stack

| Layer | Technology |
|---|---|
| Framework | Angular 21, standalone components only (no NgModules) |
| State | NgRx Signals (`signalStore`, `signalStoreFeature`) |
| Styling | Tailwind CSS + component-scoped SCSS |
| REST API | auto-generated — never edit |
| State mutations | Immer via `produceState()` helper |
| i18n | ngx-translate, files in i18n |

---

## Feature Layout

```
featureName/
  pages/              # Routed top-level components (one per route)
  components/         # Non-routed components
  dialogs/            # Kendo dialog components
  *.store.ts          # Feature-level signal store

_shared/
  components/         # Shared components across features
  dialogs/            # Shared dialog components
  guards/             # Feature-specific route guards
  helpers/            # Shared utility functions
  pipes/              # Feature-scoped pipes
  services/           # Shared services (e.g., SignalR, API)
  stores/             # signal store features (no global store)
```

---

## State Management Rules

- All state via `signalStore()` — never RxJS subjects for state
- Compose with `signalStoreFeature` — co-locate `.store-feature.ts` next to the component

---

## Key Conventions

- **Path alias**: `@/*` → `src/app/*` — use for cross-feature imports
- All routes are **lazy-loaded** (`loadComponent`)
