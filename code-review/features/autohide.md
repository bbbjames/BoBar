# Auto-Hide Feature Specification

## UI/UX Considerations
To ensure a smooth user experience, the auto-hide feature must address the following:
1.  **Edge Awareness**: The bar should collapse towards the nearest screen edge (Top or Bottom) so it doesn't float in the middle of the screen.
2.  **Accidental Hiding**: The bar must not hide while the user is interacting with it (mouse hover).
3.  **Visual Feedback**: A small portion ("lip") should remain visible to indicate the bar is present and can be expanded.
4.  **Smoothness**: Transitions should be instant or animated (instant is simpler for V1).

## Implementation Plan

### 1. State Management
The form tracks its state using several fields:
*   **Timers**: Three timers manage the feature: `_autoHideTimer` (triggers collapse), `_animationTimer` (smooth resizing), and `_fadeTimer` (action button visibility).
*   **State Flags**: `_isAutoHidden` tracks if the bar is collapsed, and `_autoHideEnabled` tracks the user preference.
*   **Dimensions**: `_expandedSize` and `_expandedLocation` store the bar's full size and position to allow accurate restoration.
*   **Action Button**: A reference to the `RoundedButton` used for toggling the bar.
*   **Fade State**: Fields to track current opacity and base colors for the action button fade effect.

### 2. Initialization
*   All timers are initialized in the constructor.
*   The `AutoHide` setting is loaded from `ConfigurationManager` on startup.
*   The initial state (collapsed or expanded) is applied based on the saved preference.

### 3. Timer Logic (Hide Action)
*   The `_autoHideTimer` ticks every 2 seconds.
*   It checks if the mouse is over the form or if the bar is already hidden/animating.
*   If safe to hide, it triggers `CollapseWindow`.
*   `CollapseWindow` saves the current size/location, disables `AutoSize`, switches the action button symbol to "Expand" (ChevronRight), and starts the animation timer.

### 4. Restore Logic (Show Action)
*   `OnMouseEnter` and `OnMouseMove` events trigger `ExpandWindow`.
*   `ExpandWindow` starts the animation timer to restore the bar to its full size.

### 5. Animation Logic
*   The `_animationTimer` handles smooth resizing between the collapsed state (showing only the action button) and the expanded state.
*   It uses a simple linear interpolation (lerp) to adjust the form's width and height.
*   When expansion completes:
    *   `AutoSize` is re-enabled.
    *   The `_autoHideTimer` is restarted.
    *   The action button symbol switches to "Collapse" (ChevronLeft).

### 6. Action Button & Fade Logic
*   **Toggle**: The user can enable/disable auto-hide via the context menu.
*   **Fade In**: When auto-hide is enabled, the action button fades in to full opacity.
*   **Fade Out**: When auto-hide is disabled, the action button fades out and is hidden from the layout.
*   **Color Handling**: The fade logic interpolates the alpha channel of the button's background, foreground, and border colors.

### 7. Persistence
*   The `AutoHide` boolean setting is added to `AppConfiguration`.
*   `ConfigurationManager` loads and saves this setting to `settings.ini`.
*   `Form1` ensures the in-memory configuration is updated whenever the setting is toggled.

### 8. Cleanup
*   All timers are stopped in the `FormClosing` event to prevent resource leaks.
