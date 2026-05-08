init
  set canvas to the first <canvas[data-chart-canvas]/> in me
  if no canvas
    exit
  end
  set raw to my @data-chart-config
  if no raw
    exit
  end
  set config to raw as JSON
  set container to me

  js (canvas, container, config)
    canvas.width = container.clientWidth;
    canvas.height = container.clientHeight;
    config.options = config.options || {};
    config.options.responsive = false;
    return new Chart(canvas, config);
  end
  set :chart to it

  wait a tick
  set chart to :chart
  if chart
    js (chart)
      chart.options.responsive = true;
      chart.resize();
    end
  end
end

on remove
  set chart to :chart
  if chart
    js (chart) chart.destroy(); end
    set :chart to null
  end
end
