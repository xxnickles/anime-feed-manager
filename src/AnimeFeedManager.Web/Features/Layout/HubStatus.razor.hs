-- Live hub connection state, driven by hx-sse:connect's lifecycle events (#sse-connect in
-- App.razor). Compiled to HubStatus.Script and bound _="@Script". htmx and hyperscript init
-- independently with no guaranteed ordering, so init checks the connection's current state
-- directly (hx-sse.js stores it as #sse-connect's _htmx.sse.status) instead of assuming a
-- future event will arrive to correct an already-lost race. sse:message is a self-correcting
-- fallback — any message, including the heartbeat ping, proves liveness.
init
  js return document.getElementById('sse-connect')?._htmx?.sse?.status end
  if it
    remove .is-offline from #hub-dot
    put 'connected' into #hub-status-word
  end
end

on htmx:after:sse:connection or htmx:after:sse:message from #sse-connect
  remove .is-offline from #hub-dot
  put 'connected' into #hub-status-word
end

on htmx:sse:close or htmx:sse:error from #sse-connect
  add .is-offline to #hub-dot
  put 'disconnected' into #hub-status-word
end
