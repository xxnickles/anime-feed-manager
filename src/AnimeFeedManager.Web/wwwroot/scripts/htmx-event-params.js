/*
Event Params Extension
============================
This extension appends custom event detail values to HTMX request URLs.

Usage:
  <div hx-get="/endpoint"
       hx-trigger="load, my-event from:body"
       hx-ext="event-params"
       data-param-name="period">

When 'my-event' is dispatched with { detail: { value: '30d' } },
the request URL becomes '/endpoint?period=30d'.

External trigger example:
  <select x-data @change="$dispatch('my-event', { value: $el.value })">
*/

(function () {

    htmx.defineExtension("event-params", {

        /**
         * onEvent handles all events passed to this extension.
         *
         * @param {string} name
         * @param {CustomEvent} evt
         */
        onEvent: function (name, evt) {

            if (name !== "htmx:configRequest") {
                return;
            }

            var triggeringEvent = evt.detail.triggeringEvent;
            var paramName = evt.detail.elt.dataset.paramName;

            // Only append if we have both a value from the event and a param name configured
            if (!triggeringEvent?.detail?.value || !paramName) {
                return;
            }

            var separator = evt.detail.path.includes("?") ? "&" : "?";
            evt.detail.path += separator + paramName + "=" + encodeURIComponent(triggeringEvent.detail.value);
        }
    });

})();
