-- Recent Activity outcome filter: click a pill to show only matching rows (or all). Bound to
-- this component's own root, not a page-level ancestor — hyperscript's runtime DOM observer
-- reinstalls _= scripts on any newly-inserted element, so this re-snapshots the current buttons
-- every time GET /admin/activity replaces this content, instead of going stale. Pill counts are
-- a fresh server render, not touched here.
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
