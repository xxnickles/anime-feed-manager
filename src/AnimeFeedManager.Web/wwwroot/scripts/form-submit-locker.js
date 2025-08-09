(() => {
    // Utility function: debounce (delays execution until after wait ms of inactivity)
    function debounce(func, wait) {
        let timeout;
        return function(...args) {
            const context = this;
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(context, args), wait);
        };
    }

    // Utility function: throttle (ensures function is called at most once per wait ms)
    function throttle(func, wait) {
        let timeout;
        let lastTime = 0;
        return function(...args) {
            const context = this;
            const now = Date.now();
            const remaining = wait - (now - lastTime);

            if (remaining <= 0) {
                clearTimeout(timeout);
                lastTime = now;
                func.apply(context, args);
            } else if (!timeout) {
                timeout = setTimeout(() => {
                    lastTime = Date.now();
                    func.apply(context, args);
                }, remaining);
            }
        };
    }

    const pluginName = "form-submit-locker";

    /**
     * Alpine.js plugin: x-form-submit-locker
     *
     * Core functionality:
     * - Disables the submit button when the form is invalid based on HTML5 constraints
     * - Prevents browser-native validation popups by setting novalidate
     * - Updates button state on input, select, form reset, and browser navigation
     * - Warns and opts-out if multiple submit buttons are detected
     * - Configurable debounce delay via modifiers
     *
     * Relies on browser's built-in :valid/:invalid pseudo-classes for styling
     */
    function plugin_default(Alpine) {
        const FORM_TAG = "FORM";
        const DEFAULT_DEBOUNCE_DELAY = 300;

        Alpine.directive(pluginName, (el, { modifiers, expression }, { cleanup, evaluate }) => {
            // Ensure the directive is only applied to <form> elements
            if (el.tagName !== FORM_TAG) {
                console.error(`[x-${pluginName}] Directive must be used on <form> elements only.`);
                return;
            }

            // Parse configuration from modifiers or expression
            const config = {
                debounceDelay: parseInt(modifiers[0]) || DEFAULT_DEBOUNCE_DELAY,
                validateOnMount: !modifiers.includes('no-initial-validation'),
                disableOnSubmit: modifiers.includes('disable-on-submit'),
                allowMultipleSubmits: modifiers.includes('allow-multiple-submits'),
                focusOnError: !modifiers.includes('no-focus-on-error'),
                changeTextOnSubmit: modifiers.includes('change-text'),
                customValidator: expression ? () => evaluate(expression) : null
            };

            // Find all submit buttons within the form
            const submitButtons = Array.from(el.querySelectorAll('button[type="submit"], input[type="submit"], button:not([type])')).filter(btn => {
                // Filter out buttons that are explicitly not submit buttons
                const type = btn.getAttribute('type');
                return type === 'submit' || (btn.tagName === 'BUTTON' && !type);
            });

            // Check for multiple submit buttons and warn/opt-out
            if (submitButtons.length > 1 && !config.allowMultipleSubmits) {
                console.warn(
                    `[x-${pluginName}] Multiple submit buttons detected in form. This often indicates a UX issue that could be better solved with:\n` +
                    `- Separate forms for different actions\n` +
                    `- A single submit with radio/select for action type\n` +
                    `- JavaScript-driven workflow buttons\n\n` +
                    `The form submit locker has been disabled for this form.\n` +
                    `If you really need multiple submit buttons, add the 'allow-multiple-submits' modifier:\n` +
                    `<form x-${pluginName}.allow-multiple-submits>\n\n` +
                    `Form element:`, el
                );
                return; // Opt-out of applying the plugin
            }

            if (submitButtons.length === 0) {
                console.warn(`[x-${pluginName}] No submit buttons found in form.`);
                return;
            }

            // Get the single submit button
            const submitButton = submitButtons[0];

            // Store original disabled state to restore it properly
            const originalDisabledState = submitButton.disabled;

            // Disable browser-based validation UI (popups) for this form
            el.setAttribute("novalidate", true);

            /**
             * Evaluates the form's validity
             * @returns {boolean} True if form is valid
             */
            function isFormValid() {
                // Check HTML5 constraint validation
                let isValid = el.checkValidity();

                // Apply custom validator if provided
                if (config.customValidator) {
                    try {
                        const customResult = config.customValidator();
                        isValid = isValid && customResult;
                    } catch (e) {
                        console.error(`[x-${pluginName}] Custom validator error:`, e);
                    }
                }

                return isValid;
            }

            /**
             * Evaluates the form's validity and toggles the disabled state of submit button
             */
            function evaluateSubmit() {
                const isValid = isFormValid();

                Alpine.nextTick(() => {
                    // Only modify disabled state if it wasn't originally disabled
                    if (!originalDisabledState) {
                        submitButton.disabled = !isValid;

                        // Set aria-disabled for better accessibility
                        if (isValid) {
                            submitButton.removeAttribute('aria-disabled');
                        } else {
                            submitButton.setAttribute('aria-disabled', 'true');
                        }
                    }
                });
            }

            // Debounced handler for text input events
            const inputHandler = debounce((event) => {
                if (['INPUT', 'TEXTAREA'].includes(event.target.tagName)) {
                    evaluateSubmit();
                }
            }, config.debounceDelay);

            // Immediate handler for select changes
            const changeHandler = (event) => {
                if (event.target.tagName === 'SELECT') {
                    evaluateSubmit();
                }
            };

            // Handler for form reset
            const resetHandler = (event) => {
                if (event.target === el) {
                    // Re-evaluate after a tick to allow form to reset
                    setTimeout(() => evaluateSubmit(), 0);
                }
            };

            // Track submission state
            let isSubmitting = false;

            /**
             * Resets the submit button to its normal state after submission
             */
            function resetSubmitButton() {
                if (!isSubmitting) return;

                isSubmitting = false;
                submitButton.removeAttribute('aria-busy');

                // Restore original text if it was changed
                if (config.changeTextOnSubmit && submitButton.dataset.originalText) {
                    submitButton.textContent = submitButton.dataset.originalText;
                    delete submitButton.dataset.originalText;
                }

                // Re-evaluate form validity for button states
                evaluateSubmit();
            }

            // Handler for form submission
            const submitHandler = (event) => {
                const isValid = isFormValid();

                if (!isValid) {
                    event.preventDefault();
                    event.stopPropagation();

                    // Focus first invalid field if configured
                    if (config.focusOnError) {
                        // Use the browser's built-in :invalid selector
                        const firstInvalid = el.querySelector(':invalid:not([type="hidden"]):not([disabled])');
                        if (firstInvalid) {
                            firstInvalid.focus();
                            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        }
                    }

                    // Announce to screen readers
                    const announcement = document.createElement('div');
                    announcement.setAttribute('role', 'alert');
                    announcement.setAttribute('aria-live', 'polite');
                    announcement.style.position = 'absolute';
                    announcement.style.left = '-10000px';
                    announcement.style.width = '1px';
                    announcement.style.height = '1px';
                    announcement.style.overflow = 'hidden';
                    announcement.textContent = 'Form validation failed. Please check the required fields.';
                    el.appendChild(announcement);
                    setTimeout(() => announcement.remove(), 1000);
                } else if (config.disableOnSubmit) {
                    // For non-HTMX forms, disable immediately
                    // For HTMX forms, we'll handle this in htmx:beforeRequest
                    const hasHtmxAttributes = el.hasAttribute('hx-post') ||
                        el.hasAttribute('hx-put') ||
                        el.hasAttribute('hx-patch') ||
                        el.hasAttribute('hx-delete') ||
                        el.hasAttribute('hx-get');

                    if (!hasHtmxAttributes) {
                        isSubmitting = true;

                        // Disable submit button during submission
                        submitButton.disabled = true;
                        submitButton.setAttribute('aria-busy', 'true');

                        // Only change text if explicitly configured
                        if (config.changeTextOnSubmit && submitButton.tagName === 'BUTTON' && !submitButton.dataset.originalText) {
                            submitButton.dataset.originalText = submitButton.textContent;
                            submitButton.textContent = submitButton.dataset.loadingText || 'Submitting...';
                        }
                    }
                    // For HTMX forms, don't disable here - wait for confirmation
                }
            };

            // HTMX-specific handlers
            const htmxConfirmHandler = (event) => {
                // Check if this is our form or a child element
                if (event.detail.elt === el || el.contains(event.detail.elt)) {
                    // Just track that a confirmation was triggered
                    // We'll monitor if a request follows or not

                    // Add a listener for the issueRequest callback if it exists
                    const originalIssueRequest = event.detail.issueRequest;
                    if (typeof originalIssueRequest === 'function') {
                        event.detail.issueRequest = function(proceed) {
                            // Call the original
                            originalIssueRequest(proceed);

                            // If user cancelled, reset button state
                            if (!proceed && submitButton.disabled && !originalDisabledState) {
                                resetSubmitButton();
                            }
                        };
                    }
                }
            };

            const htmxBeforeRequestHandler = (event) => {
                if (event.detail.elt === el || el.contains(event.detail.elt)) {
                    // Only disable if not already submitting (avoids double-disable)
                    if (config.disableOnSubmit && !isSubmitting) {
                        isSubmitting = true;
                        submitButton.disabled = true;
                        submitButton.setAttribute('aria-busy', 'true');

                        // Only change text if explicitly configured
                        if (config.changeTextOnSubmit && submitButton.tagName === 'BUTTON' && !submitButton.dataset.originalText) {
                            submitButton.dataset.originalText = submitButton.textContent;
                            submitButton.textContent = submitButton.dataset.loadingText || 'Submitting...';
                        }
                    }
                }
            };

            const htmxAfterRequestHandler = (event) => {
                if (event.detail.elt === el || el.contains(event.detail.elt)) {
                    resetSubmitButton();
                }
            };

            // Handler for when form submission is cancelled (including by confirmation)
            const htmxHaltedHandler = (event) => {
                if (event.detail.elt === el || el.contains(event.detail.elt)) {
                    // Request was halted, reset button if it was disabled
                    if (isSubmitting) {
                        resetSubmitButton();
                    }
                }
            };

            // Listen for HTMX events if HTMX is present
            if (window.htmx || document.querySelector('[hx-boost], [hx-get], [hx-post], [hx-put], [hx-patch], [hx-delete]')) {
                // IMPORTANT: htmx:confirm must be registered BEFORE htmx:beforeRequest
                document.body.addEventListener('htmx:confirm', htmxConfirmHandler);
                document.body.addEventListener('htmx:beforeRequest', htmxBeforeRequestHandler);
                document.body.addEventListener('htmx:afterRequest', htmxAfterRequestHandler);
                // Also listen for response errors
                document.body.addEventListener('htmx:responseError', htmxAfterRequestHandler);
                document.body.addEventListener('htmx:sendError', htmxAfterRequestHandler);
                // Listen for halted events (includes cancelled confirmations)
                document.body.addEventListener('htmx:halted', htmxHaltedHandler);
            }

            // Handler for browser navigation (back/forward)
            const popStateHandler = throttle(() => {
                // Re-evaluate form state after navigation
                setTimeout(() => evaluateSubmit(), 100);
            }, 500);

            // Attach event listeners
            el.addEventListener('input', inputHandler, true);
            el.addEventListener('change', changeHandler, true);
            el.addEventListener('reset', resetHandler, true);
            el.addEventListener('submit', submitHandler, true);
            window.addEventListener('popstate', popStateHandler);

            // Initial evaluation
            if (config.validateOnMount) {
                // Delay initial validation to ensure Alpine is fully initialized
                Alpine.nextTick(() => evaluateSubmit());
            }

            // Alpine event handlers for AJAX submissions
            const alpineSubmitSuccessHandler = () => {
                resetSubmitButton();
            };

            const alpineSubmitErrorHandler = () => {
                resetSubmitButton();
            };

            // Listen for Alpine/custom events that might indicate AJAX submission
            el.addEventListener('ajax:success', alpineSubmitSuccessHandler);
            el.addEventListener('ajax:error', alpineSubmitErrorHandler);
            el.addEventListener('submit:success', alpineSubmitSuccessHandler);
            el.addEventListener('submit:error', alpineSubmitErrorHandler);

            // Expose API for programmatic control
            el._formSubmitLocker = {
                validate: () => isFormValid(),
                evaluateSubmit: () => evaluateSubmit(),
                reset: () => {
                    el.reset();
                    evaluateSubmit();
                },
                // Manual unlock for custom AJAX implementations
                unlock: () => resetSubmitButton()
            };

            // Cleanup function
            cleanup(() => {
                el.removeEventListener('input', inputHandler, true);
                el.removeEventListener('change', changeHandler, true);
                el.removeEventListener('reset', resetHandler, true);
                el.removeEventListener('submit', submitHandler, true);
                window.removeEventListener('popstate', popStateHandler);

                // Clean up HTMX listeners if they were added
                if (window.htmx || document.querySelector('[hx-boost], [hx-get], [hx-post], [hx-put], [hx-patch], [hx-delete]')) {
                    document.body.removeEventListener('htmx:confirm', htmxConfirmHandler);
                    document.body.removeEventListener('htmx:beforeRequest', htmxBeforeRequestHandler);
                    document.body.removeEventListener('htmx:afterRequest', htmxAfterRequestHandler);
                    document.body.removeEventListener('htmx:responseError', htmxAfterRequestHandler);
                    document.body.removeEventListener('htmx:sendError', htmxAfterRequestHandler);
                    document.body.removeEventListener('htmx:halted', htmxHaltedHandler);
                }

                // Clean up Alpine/AJAX event listeners
                el.removeEventListener('ajax:success', alpineSubmitSuccessHandler);
                el.removeEventListener('ajax:error', alpineSubmitErrorHandler);
                el.removeEventListener('submit:success', alpineSubmitSuccessHandler);
                el.removeEventListener('submit:error', alpineSubmitErrorHandler);

                // Restore original disabled state
                if (originalDisabledState !== undefined) {
                    submitButton.disabled = originalDisabledState;
                }

                // Restore original button text if it was changed
                if (config.changeTextOnSubmit && submitButton.dataset.originalText) {
                    submitButton.textContent = submitButton.dataset.originalText;
                    delete submitButton.dataset.originalText;
                }

                // Clean up API
                delete el._formSubmitLocker;
            });
        });
    }

    // Register the plugin when Alpine initializes
    document.addEventListener("alpine:init", () => {
        window.Alpine.plugin(plugin_default);
    });

    // Auto-register if Alpine is already initialized
    if (window.Alpine) {
        window.Alpine.plugin(plugin_default);
    }
})();