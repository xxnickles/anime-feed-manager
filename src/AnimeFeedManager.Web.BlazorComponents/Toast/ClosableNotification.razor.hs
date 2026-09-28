init
  set raw to my @data-close-seconds
  if no raw
    exit
  end
  set :secondsLeft to raw as Int
  repeat until :secondsLeft <= 0
    wait 1s
    if :dismissed
      exit
    end
    decrement :secondsLeft
  end
  send dismiss to me
end

on click from <button[data-toast-dismiss]/> in me
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
