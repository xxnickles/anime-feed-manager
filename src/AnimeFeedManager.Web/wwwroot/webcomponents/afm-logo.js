/* <afm-logo> — canvas-animated Anime Feed Manager mark.
   Attributes: src, state ("entrance"|"idle"|"loading"), loader ("ring"|"dots"|"bars"),
               autoplay, entrance-once, htmx-loading
   API: el.state, el.setState("loading"|"idle"), el.play("entrance") -> Promise

   htmx-loading drives state from global htmx in-flight requests. Light-DOM children are slotted
   as a fallback for no-JS and for the window before the canvas has drawn; `ready` then hides them.
   Honours prefers-reduced-motion. */
(() => {
  const VB = 2048;
  const key = d => { const m = /^M\s*(\S+)\s+(\S+)/.exec(d); return m ? `${m[1]} ${m[2]}` : ''; };
  const EYE_L = ['814.338 554.759', '991.291 626.234', '998.623 654.127', '1004.93 663.153', '949.553 540.405'];
  const EYE_R = ['1239.42 641.213', '1250.28 683.84', '1254.2 722.794', '1235.85 626.14'];
  const PIV_L = { x: 960, y: 880 }, PIV_R = { x: 1245, y: 915 };
  const easeOutElastic = t => t <= 0 ? 0 : t >= 1 ? 1 : Math.pow(2, -10 * t) * Math.sin((t * 10 - 0.75) * (2 * Math.PI / 3)) + 1;
  const easeOutCubic = t => 1 - Math.pow(1 - t, 3);
  const clamp01 = v => Math.max(0, Math.min(1, v));

  const motionQuery = window.matchMedia?.('(prefers-reduced-motion: reduce)');
  const reduced = () => motionQuery?.matches === true;

  // Parsed once per url and shared: the two breakpoint instances render the same artwork, and
  // Path2D is immutable and context-independent.
  const toPath = el => {
    let d = el.getAttribute('d');
    if (!d) { const cx = +el.getAttribute('cx'), cy = +el.getAttribute('cy'), r = +el.getAttribute('r'); d = `M ${cx - r} ${cy} a ${r} ${r} 0 1 0 ${2 * r} 0 a ${r} ${r} 0 1 0 ${-2 * r} 0 z`; }
    let fill = el.getAttribute('fill') || '#000';
    if (fill.startsWith('url')) fill = '#2A2230';
    const k = key(d);
    return { p: new Path2D(d), fill, eye: EYE_L.includes(k) ? 'L' : EYE_R.includes(k) ? 'R' : null };
  };
  const pathCache = new Map();
  const loadPaths = url => {
    if (!pathCache.has(url)) pathCache.set(url, fetch(url).then(r => r.text()).then(txt =>
      [...new DOMParser().parseFromString(txt, 'image/svg+xml').querySelectorAll('path,circle')].map(toPath)));
    return pathCache.get(url);
  };

  // Global htmx in-flight tracking: one counter, however many marks are listening.
  const tracking = new Set();
  let inflight = 0, htmxWired = false;
  const broadcast = s => tracking.forEach(el => el.setState(s));
  function wireHtmx() {
    if (htmxWired) return;
    htmxWired = true;
    // Bound to document, not body: a whole-body boost swap keeps the body node, but document is
    // immune to any swap strategy. Both events bubble that far. finally:request also fires on
    // error, abort and a cancelled before:request, so the counter cannot leak.
    document.addEventListener('htmx:before:request', () => {
      if (++inflight === 1) broadcast('loading');
    });
    document.addEventListener('htmx:finally:request', () => {
      inflight = Math.max(0, inflight - 1);
      if (inflight === 0) broadcast('idle');
    });
  }

  class AFMLogo extends HTMLElement {
    static get observedAttributes() { return ['state', 'src']; }
    constructor() {
      super();
      this.attachShadow({ mode: 'open' });
      // The slotted <img> fallback stays visible until the canvas has drawn once, so the async svg
      // load never shows a blank box. Without it the mark disappears between element upgrade and
      // first paint — invisible behind the entrance fade, glaring on a reload that skips it.
      this.shadowRoot.innerHTML =
        '<style>' +
        ':host{display:block;position:relative;width:100%;height:100%;min-width:16px;min-height:16px;vertical-align:middle}' +
        'canvas{position:absolute;inset:0;width:100%;height:100%;display:block;opacity:0}' +
        ':host([ready]) canvas{opacity:1}' +
        ':host([ready]) slot{display:none}' +
        'slot{display:block;position:absolute;inset:0}' +
        '</style><slot></slot><canvas></canvas>';
      this.canvas = this.shadowRoot.querySelector('canvas');
      this.ctx = this.canvas.getContext('2d');
      this.paths = [];
      this._state = 'idle';
      this._load = 0;              // 0..1 blend into loading look
      this._hop = 0;               // done-hop impulse
      this._entrance = null;       // {t0, resolve}
      this._entrancePlayed = false;
      this._blink = { next: 0, t0: -1, dbl: false };
      this._visible = true;
      this._running = false;
      this._ready = false;
      this._raf = 0;
    }
    get state() { return this._state; }
    /* With entrance-once, the entrance is claimed once per tab rather than once per document.
       hx-preserve covers boosted navigation, but auth transitions use HX-Redirect — a real browser
       reload that no swap can survive — and F5 is the same shape. Keyed per id so every mark still
       plays on a genuine first visit. Storage can throw when blocked; play in that case. */
    _claimEntrance() {
      if (!this.hasAttribute('entrance-once')) return true;
      const k = 'afm-entrance:' + (this.id || this.getAttribute('src') || 'default');
      try {
        if (sessionStorage.getItem(k)) return false;
        sessionStorage.setItem(k, '1');
      } catch { /* storage unavailable */ }
      return true;
    }
    connectedCallback() {
      this._ro = new ResizeObserver(() => this._resize());
      this._ro.observe(this);
      this._io = new IntersectionObserver(e => { this._visible = e[0].isIntersecting; this._tick(); });
      this._io.observe(this);
      this._onVis = () => this._tick();
      document.addEventListener('visibilitychange', this._onVis);
      this._onMotion = () => { this._start(); };
      motionQuery?.addEventListener('change', this._onMotion);
      if (this.hasAttribute('htmx-loading')) {
        wireHtmx();
        tracking.add(this);
        if (inflight > 0) this._state = 'loading';
      }
      this._resize();
      // hx-preserve re-parents this node, which re-runs connectedCallback. Re-fetching the svg and
      // replaying the entrance there would undo the point of preserving it.
      if (this.paths.length) { this._start(); return; }
      this._loadSvg(this.getAttribute('src') || '/logo.svg').then(() => {
        const s = this.getAttribute('state');
        if (!this._entrancePlayed && (this.hasAttribute('autoplay') || s === 'entrance') && this._claimEntrance()) this.play('entrance');
        else if (s && s !== 'entrance') this.setState(s); else this._start();
      });
    }
    disconnectedCallback() {
      this._ro?.disconnect(); this._io?.disconnect();
      document.removeEventListener('visibilitychange', this._onVis);
      motionQuery?.removeEventListener('change', this._onMotion);
      tracking.delete(this);
      cancelAnimationFrame(this._raf); this._running = false;
    }
    attributeChangedCallback(n, o, v) {
      if (n === 'state' && v && v !== this._state && this.paths.length) this.setState(v);
      if (n === 'src' && o && v) this._loadSvg(v);
    }
    async _loadSvg(url) {
      this.paths = await loadPaths(url);
      this._start();
    }
    _resize() {
      const r = this.getBoundingClientRect(); const dpr = window.devicePixelRatio || 1;
      const w = Math.max(1, Math.round(r.width * dpr)), h = Math.max(1, Math.round(r.height * dpr));
      if (this.canvas.width !== w || this.canvas.height !== h) { this.canvas.width = w; this.canvas.height = h; }
      this._draw(performance.now());
    }
    play(name) {
      if (name !== 'entrance') { this.setState(name); return Promise.resolve(); }
      this._entrancePlayed = true;
      return new Promise(resolve => {
        this._entrance = { t0: performance.now(), resolve };
        this._setStateRaw('entrance');
        this._blink.next = this._entrance.t0 + 1150; this._blink.dbl = true;
        this._start();
      // Settles itself, so callers just await. Guarded: a request may have arrived mid-entrance,
      // and dropping to idle would discard that loading state.
      }).then(() => { if (this._state === 'entrance') this.setState('idle'); });
    }
    setState(s) {
      if (s === 'entrance') return this.play('entrance');
      if (this._state === 'loading' && s === 'idle' && !reduced()) this._hop = 1;
      this._setStateRaw(s);
      this._start();
    }
    _setStateRaw(s) {
      if (this._state === s) return;
      this._state = s;
      if (this.getAttribute('state') !== s) this.setAttribute('state', s);
    }
    // Off-breakpoint instances are display:none and never intersect; scheduling a frame they would
    // immediately park wastes one rAF per state change, and state changes on every htmx request.
    _start() {
      if (this._running || !this._visible || document.hidden) return;
      this._running = true; this._last = performance.now();
      this._raf = requestAnimationFrame(t => this._frame(t));
    }
    _tick() { if (this._visible && !document.hidden) this._start(); }
    _frame(t) {
      if (!this._visible || document.hidden) { this._running = false; return; }
      const dt = Math.min(0.05, (t - this._last) / 1000); this._last = t;
      const target = this._state === 'loading' ? 1 : 0;
      this._load += (target - this._load) * Math.min(1, dt * 6);
      if (Math.abs(target - this._load) < 0.002) this._load = target;
      if (this._hop > 0) this._hop = Math.max(0, this._hop - dt * 2.2);
      if (this._entrance && t - this._entrance.t0 > 1500) { const r = this._entrance.resolve; this._entrance = null; r(); }
      this._draw(t);
      // Under reduced motion an idle mark is fully static — park the loop instead of burning frames.
      if (reduced() && !this._entrance && this._load === 0 && this._hop === 0) { this._running = false; return; }
      this._raf = requestAnimationFrame(tt => this._frame(tt));
    }
    _blinkAmt(t) {
      if (reduced()) return 0;
      const b = this._blink;
      if (b.next === 0) b.next = t + 1200 + Math.random() * 2000;
      if (b.t0 < 0 && t >= b.next) { b.t0 = t; }
      if (b.t0 >= 0) {
        const p = (t - b.t0) / 200;
        if (p >= 1) {
          b.t0 = -1;
          if (b.dbl) { b.dbl = false; b.next = t + 90; } else { b.dbl = Math.random() < 0.2; b.next = t + 2400 + Math.random() * 3200; }
          return 0;
        }
        return Math.sin(Math.PI * p);
      }
      return 0;
    }
    _draw(t) {
      const { ctx, canvas } = this; if (!this.paths.length) return;
      const W = canvas.width, H = canvas.height; const s = Math.min(W, H) / VB;
      ctx.setTransform(1, 0, 0, 1, 0, 0); ctx.clearRect(0, 0, W, H);
      const calm = reduced();
      const amp = calm ? 0 : 1;
      const sec = t / 1000, L = this._load;
      // entrance — a plain fade when reduced motion is asked for, no elastic pop
      let pop = 1, alpha = 1;
      if (this._entrance) {
        const e = clamp01((t - this._entrance.t0) / 1100);
        if (!calm) pop = 0.2 + 0.8 * easeOutElastic(e);
        alpha = clamp01(e * 5);
      }
      // calm body: same tempo in every state; loading only leans her toward the tablet
      const breathe = 1 + amp * 0.012 * Math.sin(sec * Math.PI * 2 * 0.3);
      const bob = amp * 10 * Math.sin(sec * Math.PI * 2 * 0.55);
      const sway = amp * 0.010 * Math.sin(sec * Math.PI * 2 * 0.22 + 1) + amp * 0.018 * L;
      const hop = -Math.sin(this._hop * Math.PI) * 70 * amp;
      // world transform: fit, anchor at bottom-centre
      ctx.translate((W - VB * s) / 2, (H - VB * s) / 2); ctx.scale(s, s);
      ctx.translate(VB / 2, 1900);
      ctx.scale(pop, pop); ctx.translate(0, bob + hop); ctx.scale(1, breathe); ctx.transform(1, 0, sway, 1, 0, 0);
      ctx.translate(-VB / 2, -1900);
      ctx.globalAlpha = alpha;
      // eyes
      const blink = this._blinkAmt(t);
      const eyeSy = 1 - 0.9 * blink;
      const gazeX = 8 + 2 * Math.sin(sec * 0.9);   // toward the tablet while loading
      const eyeShift = { x: amp * (L * gazeX + (1 - L) * 6 * Math.sin(sec * 0.6)), y: amp * (L * 14 + (1 - L) * 4 * Math.sin(sec * 0.4)) };
      for (const q of this.paths) {
        if (q.eye) {
          const pv = q.eye === 'L' ? PIV_L : PIV_R;
          ctx.save(); ctx.translate(eyeShift.x, eyeShift.y); ctx.translate(pv.x, pv.y); ctx.scale(1, eyeSy); ctx.translate(-pv.x, -pv.y);
          ctx.fillStyle = q.fill; ctx.fill(q.p); ctx.restore();
        } else { ctx.fillStyle = q.fill; ctx.fill(q.p); }
      }
      if (L > 0.005) this._drawBubble(ctx, sec, L, alpha, Math.min(W, H) / (window.devicePixelRatio || 1), calm);
      ctx.globalAlpha = 1;
      // Hand over from the slotted fallback only once there is a real frame behind it.
      if (!this._ready) { this._ready = true; this.setAttribute('ready', ''); }
    }
    _drawBubble(ctx, sec, L, alpha, px, calm) {
      const style = this.getAttribute('loader') || 'bars';
      // speech bubble popping out of the tablet's top-right corner, sized up for tiny renders.
      // Under reduced motion it sits still and only the loader itself moves — that motion is the
      // state feedback, so it stays, at half speed.
      const beat = calm ? sec * 0.5 : sec;
      const k = Math.min(1.42, 1.35 * Math.max(1, Math.min(1.5, 110 / Math.max(1, px))));
      const pop = calm ? 1 : easeOutCubic(clamp01(L));   // symmetric: same ease growing in and shrinking out
      const ax = 1600, ay = 1150;                       // tail anchor on the tablet corner
      const float = calm ? 0 : 10 * Math.sin(sec * 1.6);
      ctx.save(); ctx.globalAlpha = alpha * clamp01(L * 1.5);
      const w = 330, h = 230, r = 56, bx = 0, by = -h - 200;   // body up and to the right of the tablet corner
      ctx.translate(ax, ay + float); ctx.scale(k, k); ctx.translate(bx + w / 2, by + h / 2); ctx.scale(pop, pop); ctx.translate(-(bx + w / 2), -(by + h / 2));
      ctx.beginPath(); ctx.roundRect(bx, by, w, h, r);
      ctx.fillStyle = '#FFFFFF'; ctx.fill(); ctx.lineWidth = 16; ctx.strokeStyle = '#2A2230'; ctx.stroke();
      // thought-bubble trail: three circles from the tablet corner up to the body, small to large
      // circles sit on a quadratic Bézier from the tablet corner (0,0) curving out right then up into the body
      const P0 = { x: 0, y: 0 }, P1 = { x: 130, y: -20 }, P2 = { x: bx + w * 0.55, y: by + h + 10 };
      const bez = t => ({ x: (1 - t) * (1 - t) * P0.x + 2 * (1 - t) * t * P1.x + t * t * P2.x, y: (1 - t) * (1 - t) * P0.y + 2 * (1 - t) * t * P1.y + t * t * P2.y });
      [[0.12, 13], [0.45, 22], [0.78, 32]].forEach(([at, rad], i) => {
        const grow = clamp01((pop - i * 0.2) / 0.6);
        if (grow <= 0) return;
        const c = bez(at);
        ctx.beginPath(); ctx.arc(c.x, c.y, rad * grow, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
      });
      ctx.translate(bx + w / 2, by + h / 2);
      const ink = '#2A2230', soft = 'rgba(42,34,48,0.18)';
      if (style === 'dots') {
        for (let i = 0; i < 3; i++) { const ph = beat * 5 - i * 0.7; const y = -Math.max(0, Math.sin(ph)) * 34; ctx.fillStyle = ink; ctx.beginPath(); ctx.arc(-70 + i * 70, y + 10, 22, 0, Math.PI * 2); ctx.fill(); }
      } else if (style === 'bars') {
        for (let i = 0; i < 4; i++) { const hgt = 40 + 60 * (0.5 + 0.5 * Math.sin(beat * 6 + i * 0.9)); ctx.fillStyle = ink; ctx.beginPath(); ctx.roundRect(-92 + i * 56, -hgt / 2, 30, hgt, 15); ctx.fill(); }
      } else {
        const a0 = beat * 3.4; ctx.lineWidth = 20; ctx.lineCap = 'round';
        ctx.strokeStyle = soft; ctx.beginPath(); ctx.arc(0, 0, 62, 0, Math.PI * 2); ctx.stroke();
        ctx.strokeStyle = ink; ctx.beginPath(); ctx.arc(0, 0, 62, a0, a0 + Math.PI * 1.25); ctx.stroke();
      }
      ctx.restore();
    }
  }
  if (!customElements.get('afm-logo')) customElements.define('afm-logo', AFMLogo);
})();
