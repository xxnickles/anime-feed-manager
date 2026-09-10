/*
SignalR Extension
============================
This extension adds support for SignalR to htmx 4.
Ported from the htmx 1/2-era extension (based on the WebSockets/SSE extensions by bigskysoftware)
to htmx 4's registerExtension API: hook methods are named after the htmx event
(colons -> underscores) instead of a generic onEvent(name, evt) dispatcher, and
the internal API surface passed to init() is a small, fixed set of primitives
(attributeValue, htmxProp, insertContent, onTrigger, collectFormData,
getAttributeObject, triggerHtmxEvent, ...) rather than the old getInternalData/
getTarget/getSwapSpecification/swap/addTriggerHandler/getInputValues/getHeaders/
filterValues/withExtensions surface, none of which exist in htmx 4.
*/

(function () {

    /** @type {object} htmx 4's internal API object, passed to init() */
    var api;

    var signalRConnect = "signalr-connect";
    var signalRSubscribe = "signalr-subscribe";
    var signalRSend = "signalr-send";

    htmx.registerExtension("signalr", {

        /**
         * init is called once, when this extension is first registered.
         * @param {object} apiRef
         */
        init: function (apiRef) {

            // Store reference to internal API
            api = apiRef;

            // Default function for creating new HubConnection objects
            if (htmx.createHubConnection == undefined) {
                htmx.createHubConnection = createHubConnection;
            }
        },

        /**
         * Fires once per processed root node (not recursively per descendant),
         * so this element and all matching descendants must be queried here.
         * @param {HTMLElement} elt
         */
        htmx_before_process: function (elt) {
            forEach(queryAttributeOnThisOrChildren(elt, signalRConnect), ensureHubConnection);
            forEach(queryAttributeOnThisOrChildren(elt, signalRSubscribe), ensureSubscription);
            forEach(queryAttributeOnThisOrChildren(elt, signalRSend), ensureSending);
        },

        /**
         * Fires once per htmx-powered element being cleaned up (elt itself, or a
         * descendant previously marked powered via htmxProp). Tears down the hub
         * connection owned by this element, if any.
         * @param {HTMLElement} elt
         */
        htmx_before_cleanup: function (elt) {
            var hubConnection = elt._htmx?.HubConnection;
            if (hubConnection != undefined) {
                hubConnection.stop();
            }
        }
    });

    /**
     * ensureHubConnection creates a new SignalR Hub Connection on the designated element, using
     * the element's "signalr-connect" attribute.
     * @param {HTMLElement} hubElt
     * @returns
     */
    function ensureHubConnection(hubElt) {

        // If the element containing the connection no longer exists, then
        // do not connect/reconnect the Hub.
        if (!document.body.contains(hubElt)) {
            return;
        }
        if (typeof signalR === 'undefined') {
            console.error('SignalR object not found. Make sure to include SignalR script in the page scripts before this extension.');
            return;
        }

        // signalr-connect lives on <body>, which boost's outerSync keeps and re-processes, so
        // htmx_before_process reaches this element again on every navigation. Keep the live
        // connection: recreating it orphans the previous socket and replays the whole
        // starting -> start event sequence at every page change.
        var existingConnection = hubElt._htmx?.HubConnection;
        if (existingConnection != undefined &&
            existingConnection.state !== signalR.HubConnectionState.Disconnected) {
            return;
        }

        // Get the source straight from the element's value
        var signalrHubUrl = api.attributeValue(hubElt, signalRConnect);

        // Create a new HubConnection and event handlers
        var hubConnection = htmx.createHubConnection(signalrHubUrl);
        api.triggerHtmxEvent(hubElt, 'htmx:signalr:starting');

        hubConnection.onreconnecting(function (error) {
            api.triggerHtmxEvent(hubElt, 'htmx:signalr:reconnecting', { error: error });
        });
        hubConnection.onreconnected(function (connectionId) {
            api.triggerHtmxEvent(hubElt, 'htmx:signalr:reconnected', { connectionId: connectionId });
        });
        hubConnection.onclose(function (error) {
            api.triggerHtmxEvent(hubElt, 'htmx:signalr:close', { error: error });
        });

        hubConnection.start().then(function () {
            api.triggerHtmxEvent(hubElt, 'htmx:signalr:start', { connectionId: hubConnection.connectionId });
        }).catch(function (ex) {
            api.triggerHtmxEvent(hubElt, 'htmx:signalr:start-error', { error: ex, errorType: ex.errorType });
        });

        // Put the HubConnection into the element's htmx-internal data.
        api.htmxProp(hubElt).HubConnection = hubConnection;
    }

    /**
     * ensureSending attaches event listeners to elements with "signalr-send" attribute.
     * @param {HTMLElement} elt
     * @returns
     */
    function ensureSending(elt) {

        // If the element containing the connection no longer exists, then
        // do not connect/reconnect the Hub.
        if (!document.body.contains(elt)) {
            return;
        }
        if (typeof signalR === 'undefined') {
            console.error('SignalR object not found. Make sure to include SignalR script in the page scripts before this extension.');
            return;
        }

        var hubElement = findParentWithHubConnection(elt);

        if (!hubElement) {
            return;
        }

        processHubConnectionSend(hubElement, elt);
    }

    /**
     * ensureSubscription creates a listener that swaps the target element's content
     * whenever the given hub method is invoked, per the element's "signalr-subscribe" attribute.
     * @param {HTMLElement} elt
     * @returns
     */
    function ensureSubscription(elt) {

        // If the element containing the connection no longer exists, then
        // do not subscribe
        if (!document.body.contains(elt)) {
            return;
        }
        if (typeof signalR === 'undefined') {
            console.error('SignalR object not found. Make sure to include SignalR script in the page scripts before this extension.');
            return;
        }

        var hubElement = findParentWithHubConnection(elt);

        if (!hubElement) {
            return;
        }

        var hubConnection = api.htmxProp(hubElement).HubConnection;

        var signalrSubscribeAttribute = api.attributeValue(elt, signalRSubscribe);
        var signalrMethodNames = signalrSubscribeAttribute.split(",");

        for (let i = 0; i < signalrMethodNames.length; i++) {
            var method = signalrMethodNames[i].trim();

            hubConnection.on(method, function handler(message) {
                if (maybeCloseHubConnectionSource(hubElement)) {
                    hubConnection.off(method, handler);
                    return;
                }

                if (maybeUnsubscribe(hubElement, method, elt, handler)) {
                    return;
                }

                if (!api.triggerHtmxEvent(elt, 'htmx:signalr:message', { message: message, method: method })) {
                    return;
                }

                // The rest of htmx expects HTML content as a string, so serialize objects
                if (typeof message === "object") {
                    message = JSON.stringify(message);
                }

                if (message === null || message === undefined) {
                    return;
                }

                var template = document.createElement('template');
                template.innerHTML = message;

                api.insertContent({
                    target: resolveTarget(elt),
                    swapSpec: api.attributeValue(elt, "hx-swap") ?? htmx.config.defaultSwap,
                    fragment: template.content
                });
            });
        }
    }

    /**
     * processHubConnectionSend wires up the element's trigger (hx-trigger, or a sensible
     * default per tag) so that messages are sent to the HubConnection on that trigger.
     * @param {HTMLElement} hubElt
     * @param {HTMLElement} sendElt
     */
    function processHubConnectionSend(hubElt, sendElt) {
        var triggerSpec = api.attributeValue(sendElt, "hx-trigger") || defaultTriggerFor(sendElt);

        api.onTrigger(sendElt, triggerSpec, function (evt) {
            // Mirrors htmx core: decide + apply preventDefault synchronously, before any
            // async work below, since evt.currentTarget/preventDefault are only meaningful
            // during the event's own dispatch.
            if (shouldCancelDefaultAction(evt)) {
                evt.preventDefault();
            }

            sendToHub(hubElt, sendElt, evt);
        });
    }

    /**
     * @param {HTMLElement} hubElt
     * @param {HTMLElement} sendElt
     * @param {Event} evt
     */
    async function sendToHub(hubElt, sendElt, evt) {
        var hubConnection = api.htmxProp(hubElt).HubConnection;
        var method = api.attributeValue(sendElt, signalRSend);
        var validate = api.attributeValue(sendElt, "hx-validate") === "true";
        var form = sendElt.form || sendElt.closest("form");

        var body = api.collectFormData(sendElt, form, evt.submitter, validate, false);
        if (!body) {
            api.triggerHtmxEvent(sendElt, 'htmx:validation:halted', { warn: "form validation failed" });
            return;
        }

        var parameters = Object.fromEntries(body);

        var valsResult = api.getAttributeObject(sendElt, "hx-vals", function (obj) {
            Object.assign(parameters, obj);
        });
        if (valsResult) await valsResult;

        var headersResult = api.getAttributeObject(sendElt, "hx-headers", function (obj) {
            parameters['HEADERS'] = obj;
        });
        if (headersResult) await headersResult;

        if (!api.triggerHtmxEvent(sendElt, 'htmx:signalr:beforeSend', { method: method, allParameters: parameters })) {
            return;
        }

        hubConnection.send(method, parameters);

        api.triggerHtmxEvent(sendElt, 'htmx:signalr:afterSend', { method: method, message: parameters });
    }

    /**
     * maybeCloseHubConnectionSource checks if the element that created the HubConnection
     * still exists in the DOM. If NOT, then the HubConnection is closed and this function
     * returns TRUE. If the element DOES EXIST, then no action is taken, and this function
     * returns FALSE.
     *
     * @param {HTMLElement} elt
     * @returns
     */
    function maybeCloseHubConnectionSource(elt) {
        if (!document.body.contains(elt)) {
            api.htmxProp(elt).HubConnection.stop();
            return true;
        }
        return false;
    }

    /**
     * maybeUnsubscribe checks if the element that created the subscription to method
     * still has matching subscription attribute. If NOT, then the subscription is removed and this function
     * returns TRUE. If the element DOES EXIST, then no action is taken, and this function
     * returns FALSE.
     *
     * @param {HTMLElement} hubElement
     * @param {string} subscription
     * @param {HTMLElement} elt
     * @param {Function} handler
     * @returns
     */
    function maybeUnsubscribe(hubElement, subscription, elt, handler) {
        if (!document.body.contains(elt)) {
            api.htmxProp(hubElement).HubConnection.off(subscription, handler);
            return true;
        }
        if (api.attributeValue(elt, signalRSubscribe).split(",").indexOf(subscription) === -1) {
            api.htmxProp(hubElement).HubConnection.off(subscription, handler);
            return true;
        }
        return false;
    }

    /**
     * createHubConnection is the default method for creating new HubConnection objects.
     * it is hoisted into htmx.createHubConnection to be overridden by the user, if needed.
     *
     * @param {string} url
     * @returns HubConnection
     */
    function createHubConnection(url) {
        return new signalR.HubConnectionBuilder()
            .withUrl(url)
            .withAutomaticReconnect()
            .build();
    }

    /**
     * resolveTarget resolves the swap target for a subscribed element: its "hx-target"
     * attribute (as a plain CSS selector), or the element itself.
     * @param {HTMLElement} elt
     */
    function resolveTarget(elt) {
        var selector = api.attributeValue(elt, "hx-target");
        if (!selector || selector === "this") {
            return elt;
        }
        return document.querySelector(selector) || elt;
    }

    /**
     * defaultTriggerFor mirrors htmx core's own default hx-trigger fallback by tag.
     * @param {HTMLElement} elt
     */
    function defaultTriggerFor(elt) {
        if (elt.matches("form")) return "submit";
        if (elt.matches("input:not([type=button]):not([type=submit]),select,textarea")) return "change";
        return "click";
    }

    /**
     * shouldCancelDefaultAction mirrors htmx core's own default-action guard, so a
     * signalr-send button/form doesn't also submit/navigate natively.
     * @param {Event} evt
     */
    function shouldCancelDefaultAction(evt) {
        var elt = evt.currentTarget;
        var isSubmit = evt.type === 'submit' && elt?.tagName === 'FORM';
        if (isSubmit) return true;

        var isClick = evt.type === 'click' && evt.button === 0;
        if (!isClick) return false;

        var btn = elt?.closest?.('button, input[type="submit"], input[type="image"]');
        var form = btn?.form || btn?.closest('form');
        var isSubmitButton = btn && !btn.disabled && form &&
            (btn.type === 'submit' || btn.type === 'image' || (!btn.type && btn.tagName === 'BUTTON'));
        if (isSubmitButton) return true;

        var link = elt?.closest?.('a');
        if (!link || !link.href) return false;

        var href = link.getAttribute('href');
        var isFragmentOnly = href && href.startsWith('#') && href.length > 1;
        return !isFragmentOnly;
    }

    /**
     * queryAttributeOnThisOrChildren returns all nodes that contain the requested attributeName, INCLUDING THE PROVIDED ROOT ELEMENT.
     *
     * @param {HTMLElement} elt
     * @param {string} attributeName
     */
    function queryAttributeOnThisOrChildren(elt, attributeName) {

        var result = [];

        // If the parent element also contains the requested attribute, then add it to the results too.
        if (elt.hasAttribute?.(attributeName)) {
            result.push(elt);
        }

        // Search all child nodes that match the requested attribute
        elt.querySelectorAll?.("[" + attributeName + "], [data-" + attributeName + "]").forEach(function (node) {
            result.push(node);
        });

        return result;
    }

    /**
     * findParentWithHubConnection returns the closest element (including itself) that owns a
     * HubConnection.
     * @param {HTMLElement} elt
     */
    function findParentWithHubConnection(elt) {
        var node = elt;
        while (node) {
            if (hasHubConnection(node)) return node;
            node = node.parentElement;
        }
        return null;
    }

    function hasHubConnection(node) {
        return node._htmx?.HubConnection != null;
    }

    /**
     * @template T
     * @param {T[]} arr
     * @param {(T) => void} func
     */
    function forEach(arr, func) {
        if (arr) {
            for (var i = 0; i < arr.length; i++) {
                func(arr[i]);
            }
        }
    }
})();
