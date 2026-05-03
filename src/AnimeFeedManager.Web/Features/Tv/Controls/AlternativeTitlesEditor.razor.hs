on addTitle from me
  set inp to the first <input[data-title-input]/> in me
  if no inp
    exit
  end
  set raw to ((inp.value or '') + '').trim()
  if raw is ''
    exit
  end
  set key to raw.toLowerCase()

  set duplicate to false
  repeat for hi in <input[data-alt-title-hidden]/> in me
    if ((hi.value or '') + '').trim().toLowerCase() is key
      set duplicate to true
      break
    end
  end
  if duplicate
    exit
  end

  set tplEl to the first <template[data-row-blueprint]/> in me
  if no tplEl
    exit
  end
  set newRow to tplEl.content.firstElementChild.cloneNode(true)
  set titleEl to the first <div[data-row-title]/> in newRow
  set hiEl to the first <input[data-alt-title-hidden]/> in newRow
  if titleEl
    set titleEl's textContent to raw
  end
  if hiEl
    set hiEl's value to raw
  end

  set list to the first <ul.alt-titles-list/> in me
  put newRow at the end of list

  set inp's value to ''
  send listChanged to me
  wait a tick
  call inp.focus()
end

on removeTitle(row) from me
  if row
    remove row
  end
  send listChanged to me
end

on listChanged from me
  set fld to my @data-field-name
  set raw to my @data-items
  if no raw
    set raw to '[]'
  end
  set baseline to raw as JSON

  set list to the first <ul.alt-titles-list/> in me
  set rows to <li.list-row/> in list

  set current to []
  repeat for li in rows index i
    set hi to the first <input[data-alt-title-hidden]/> in li
    if hi
      append hi.value to current
      set hi's name to ((fld + '[') + i) + ']'
    end
    set indexEl to the first <div[data-row-index]/> in li
    if indexEl
      set indexEl's textContent to ((i + 1) + '').padStart(2, '0')
    end
  end

  set isDirty to false
  if current.length is not baseline.length
    set isDirty to true
  else
    repeat for v in current index i
      set a to ((v or '') + '').trim().toLowerCase()
      set b to ((baseline[i] or '') + '').trim().toLowerCase()
      if a is not b
        set isDirty to true
        break
      end
    end
  end

  set banner to the first <div.alt-titles-dirty-banner/> in me
  set submitBtn to the first <input.alt-titles-submit/> in me
  set emptyState to the first <li.alt-titles-empty/> in me

  if isDirty
    if banner
      remove @hidden from banner
    end
    if submitBtn
      remove @hidden from submitBtn
    end
  else
    if banner
      add @hidden to banner
    end
    if submitBtn
      add @hidden to submitBtn
    end
  end

  if rows.length is 0
    if emptyState
      remove @hidden from emptyState
    end
  else
    if emptyState
      add @hidden to emptyState
    end
  end
end

on htmx:confirm(issueRequest) from me
  set frm to me
  js (issueRequest, frm)
    return function(proceed) {
      if (proceed) {
        const dlg = frm.closest('dialog');
        dlg && dlg.close();
      }
      return issueRequest(proceed);
    };
  end
  set event.detail.issueRequest to it
end
