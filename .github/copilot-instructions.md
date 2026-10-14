# Copilot Instructions

## Project Guidelines
- Prefer an editor-first layout: place the header and grid below the editor.
- Use per-form absolute row sizing for the root TableLayoutPanel's top row: set StyleType = Absolute and Absolute = editor.Height so the editor area is fully visible and avoids clipping when increasing editor/toolbar sizes. Prefer per-form editor/toolbar heights to ensure editor area shows buttons fully and controls remain accessible.
- Keep the header row AutoSize and the grid row as Percent = 100.
- Set per-form editor/toolbar heights:
  - Patient: 340
  - Doctor: 300
  - Appointment: 320
  - Billing: 360
  - Room toolbar: 180