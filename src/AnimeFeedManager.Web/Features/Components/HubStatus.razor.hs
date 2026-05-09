init
  js return localStorage.getItem('last-hub-status') || 'connecting'; end
  set :status to it

  js return localStorage.getItem('hub-has-connected') === 'true'; end
  set :hasConnected to it

  js (me)
    window.addEventListener('htmx:signalr:start-error', function (e) {
      me.dispatchEvent(new CustomEvent('signalrStartError', { detail: e.detail }));
    });
  end

  send hubStatusApplied to me
end

on htmx:signalr:starting from window
  set :status to 'connecting'
  send hubStatusApplied to me
end

on htmx:signalr:start from window
  set :hasConnected to true
  js localStorage.setItem('hub-has-connected', 'true'); end
  set :status to 'connected'
  send hubStatusApplied to me
end

on signalrStartError
  if :hasConnected
    set :status to 'error'
  else
    set :status to 'unavailable'
  end
  send hubStatusApplied to me
end

on htmx:signalr:reconnecting from window
  if :hasConnected
    set :status to 'reconnecting'
  else
    set :status to 'connecting'
  end
  send hubStatusApplied to me
end

on htmx:signalr:reconnected from window
  set :status to 'connected'
  send hubStatusApplied to me
end

on htmx:signalr:close from window
  if :hasConnected
    set :status to 'error'
  else
    set :status to 'unavailable'
  end
  send hubStatusApplied to me
end

on hubStatusApplied
  set status to :status
  js (status) localStorage.setItem('last-hub-status', status); end

  repeat for icon in <svg[data-state]/> in me
    if icon's @data-state is status
      remove @hidden from icon
    else
      add @hidden to icon
    end
  end

  set color to the first <span[data-hub-color]/> in me
  if color
    remove .text-success from color
    remove .text-info from color
    remove .text-warning from color
    remove .text-error from color
    if status is 'connected'
      add .text-success to color
    else if status is 'connecting'
      add .text-info to color
    else if status is 'reconnecting'
      add .text-warning to color
    else
      add .text-error to color
    end
  end

  if status is 'connected'
    set tipText to 'Connected'
    set ariaText to 'Hub status: connected'
  else if status is 'connecting'
    set tipText to 'Connecting'
    set ariaText to 'Hub status: connecting'
  else if status is 'reconnecting'
    set tipText to 'Reconnecting'
    set ariaText to 'Hub status: reconnecting'
  else if status is 'error'
    set tipText to 'Not available'
    set ariaText to 'Hub status: not available'
  else if status is 'unavailable'
    set tipText to 'Not available'
    set ariaText to 'Hub status: not available'
  else
    set tipText to 'Status'
    set ariaText to 'Hub status: ' + status
  end

  set tooltip to the first <span[data-hub-tooltip]/> in me
  if tooltip
    set tooltip's @data-tip to tipText
    set tooltip's @aria-label to ariaText
  end

  set sr to the first <span[data-hub-srlabel]/> in me
  if sr
    put ariaText into sr
  end
end
