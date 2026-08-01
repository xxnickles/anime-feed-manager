-- Recent Activity outcome filter: click a pill to show only matching rows (or all). Pill
-- counts are a fresh server render (AdminActivityFeed) on every load/refresh, not touched here.
on click from <button[data-filter]/> in me
  set filter to target's @data-filter
  for btn in <button[data-filter]/> in me
    if btn is target
      set btn's @aria-pressed to 'true'
    else
      set btn's @aria-pressed to 'false'
    end
  end
  for row in <div[data-outcome-bucket]/> in me
    if filter is 'all' or row's @data-outcome-bucket is filter
      remove .hidden from row
    else
      add .hidden to row
    end
  end
end
