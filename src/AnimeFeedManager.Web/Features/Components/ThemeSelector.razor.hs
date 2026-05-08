init
  js return localStorage.getItem('user-theme') || 'default'; end
  set stored to it

  set radio to null
  repeat for r in <input[data-theme-radio]/> in me
    if r.value is stored
      set radio to r
      break
    end
  end

  if no radio
    set stored to 'default'
    repeat for r in <input[data-theme-radio]/> in me
      if r.value is 'default'
        set radio to r
        break
      end
    end
  end

  if radio
    set radio's checked to true
  end
  send themeApplied(value: stored) to me
end

on change from <input[data-theme-radio]/> in me
  set chosen to target.value
  js (chosen) localStorage.setItem('user-theme', chosen); end
  send themeApplied(value: chosen) to me
end

on themeApplied(value) from me
  repeat for icon in <svg[data-theme-icon]/> in me
    if icon's @data-theme-icon is value
      remove @hidden from icon
    else
      add @hidden to icon
    end
  end

  repeat for opt in <div[data-theme-option]/> in me
    set isActive to (opt's @data-theme-option is value)
    if isActive
      add .btn-active to opt
    else
      remove .btn-active from opt
    end
    set checkmark to the first <svg[data-theme-checkmark]/> in opt
    if checkmark
      if isActive
        remove @hidden from checkmark
      else
        add @hidden to checkmark
      end
    end
  end

  set displayName to value
  repeat for r in <input[data-theme-radio]/> in me
    if r.value is value
      set displayName to (r's @data-theme-name)
      break
    end
  end
  set summary to the first <summary[data-theme-summary]/> in me
  if summary
    set summary's @title to 'Change theme, current: ' + displayName
    set summary's @aria-label to 'Theme selector, currently selected: ' + displayName
  end
end
