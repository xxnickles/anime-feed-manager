init
  set raw to my @data-close-seconds
  js (raw) return parseInt(raw, 10) || 5; end
  set :secondsLeft to it
  set :paused to false
  set :dismissed to false
  set counter to the first <span[data-toast-counter]/> in me
  if counter
    set counter's textContent to :secondsLeft + 's'
  end

  repeat forever
    wait 1s
    if :dismissed
      exit
    end
    if not :paused
      decrement :secondsLeft
      set counter to the first <span[data-toast-counter]/> in me
      if counter
        set counter's textContent to :secondsLeft + 's'
      end
      if :secondsLeft <= 0
        send dismiss to me
        exit
      end
    end
  end
end

on click from <button[data-toast-pause]/> in me
  if :dismissed
    exit
  end
  set :paused to not :paused
  set btn to the first <button[data-toast-pause]/> in me
  set pauseIcon to the first <path[data-toast-pause-icon]/> in btn
  set playIcon to the first <path[data-toast-play-icon]/> in btn
  if :paused
    add .btn-warning to btn
    remove .btn-info from btn
    add @hidden to pauseIcon
    remove @hidden from playIcon
    set btn's @title to 'Resume auto-dismiss'
  else
    add .btn-info to btn
    remove .btn-warning from btn
    remove @hidden from pauseIcon
    add @hidden to playIcon
    set btn's @title to 'Pause auto-dismiss'
  end
end

on click from <button[data-toast-dismiss]/> in me
  send dismiss to me
end

on dismiss from me
  if :dismissed
    exit
  end
  set :dismissed to true
  add .opacity-0 to me
  settle
  remove me
end
