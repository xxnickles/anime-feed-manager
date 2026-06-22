-- Toast lifecycle: auto-dismiss countdown + manual dismiss + fade-out. Compiled to
-- Notification.Script by the hyperscript source generator and bound via _="@Script".
-- The duration is read from the element's data-close-seconds (the generated const is static,
-- so it can't interpolate the CloseTime parameter). wait only accepts literal times, hence the
-- 1s tick loop rather than a single dynamic wait.
init
  set raw to my @data-close-seconds
  js (raw) return parseInt(raw, 10) || 6; end
  set :secondsLeft to it
  repeat until :secondsLeft <= 0
    wait 1s
    if :dismissed
      exit
    end
    decrement :secondsLeft
  end
  send dismiss to me
end

on click from <button[data-dismiss]/> in me
  send dismiss to me
end

on dismiss
  if :dismissed
    exit
  end
  set :dismissed to true
  add .opacity-0 to me
  settle
  remove me
end
