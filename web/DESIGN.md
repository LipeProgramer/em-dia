---
version: alpha
colors:
  forest: "#153f38"
  forestDeep: "#10332e"
  primary: "#176d61"
  mint: "#a9e3d2"
  background: "#f3f7f5"
  surface: "#ffffff"
  text: "#183a35"
  muted: "#5f756a"
  border: "#dce6e1"
  overdue: "#b83d32"
  attention: "#985b08"
  onTrack: "#24765e"
typography:
  display:
    fontFamily: "Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif"
    fontSize: "clamp(1.75rem, 3vw, 2.5rem)"
    lineHeight: "1.12"
  body:
    fontFamily: "Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif"
    fontSize: "14px"
    lineHeight: "1.6"
  utility:
    fontFamily: "ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif"
    fontSize: "12px"
    lineHeight: "1.45"
rounded:
  small: "8px"
  control: "10px"
  card: "15px"
  modal: "17px"
spacing:
  pageDesktop: "32px"
  pageMobile: "16px"
  controlGap: "10px"
  sectionGap: "18px"
components:
  primaryButton:
    minHeight: "44px"
    radius: "10px"
  statusBadge:
    requiresIconOrText: true
    colorOnly: false
  radarLevel:
    requiresLabelAndCount: true
    colorOnly: false
omitted:
  - section: elevation
    reason: "Depth is intentionally restrained and defined in prose and CSS shadows rather than as a token scale."
---

# Em Dia — visual system

## Overview

Em Dia is a personal deadline radar for adults with ADHD. The product surface should feel like a calm, dependable instrument panel: clear enough to decide what to do next, warm enough not to shame the user, and distinctive through the four-level urgency radar rather than decorative effects. The main action is to understand priority and take one small next step.

The web interface extends a mobile-first product. Desktop may use two-column item layouts and persistent navigation; on small screens, it should reduce density and keep touch targets comfortable. The public authentication page is a hybrid brand/product surface; authenticated routes prioritize clarity and task completion.

Anti-references: generic analytics dashboards, excessive rounded cards, gradients used as decoration, emoji in place of icons, fake metrics, unexplained demo data, and controls that imply integrations the app does not support.

## Colors

The forest/teal palette communicates steadiness and gives the Radar a recognizable home. Green is not used alone to explain state: status text, headings and position remain visible. Urgency red and amber are reserved for real deadlines, while neutral green is used for status and identity. The CSS runtime implementation lives in `src/styles/tokens.css` and `src/styles/redesign.css`; this document records the intent and canonical values.

## Typography

Use a familiar sans-serif stack that remains available without external font downloads. Headlines use restrained negative tracking and a strong size step; body copy stays conversational and legible. Small utility labels are uppercase only when they act as metadata, never for important instructions. Avoid overly small copy on mobile.

## Layout

The Radar is the signature: a conceptual orbit visual and a clearly labeled, count-bearing summary of the urgency groups, followed by actionable sections and item cards. Desktop content uses a bounded wide canvas, two-column item grids where useful, and clear section rhythm. At mobile widths, the layout stacks, tables become labeled cards, and modal actions remain reachable above the virtual keyboard.

## Elevation & Depth

Static content is mostly flat. Borders define groups; a small number of subtle shadows are used for floating or interactive surfaces. The top navigation may use a translucent white surface, but the main product surfaces are opaque to preserve legibility.

## Shapes

Use 8–17px radii according to component hierarchy. Buttons and fields are clearly rectangular rather than pill-shaped. The brand mark may use a non-symmetric corner as a small distinctive detail. Touch controls must preserve a minimum 44px target where practical.

## Components

Use the local `Icon` component (`src/components/Icon.tsx`) for consistent line icons; do not replace icons with emoji. Status badges always combine readable text and visual color. Shared `Modal` and `ConfirmDialog` own dialog presentation and keyboard handling. Errors are actionable, successes describe the completed action, and destructive account deletion requires an explicit typed confirmation.

## Do's and Don'ts

Do keep labels concrete, reduce unnecessary choices, preserve the four-level Radar metaphor, provide visible keyboard focus, respect `prefers-reduced-motion`, and show loading/error/empty states. Do not invent product statistics, present sample data as live account data, introduce fake social login, animate every control, or treat color as the only status cue.
