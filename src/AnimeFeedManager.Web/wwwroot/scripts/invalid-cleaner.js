(() => {
    /**
     * Alpine.js plugin: x-invalid-cleaner
     *
     * Enhanced plugin that:
     * 1. Removes the 'invalid' class from inputs when user starts editing them
     * 2. Gracefully hides ValidationErrorInfo components using Tailwind transitions
     * Apply to any container element (form, fieldset, div) or directly to an input.
     */
    function plugin(Alpine) {
        Alpine.directive('invalid-cleaner', (el, { modifiers }, { cleanup }) => {

            function handleInput(event) {
                const target = event.target;

                // Only handle input, select, and textarea elements
                if (['INPUT', 'SELECT', 'TEXTAREA'].includes(target.tagName)) {
                    // Remove 'invalid' class when user starts editing
                    if (target.classList.contains('invalid')) {
                        target.classList.remove('invalid');
                    }

                    // Find and gracefully hide validation error icons
                    hideValidationErrorIcons(target);
                }
            }

            function hideValidationErrorIcons(inputElement) {
                // Look for validation error icons in the same label or nearby
                const label = inputElement.closest('label');
                if (!label) return;

                // Find ValidationErrorInfo components
                const errorIcons = label.querySelectorAll('.validation-error-icon');

                errorIcons.forEach(icon => {
                    // Skip if already being hidden
                    if (icon.classList.contains('opacity-0')) return;

                    // Add Tailwind classes for hiding transition
                    icon.classList.add('opacity-0', 'scale-75', 'pointer-events-none');

                    // Remove from DOM after transition completes
                    // Using transitionend event for better timing
                    const handleTransitionEnd = (e) => {
                        if (e.target === icon && e.propertyName === 'opacity') {
                            icon.remove();
                            icon.removeEventListener('transitionend', handleTransitionEnd);
                        }
                    };

                    icon.addEventListener('transitionend', handleTransitionEnd);

                    // Fallback timeout in case transitionend doesn't fire
                    setTimeout(() => {
                        if (icon.parentNode && icon.classList.contains('opacity-0')) {
                            icon.remove();
                            icon.removeEventListener('transitionend', handleTransitionEnd);
                        }
                    }, 350); // Slightly longer than CSS transition
                });
            }

            // Attach event listener
            el.addEventListener('input', handleInput, true);

            // Cleanup
            cleanup(() => {
                el.removeEventListener('input', handleInput, true);
            });
        });
    }

    // Register the plugin
    document.addEventListener("alpine:init", () => {
        window.Alpine.plugin(plugin);
    });

    // Auto-register if Alpine is already initialized
    if (window.Alpine) {
        window.Alpine.plugin(plugin);
    }
})();