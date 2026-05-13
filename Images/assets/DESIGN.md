---
name: Academic Clarity
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#40484d'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#70787d'
  outline-variant: '#c0c8cd'
  surface-tint: '#236580'
  primary: '#00475e'
  on-primary: '#ffffff'
  primary-container: '#1a5f7a'
  on-primary-container: '#9bd7f7'
  inverse-primary: '#92cfee'
  secondary: '#006591'
  on-secondary: '#ffffff'
  secondary-container: '#7cc9fe'
  on-secondary-container: '#00547a'
  tertiary: '#004a41'
  on-tertiary: '#ffffff'
  tertiary-container: '#006459'
  on-tertiary-container: '#64e4d0'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#c0e8ff'
  primary-fixed-dim: '#92cfee'
  on-primary-fixed: '#001e2b'
  on-primary-fixed-variant: '#004d66'
  secondary-fixed: '#c9e6ff'
  secondary-fixed-dim: '#89ceff'
  on-secondary-fixed: '#001e2f'
  on-secondary-fixed-variant: '#004c6e'
  tertiary-fixed: '#79f7e3'
  tertiary-fixed-dim: '#59dbc7'
  on-tertiary-fixed: '#00201c'
  on-tertiary-fixed-variant: '#005047'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  display-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 60px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
  label-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 8px
  container-max: 1280px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 32px
---

## Brand & Style

The brand personality is **Academic, Trustworthy, and Empowering**. Designed for students and educators, the UI prioritizes cognitive ease and structural organization. It avoids visual clutter to foster a focused learning environment.

The design style follows a **Modern Corporate** aesthetic with high-density information management. It utilizes a "Surface-over-Surface" approach, where content lives on clean white cards elevated slightly above soft-grey backgrounds. Visual metaphors are grounded in traditional educational values but executed with contemporary digital precision—clean lines, purposeful whitespace, and a clear hierarchy that respects Right-to-Left (RTL) reading patterns.

## Colors

The palette is anchored by **Deep Blue (#1A5F7A)**, evoking stability and institutional authority. This is used for primary navigation, headings, and high-emphasis components. 

**Secondary Light Blue (#57A6D9)** is the "Action" color, reserved for interactive elements like buttons, active states, and links. **Tertiary Teal (#00A896)** serves as a functional accent for "Success" states and availability indicators, as seen in the teacher-status badges.

The background uses a cool **Neutral Grey (#F8FAFC)** to reduce eye strain, while pure **White (#FFFFFF)** is used for content containers to provide maximum contrast for text.

## Typography

This design system uses **Plus Jakarta Sans** for its friendly yet professional geometry. For the Arabic implementation, the font should be paired with a modern, high-legibility Noto Sans Arabic fallback to ensure seamless RTL reading.

- **Headlines:** Use Bold weights for primary page titles to create a strong anchor.
- **Body:** Standard body text utilizes a 16px base to ensure readability for long-form educational content.
- **Labels:** Use Medium weights for navigation items and metadata to distinguish them from body copy.
- **RTL Considerations:** Line heights are increased by 15% for Arabic text compared to standard Latin defaults to accommodate the height of Arabic characters.

## Layout & Spacing

The system employs a **Fixed Grid** model for the web portal to maintain optimal line lengths for reading. 

- **Desktop:** A 12-column grid with a 1280px maximum container width. Gutters are fixed at 24px.
- **Tablet:** 8-column grid with fluid margins.
- **Mobile:** 4-column grid with 16px side margins.

The spacing rhythm is based on an **8px base unit**. All padding and margins between elements should be multiples of 8 (e.g., 16px for internal card padding, 32px for section spacing). In the RTL layout, all horizontal offsets and placements are mirrored—sidebars appear on the right, and "Back" actions point to the right.

## Elevation & Depth

Depth is used sparingly to define hierarchy and interactivity. The system relies on **Ambient Shadows** that are extremely soft and diffused to prevent the UI from feeling heavy.

- **Level 0 (Background):** Neutral Grey (#F8FAFC).
- **Level 1 (Cards/Surface):** Pure White with a subtle 1px border (#E2E8F0) and no shadow. Used for secondary content.
- **Level 2 (Interactive Cards):** Pure White with a soft shadow (0px 4px 20px rgba(0, 0, 0, 0.05)). Used for teacher profiles and session cards.
- **Level 3 (Modals/Overlays):** Elevated with a more pronounced shadow (0px 12px 32px rgba(26, 95, 122, 0.12)) to focus user attention.

## Shapes

The shape language is **Rounded**, reflecting a welcoming and approachable educational environment.

- **Standard Elements:** Buttons, inputs, and small chips use a 0.5rem (8px) radius.
- **Containers:** Content cards and dashboard sections use a 1rem (16px) radius to create a distinct frame for information.
- **Avatars:** Teacher profiles use circular containers (50% radius) with a 2px offset border to signify status.

## Components

### Buttons
- **Primary:** Deep Blue background with White text. Used for "Start Session" or "Submit."
- **Secondary:** Light Blue ghost buttons (border and text) for secondary actions like "Add to Favorites."
- **Iconography:** Icons are always placed to the left of the text in RTL layouts for "Forward" actions and to the right for "Back" actions.

### Cards
Cards are the primary container for data. They feature a white background, 1rem corner radius, and a subtle border. For teacher cards, the layout should be: Avatar (Right), Name & Rating (Center), and Action Button (Left/Bottom).

### Input Fields
Inputs use a soft grey background (#F1F5F9) with no border in their default state, transitioning to a Deep Blue border on focus. Placeholders and labels must be right-aligned.

### Status Indicators
Small circular badges (8px) located at the bottom-right of avatars indicate "Online" (Tertiary Teal) or "Offline" (Grey).

### Navigation
A persistent top bar for branding and user profile, with a right-aligned vertical sidebar for main portal categories (Dashboard, Lessons, Teachers, Tasks).