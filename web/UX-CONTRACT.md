# Em Dia — UX contract

## Scope and authority

This document describes observable frontend behavior for the React application. Product scope and domain behavior are based on `Em_Dia_Entrega1_Proposta_Inicial.pdf` and `Em_Dia_Entrega3_Relatorio_Final.pdf`; API services and types under `src/api/` are the implementation contract for current frontend requests. If these differ, report the conflict rather than silently inventing server behavior.

## Primary navigation

Authenticated users can reach Radar (`/`), Meus itens (`/itens`), Veículos (`/veiculos`) and Preferências (`/perfil`). The Em Dia mark returns to Radar. The skip link goes to the main content. Navigation uses visible labels and icons; the active route is programmatically and visually distinguishable.

## Workflow ledger

| Operation | Trigger | Pending state | Success | Failure/recovery |
|---|---|---|---|---|
| Load Radar | Enter `/` | Loading message with status semantics | Show API-provided urgency groups and counts | Inline error and retry action |
| Toggle Modo Foco | Toggle in Radar | Disable while saving | Invalidate/refetch Radar query | Keep the screen usable and show an inline error |
| Create item | New-item navigation and form submission | Existing form pending state | Return/show item through established route behavior | Field/form error remains visible for correction |
| Complete item | Card action | Modal and mutation pending state | Refresh the Radar and history | Preserve the item and show recoverable error |
| Toggle checklist step | Checkbox on item | Existing mutation state | Invalidate Radar query | Do not claim success without API response |
| Delete item/vehicle | Delete control then confirmation | Confirmation action disabled while pending | Close dialog and invalidate affected queries | Keep context and display an error |
| Update odometer | Vehicle card/form | Existing mutation state | Recalculate/refetch linked item status | Keep current value and display recoverable error |
| Save profile/preferences | Submit profile form | Disable duplicate submits | Inline success feedback | Inline error near the form |
| Delete account | Danger-zone action | Typed confirmation `EXCLUIR`; pending request | Logout and route to login | Dialog remains open with an error |
| Authentication | Login/register form | Submit button shows processing and is disabled | Login routes according to check-up requirement; registration goes to check-up | Visible form error and retry path |

## Dialog behavior

Shared modal supports Escape, Tab focus containment, initial focus, body-scroll locking, and restoration of prior focus. Backdrop click closes non-pending dialogs. Destructive account deletion cannot be triggered until the user types `EXCLUIR`; errors do not silently dismiss the dialog. Do not use browser-native `alert`, `confirm`, or `prompt` in product flows.

## Content and sample data

Never display hardcoded sample deadlines as if they belong to the authenticated account. Public-page previews must be explicitly identified as illustrative. Do not show statistics unless the source is documented and attributable. Do not advertise social authentication unless it is backed by an implemented provider flow.

## Accessibility and responsive behavior

- Text labels accompany color-coded statuses.
- Keyboard focus is visible; icon-only buttons have accessible labels.
- Main controls target at least 44×44px where practical.
- Reduced-motion preferences suppress nonessential transitions.
- At narrow mobile widths, navigation and data layouts adapt instead of requiring horizontal page scrolling.
- Errors use alert semantics; loading and active-state messages use status semantics where appropriate.

## Verification expectations

Required local checks: `npm run lint`, `npx tsc -b`, `npm run test:run`, and `npm run build`. Verify Radar, focus toggle, item create/edit/delete/complete, vehicle odometer, profile preferences, account deletion guard, authentication errors, keyboard-only operation, reduced motion, and narrow viewport. Report skipped checks and environment errors without presenting them as passing evidence.
