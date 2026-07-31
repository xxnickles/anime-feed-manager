-- Recomputes the outcome-bucket pill counts whenever this element's own row set changes.
-- A filter click only toggles .hidden on existing rows (no childList change, so this doesn't
-- re-fire); a live content refresh replaces the rows outright, which does.
on mutation of anything
  set succeeded to 0
  set errored to 0
  for row in <div[data-outcome-bucket]/> in me
    if row's @data-outcome-bucket is 'succeeded'
      increment succeeded
    else
      increment errored
    end
  end
  set text of #activity-count-all to (succeeded + errored)
  set text of #activity-count-succeeded to succeeded
  set text of #activity-count-error to errored
end
