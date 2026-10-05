on htmx:error from window
  set ctx to event.detail.ctx
  if no ctx
    exit
  end
  if ctx.response
    exit
  end
  if event.detail.error.name is 'AbortError'
    exit
  end
  set toast to me.content.firstElementChild.cloneNode(true)
  put toast at the end of #toast-panel
end
