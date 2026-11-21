/* <afm-chart> — ApexCharts host with daisyUI-styled presets.
   Attributes: data-kind ("line"|"column"), data-chart (JSON {categories, series: [{name, data, color}]}),
               id (optional; becomes chart.id, which registers the chart in Apex._chartInstances)
   Renders on connect and destroys on disconnect: ApexCharts holds a window resize listener per chart.
   Any bad input renders an error alert instead. Kinds and color tokens mirror the server's closed sets. */
const TOKENS = new Set(['base-content', 'primary', 'secondary', 'accent', 'neutral', 'info', 'success', 'warning', 'error']);
const token = name => `var(--color-${name})`;
const faint = pct => `color-mix(in srgb, var(--color-base-content), transparent ${pct}%)`;

// ApexCharts writes tooltip strings as innerHTML.
const ESCAPES = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' };
const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ESCAPES[c]);

const tooltipRow = (color, name, value) => `
  <div class="flex items-center justify-between gap-4 py-1">
    <div class="flex items-center gap-2">
      <span class="size-2 rounded-selector" style="background-color:${esc(color)}"></span>
      <span class="text-base-content/60 text-xs">${esc(name)}</span>
    </div>
    <span class="text-base-content font-semibold text-xs">${esc(Number(value).toLocaleString())}</span>
  </div>`;

const sharedTooltip = {
  shared: true,
  intersect: false,
  custom: ({ series, dataPointIndex, w }) =>
    `<div class="p-3">${w.config.series.map((s, i) =>
      tooltipRow(w.globals.colors[i], s.name, series[i][dataPointIndex])).join('')}</div>`,
};

const cartesian = (type, data, id) => ({
  chart: {
    ...(id && { id }),
    type,
    height: 300,
    fontFamily: 'inherit',
    toolbar: { show: false },
    zoom: { enabled: false },
  },
  series: data.series.map(({ name, data: values }) => ({ name, data: values })),
  colors: data.series.map(s => token(s.color)),
  xaxis: {
    categories: data.categories,
    labels: {
      rotate: 0,
      hideOverlappingLabels: true,
      style: { colors: token('base-content'), fontSize: '12px' },
    },
    axisBorder: { show: false },
    axisTicks: { show: false },
    tooltip: { formatter: esc },
  },
  yaxis: { show: false },
  legend: { show: false },
  dataLabels: { enabled: false },
  grid: { borderColor: faint(95), padding: { top: 10, right: 0, bottom: 0, left: 10 } },
  tooltip: sharedTooltip,
});

const presets = {
  line: (data, id) => {
    const options = cartesian('line', data, id);
    options.chart.dropShadow = { enabled: true, opacity: 0.3, blur: 8, top: 20 };
    options.stroke = { width: 4, curve: 'smooth' };
    return options;
  },
  column: (data, id) => {
    const options = cartesian('bar', data, id);
    options.plotOptions = { bar: { horizontal: false, columnWidth: '55%', borderRadius: 5, borderRadiusApplication: 'end' } };
    options.stroke = { show: true, width: 2, colors: ['transparent'] };
    options.fill = { opacity: 1 };
    options.xaxis.labels.style.opacity = 0.5;
    options.xaxis.axisBorder = { show: true, color: faint(90) };
    options.xaxis.axisTicks = { show: true, color: faint(90) };
    return options;
  },
};

const parse = raw => {
  const data = JSON.parse(raw ?? '');
  const valid = Array.isArray(data?.categories) && Array.isArray(data?.series) &&
    data.series.every(s => typeof s?.name === 'string' && Array.isArray(s.data) && TOKENS.has(s.color));
  if (!valid) throw new Error('data-chart does not match {categories, series: [{name, data, color}]}');
  return data;
};

class AfmChart extends HTMLElement {
  #chart;

  connectedCallback() {
    this.#destroy();
    try {
      if (typeof ApexCharts === 'undefined') throw new Error('ApexCharts is not loaded');
      const kind = this.dataset.kind;
      if (!Object.hasOwn(presets, kind)) throw new Error(`unknown kind "${kind}"`);
      const chart = new ApexCharts(this, presets[kind](parse(this.dataset.chart), this.id));
      this.#chart = chart;
      chart.render().catch(error => this.#fail(error, chart));
    } catch (error) {
      this.#fail(error);
    }
  }

  disconnectedCallback() {
    this.#destroy();
  }

  #destroy() {
    this.#chart?.destroy();
    this.#chart = undefined;
  }

  // A stale chart (replaced before its render settled) must not overwrite the current one.
  #fail(error, chart) {
    if (chart && chart !== this.#chart) return;
    console.error('afm-chart:', error);
    this.#destroy();
    const alert = document.createElement('div');
    alert.className = 'alert alert-error';
    alert.textContent = 'Failed to load chart';
    const frame = document.createElement('div');
    frame.className = 'h-75 flex items-center justify-center';
    frame.setAttribute('role', 'alert');
    frame.append(alert);
    this.replaceChildren(frame);
  }
}

if (!customElements.get('afm-chart')) customElements.define('afm-chart', AfmChart);
