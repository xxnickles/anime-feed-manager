-- Live hub connection state, driven by the real hx-sse:connect element's lifecycle
-- events (#sse-connect in App.razor). Compiled to HubStatus.Script and bound _="@Script".
on htmx:after:sse:connection from #sse-connect
  remove .is-offline from #hub-dot
  put 'connected' into #hub-status-word
end

on htmx:sse:close from #sse-connect
  add .is-offline to #hub-dot
  put 'disconnected' into #hub-status-word
end

on htmx:sse:error from #sse-connect
  add .is-offline to #hub-dot
  put 'disconnected' into #hub-status-word
end
