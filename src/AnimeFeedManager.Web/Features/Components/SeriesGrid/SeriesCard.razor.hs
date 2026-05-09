on click
  set article to the closest <article/>
  set tooltip to the first <div[data-card-tooltip]/> in article
  set eyeIcon to the first <svg[data-icon='eye']/> in me
  set eyeOff to the first <svg[data-icon='eye-off']/> in me

  if my @aria-pressed is 'true'
    set my @aria-pressed to 'false'
    set my @aria-label to 'Show image'
    set tooltip's @data-tip to 'Show image'
    remove .btn-info from me
    add .btn-ghost to me
    remove @hidden from eyeIcon
    add @hidden to eyeOff
    remove @data-show-image from article
  else
    set my @aria-pressed to 'true'
    set my @aria-label to 'Show content'
    set tooltip's @data-tip to 'Show content'
    remove .btn-ghost from me
    add .btn-info to me
    add @hidden to eyeIcon
    remove @hidden from eyeOff
    add @data-show-image to article
  end
end
