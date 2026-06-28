-- Single-select type filter for the season grid. Clicking a segment marks it pressed and
-- shows only matching cards (or all). Compiled to `SeasonGrid.Script` by the hyperscript
-- source generator and bound via _="@Script" on the grid container.
on click from <button[data-filter]/> in me
  set filter to target's @data-filter
  for btn in <button[data-filter]/> in me
    if btn is target
      set btn's @aria-pressed to 'true'
    else
      set btn's @aria-pressed to 'false'
    end
  end
  for card in <a[data-type]/> in me
    if filter is 'all' or card's @data-type is filter
      remove .hidden from card
    else
      add .hidden to card
    end
  end
end
