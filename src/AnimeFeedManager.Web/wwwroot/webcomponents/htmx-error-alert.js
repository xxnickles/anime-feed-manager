/**
 * DaisyHTMXAlerts Web Component
 *
 * A custom web component that automatically displays DaisyUI-styled alerts
 * when HTMX requests encounter errors.
 *
 * @element daisy-htmx-alerts
 * @attribute {string} attach-to - CSS selector for the container where alerts will be appended (default: 'body')
 * @attribute {number} max-retries - Maximum number of retries to wait for HTMX to load (default: 5)
 * @attribute {number} retry-delay - Delay in milliseconds between retry attempts (default: 1000)
 * @attribute {string} messages-config - JSON string or ID of script tag containing custom messages
 *
 * @example Basic usage:
 * <daisy-htmx-alerts attach-to="#alert-container"></daisy-htmx-alerts>
 *
 * @example With custom messages via script tag:
 * <script id="custom-messages" type="application/json">
 * {
 *   "404": {
 *     "title": "Page Not Found",
 *     "message": "The page you're looking for doesn't exist."
 *   },
 *   "401": {
 *     "title": "Please Sign In",
 *     "message": "You need to be logged in to access this content."
 *   }
 * }
 * </script>
 * <daisy-htmx-alerts attach-to="#alerts" messages-config="#custom-messages"></daisy-htmx-alerts>
 *
 * @example With inline JSON (for simple cases):
 * <daisy-htmx-alerts
 *   attach-to="#alerts"
 *   messages-config='{"404":{"title":"Not Found","message":"Resource is missing"}}'>
 * </daisy-htmx-alerts>
 *
 * @example Programmatic message customization:
 * const alertComponent = document.querySelector('daisy-htmx-alerts');
 * alertComponent.setCustomMessages({
 *   404: { title: 'Custom 404', message: 'Custom not found message' },
 *   500: { title: 'Server Problem', message: 'Please try again later' }
 * });
 */
class DaisyHTMXAlerts extends HTMLElement {
    constructor() {
        super();
        this.alertContainer = null;
        this.alertCounter = 0;
        this.htmxRetryCount = 0;
        this.maxHtmxRetries = 5;
        this.retryDelay = 1000;

        // Default error messages
        this.errorMessages = {
            // HTTP Status Codes
            400: {
                title: 'Bad Request',
                message: 'The request could not be understood by the server'
            },
            401: {
                title: 'Unauthorized',
                message: 'Authentication is required'
            },
            403: {
                title: 'Forbidden',
                message: 'Access to this resource is forbidden'
            },
            404: {
                title: 'Not Found',
                message: 'The requested resource was not found'
            },
            409: {
                title: 'Conflict',
                message: 'The request could not be completed due to a conflict'
            },
            422: {
                title: 'Validation Error',
                message: 'The request contains invalid data'
            },
            429: {
                title: 'Too Many Requests',
                message: 'Please slow down and try again later'
            },
            500: {
                title: 'Server Error',
                message: 'An internal server error occurred'
            },
            502: {
                title: 'Bad Gateway',
                message: 'The server received an invalid response'
            },
            503: {
                title: 'Service Unavailable',
                message: 'The server is temporarily unavailable'
            },
            504: {
                title: 'Gateway Timeout',
                message: 'The server did not respond in time'
            },
            // Special cases
            timeout: {
                title: 'Request Timeout',
                message: 'The request took too long to complete'
            },
            networkError: {
                title: 'Connection Failed',
                message: 'Could not connect to the server'
            },
            default: {
                title: 'Request Error',
                message: 'An error occurred while processing your request'
            }
        };
    }

    connectedCallback() {
        // Get configuration from attributes
        const attachTo = this.getAttribute('attach-to') || 'body';
        this.maxHtmxRetries = parseInt(this.getAttribute('max-retries') || '5', 10);
        this.retryDelay = parseInt(this.getAttribute('retry-delay') || '1000', 10);

        // Validate attribute values
        if (isNaN(this.maxHtmxRetries) || this.maxHtmxRetries < 0) {
            console.warn('DaisyHTMXAlerts: Invalid max-retries value, using default (5)');
            this.maxHtmxRetries = 5;
        }

        if (isNaN(this.retryDelay) || this.retryDelay < 0) {
            console.warn('DaisyHTMXAlerts: Invalid retry-delay value, using default (1000ms)');
            this.retryDelay = 1000;
        }

        // Load custom messages if provided
        this.loadCustomMessages();

        // Get the target element where alerts should be attached
        this.alertContainer = document.querySelector(attachTo);

        if (!this.alertContainer) {
            console.warn(`DaisyHTMXAlerts: Could not find element "${attachTo}", defaulting to body`);
            this.alertContainer = document.body;
        }

        // Setup HTMX error event listeners
        this.setupHTMXListeners();
    }

    setupHTMXListeners() {
        // Check if HTMX is available
        if (typeof htmx === 'undefined') {
            this.htmxRetryCount++;

            if (this.htmxRetryCount <= this.maxHtmxRetries) {
                console.warn(`DaisyHTMXAlerts: HTMX is not loaded! Retry attempt ${this.htmxRetryCount}/${this.maxHtmxRetries}...`);
                setTimeout(() => this.setupHTMXListeners(), this.retryDelay);
                return;
            } else {
                console.error(`DaisyHTMXAlerts: HTMX could not be loaded after ${this.maxHtmxRetries} attempts. Alerts will not work.`);
                return;
            }
        }

        // Listen for HTMX response errors (4xx, 5xx status codes)
        document.body.addEventListener('htmx:responseError', (event) => {
            const status = event.detail.xhr?.status;
            const url = this.getRequestUrl(event.detail);

            // Get custom or default message for this status code
            const errorConfig = this.errorMessages[status] || {
                title: this.errorMessages.default.title,
                message: `${this.errorMessages.default.message} (Status: ${status})`
            };

            this.createAlert('error', errorConfig.title, errorConfig.message, url);
        });

        // Listen for network/connection errors
        document.body.addEventListener('htmx:sendError', (event) => {
            const url = this.getRequestUrl(event.detail);
            const errorConfig = this.errorMessages.networkError;
            this.createAlert('error', errorConfig.title, errorConfig.message, url);
        });

        // Listen for timeout errors
        document.body.addEventListener('htmx:timeout', (event) => {
            const url = this.getRequestUrl(event.detail);
            const errorConfig = this.errorMessages.timeout;
            this.createAlert('warning', errorConfig.title, errorConfig.message, url);
        });
    }

    /**
     * Load custom messages from various sources
     * @private
     *
     * Custom Message Structure:
     * {
     *   "404": {
     *     "title": "Your custom title",
     *     "message": "Your custom message"
     *   },
     *   "500": {
     *     "title": "Server Error",
     *     "message": "Something went wrong on our end"
     *   },
     *   "timeout": {
     *     "title": "Slow Connection",
     *     "message": "The server is taking longer than expected"
     *   },
     *   "networkError": {
     *     "title": "No Connection",
     *     "message": "Please check your internet connection"
     *   }
     * }
     */
    loadCustomMessages() {
        const messagesConfig = this.getAttribute('messages-config');
        if (!messagesConfig) return;

        try {
            // Method 1: Try to parse as JSON directly
            if (messagesConfig.startsWith('{')) {
                const customMessages = JSON.parse(messagesConfig);
                this.errorMessages = { ...this.errorMessages, ...customMessages };
            }
            // Method 2: Try to load from script tag
            else if (messagesConfig.startsWith('#')) {
                const scriptElement = document.querySelector(messagesConfig);
                if (scriptElement && scriptElement.type === 'application/json') {
                    const customMessages = JSON.parse(scriptElement.textContent);
                    this.errorMessages = { ...this.errorMessages, ...customMessages };
                }
            }
        } catch (error) {
            console.error('DaisyHTMXAlerts: Failed to load custom messages', error);
        }
    }

    /**
     * Set custom messages programmatically
     * @param {Object} messages - Object containing status codes as keys and {title, message} as values
     * @public
     *
     * @example
     * const alerts = document.querySelector('daisy-htmx-alerts');
     * alerts.setCustomMessages({
     *   404: { title: 'Oops!', message: 'We couldn\'t find that page' },
     *   401: { title: 'Please Login', message: 'You need to sign in first' }
     * });
     */
    setCustomMessages(messages) {
        this.errorMessages = { ...this.errorMessages, ...messages };
    }

    /**
     * Get current error messages configuration
     * @returns {Object} Current error messages
     * @public
     */
    getErrorMessages() {
        return { ...this.errorMessages };
    }

    /**
     * Extract the request URL from HTMX event detail
     * @private
     */
    getRequestUrl(detail) {
        // Try multiple ways to get the URL
        return detail.pathInfo?.requestPath ||
            detail.elt?.getAttribute('hx-get') ||
            detail.elt?.getAttribute('hx-post') ||
            detail.elt?.getAttribute('hx-put') ||
            detail.elt?.getAttribute('hx-delete') ||
            detail.elt?.getAttribute('hx-patch') ||
            'Unknown URL';
    }

    /**
     * Create and display an alert
     * @param {string} type - Alert type: 'error', 'warning', 'info', 'success'
     * @param {string} title - Alert title
     * @param {string} message - Alert message
     * @param {string} url - The URL that triggered the alert
     */
    createAlert(type, title, message, url) {
        const alertId = `daisy-htmx-alert-${++this.alertCounter}`;

        // Create the alert element
        const alertDiv = document.createElement('div');
        alertDiv.id = alertId;
        alertDiv.setAttribute('role', 'alert');
        alertDiv.className = `alert alert-${type} shadow-lg`;

        // Add fade transition styles
        alertDiv.style.transition = 'opacity 300ms ease-in-out';
        alertDiv.style.opacity = '0';

        // Get the appropriate icon
        const icon = this.getIcon(type);

        // Build the alert HTML following DaisyUI structure
        alertDiv.innerHTML = `
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="stroke-current shrink-0 w-6 h-6">
                ${icon}
            </svg>
            <div>
                <h3 class="font-bold">${this.escapeHtml(title)}</h3>
                <div class="text-xs">${this.escapeHtml(message)}</div>
                <div class="text-xs opacity-50" style="margin-top: 0.25rem;">URL: ${this.escapeHtml(url)}</div>
            </div>
            <button class="btn btn-sm" onclick="document.querySelector('daisy-htmx-alerts').dismissAlert('${alertId}')">
                Dismiss
            </button>
        `;

        // Append to container
        this.alertContainer.appendChild(alertDiv);

        // Trigger fade in after a brief delay to ensure the element is in the DOM
        requestAnimationFrame(() => {
            alertDiv.style.opacity = '1';
        });
    }

    /**
     * Dismiss an alert with fade out animation
     * @param {string} alertId - The ID of the alert to dismiss
     * @public
     */
    dismissAlert(alertId) {
        const alertElement = document.getElementById(alertId);
        if (!alertElement) return;

        // Fade out
        alertElement.style.opacity = '0';

        // Remove from DOM after animation completes
        setTimeout(() => {
            alertElement.remove();
        }, 300); // Match the transition duration
    }

    /**
     * Get SVG icon path based on alert type
     * @private
     */
    getIcon(type) {
        const icons = {
            error: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />',
            warning: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />',
            info: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>',
            success: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />'
        };
        return icons[type] || icons.info;
    }

    /**
     * Escape HTML to prevent XSS attacks
     * @private
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Public method to manually create an alert
     * Can be called from outside the component
     * @public
     */
    showAlert(type, title, message, url = 'Manual trigger') {
        this.createAlert(type, title, message, url);
    }

    /**
     * Remove all alerts from the container
     * @public
     */
    clearAllAlerts() {
        const alerts = this.alertContainer.querySelectorAll('[id^="daisy-htmx-alert-"]');
        alerts.forEach(alert => alert.remove());
    }
}

// Register the custom element
customElements.define('daisy-htmx-alerts', DaisyHTMXAlerts);