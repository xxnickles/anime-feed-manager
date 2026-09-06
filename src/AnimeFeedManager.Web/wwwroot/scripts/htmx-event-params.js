/*
Event Params Extension
============================
This extension appends custom event detail values to HTMX request URLs.

Usage:
  <div hx-get="/endpoint"
       hx-trigger="load, my-event from:body"
       data-param-name="period">

When 'my-event' is dispatched with { detail: { value: '30d' } },
the request URL becomes '/endpoint?period=30d'.

External trigger example:
  <select _="on change send my-event(value: my value) to body">
*/

(function () {

    htmx.registerExtension("event-params", {

        /**
         * Runs on every htmx:config:request, just before the request URL/body is finalized.
         *
         * @param {HTMLElement} elt
         * @param {{ctx: object}} detail
         */
        htmx_config_request: function (elt, detail) {

            var ctx = detail.ctx;
            var triggeringEvent = ctx.sourceEvent;
            var paramName = elt.dataset.paramName;

            // Only append if we have both a value from the event and a param name configured
            if (!triggeringEvent?.detail?.value || !paramName) {
                return;
            }

            var separator = ctx.request.action.includes("?") ? "&" : "?";
            ctx.request.action += separator + paramName + "=" + encodeURIComponent(triggeringEvent.detail.value);
        }
    });

})();
