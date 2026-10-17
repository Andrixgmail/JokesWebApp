// =====================================================================
// Andrea's Jokes & Riddles: live graphics & interactions
// (no external libraries needed beyond the Bootstrap that ships with the site)
// =====================================================================
(function () {
    'use strict';

    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var $ = function (sel, root) { return (root || document).querySelector(sel); };
    var $$ = function (sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); };

    function store(key, value) {
        try {
            if (value === undefined) return localStorage.getItem(key);
            localStorage.setItem(key, value);
        } catch (e) { return null; }
    }

    // ------------------------------------------------------------ theme toggle
    function initTheme() {
        var btn = $('#theme-toggle');
        if (!btn) return;
        var paint = function () {
            btn.textContent = document.documentElement.getAttribute('data-theme') === 'dark' ? '☀️' : '🌙';
        };
        paint();
        btn.addEventListener('click', function () {
            var next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-theme', next);
            store('jw-theme', next);
            paint();
        });
    }

    // ------------------------------------------------------------ confetti (full-screen canvas)
    var fx = { canvas: null, ctx: null, parts: [], running: false };
    function fxSize() {
        if (!fx.canvas) return;
        var dpr = window.devicePixelRatio || 1;
        fx.canvas.width = innerWidth * dpr; fx.canvas.height = innerHeight * dpr;
        fx.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    }
    function confetti(x, y, opts) {
        if (reduceMotion) return;
        fx.canvas = fx.canvas || $('#fx-canvas');
        if (!fx.canvas) return;
        if (!fx.ctx) { fx.ctx = fx.canvas.getContext('2d'); fxSize(); addEventListener('resize', fxSize); }
        opts = opts || {};
        var colors = opts.colors || ['#ff5a5f', '#ff9a3c', '#ffc93c', '#7b5cff', '#2ec5d3', '#22c55e'];
        var count = opts.count || 120;
        for (var i = 0; i < count; i++) {
            var angle = (opts.spread ? (Math.random() - .5) * opts.spread : Math.random() * Math.PI * 2) + (opts.angle || 0);
            var speed = 4 + Math.random() * 9;
            fx.parts.push({
                x: x, y: y,
                vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed - 4,
                w: 6 + Math.random() * 6, h: 8 + Math.random() * 8,
                rot: Math.random() * Math.PI, vr: (Math.random() - .5) * .3,
                color: colors[i % colors.length], life: 0, max: 90 + Math.random() * 60,
                emoji: opts.emoji && Math.random() < .25 ? opts.emoji : null
            });
        }
        if (!fx.running) { fx.running = true; requestAnimationFrame(fxTick); }
    }
    function fxTick() {
        var c = fx.ctx;
        c.clearRect(0, 0, innerWidth, innerHeight);
        fx.parts = fx.parts.filter(function (p) { return p.life < p.max && p.y < innerHeight + 40; });
        fx.parts.forEach(function (p) {
            p.life++; p.vy += .25; p.vx *= .99; p.x += p.vx; p.y += p.vy; p.rot += p.vr;
            var alpha = 1 - p.life / p.max;
            c.save(); c.globalAlpha = Math.max(alpha, 0); c.translate(p.x, p.y); c.rotate(p.rot);
            if (p.emoji) { c.font = '22px serif'; c.fillText(p.emoji, -11, 8); }
            else { c.fillStyle = p.color; c.fillRect(-p.w / 2, -p.h / 2, p.w, p.h * Math.cos(p.life / 6)); }
            c.restore();
        });
        if (fx.parts.length) requestAnimationFrame(fxTick);
        else { fx.running = false; c.clearRect(0, 0, innerWidth, innerHeight); }
    }
    function confettiFrom(el, opts) {
        var r = el.getBoundingClientRect();
        confetti(r.left + r.width / 2, r.top + r.height / 2, opts);
    }

    // ------------------------------------------------------------ floating emojis
    function emojiBurst(el, emojis, n) {
        if (reduceMotion) return;
        var r = el.getBoundingClientRect();
        for (var i = 0; i < (n || 6); i++) {
            var e = document.createElement('div');
            e.className = 'floating-emoji';
            e.textContent = emojis[Math.floor(Math.random() * emojis.length)];
            e.style.left = (r.left + r.width / 2 - 14 + (Math.random() - .5) * r.width * .6) + 'px';
            e.style.top = (r.top - 10) + 'px';
            e.style.setProperty('--dx', ((Math.random() - .5) * 140) + 'px');
            e.style.setProperty('--rot', ((Math.random() - .5) * 80) + 'deg');
            e.style.animationDelay = (i * 60) + 'ms';
            document.body.appendChild(e);
            setTimeout(function (node) { node.remove(); }.bind(null, e), 2000 + i * 60);
        }
    }

    // ------------------------------------------------------------ laugh button (saved on the server)
    function laughedSet() {
        try { return JSON.parse(store('jw-laughed') || '[]'); } catch (e) { return []; }
    }
    function initLaughs() {
        var done = laughedSet();
        $$('[data-laugh]').forEach(function (btn) {
            var id = btn.getAttribute('data-laugh');
            if (done.indexOf(id) >= 0) btn.classList.add('laughed');
            btn.addEventListener('click', function (ev) {
                ev.preventDefault(); ev.stopPropagation();
                emojiBurst(btn, ['😂', '🤣', '😆', '😹', '💥'], 7);
                var list = laughedSet();
                if (list.indexOf(id) >= 0) return;          // one laugh per joke per browser
                list.push(id); store('jw-laughed', JSON.stringify(list));
                $$('[data-laugh="' + id + '"]').forEach(function (b) { b.classList.add('laughed'); });

                var tokenInput = $('#af-token input[name="__RequestVerificationToken"]');
                fetch('/Jokes/Laugh/' + encodeURIComponent(id), {
                    method: 'POST',
                    headers: { 'RequestVerificationToken': tokenInput ? tokenInput.value : '' },
                    credentials: 'same-origin'
                }).then(function (r) { return r.ok ? r.json() : null; })
                  .then(function (data) {
                      if (!data) return;
                      $$('[data-laugh-count="' + id + '"]').forEach(function (c) {
                          c.textContent = data.likes;
                          c.classList.remove('bump'); void c.offsetWidth; c.classList.add('bump');
                      });
                  }).catch(function () { });
            });
        });
    }

    // ------------------------------------------------------------ reveal answers on cards
    function initCardReveals() {
        $$('[data-reveal]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var card = btn.closest('.jw-card');
                var ans = card && $('.jw-answer', card);
                if (!ans) return;
                var showing = !ans.hidden;
                ans.hidden = showing;
                btn.textContent = showing ? (card.classList.contains('is-riddle') ? '🤔 Reveal answer' : '🥁 Punchline') : '🙈 Hide';
                if (!showing) confettiFrom(btn, { count: 40, spread: Math.PI / 1.5, angle: -Math.PI / 2 });
            });
        });
    }

    // ------------------------------------------------------------ flip cards
    function initFlips() {
        $$('[data-flip]').forEach(function (card) {
            var flip = function (ev) {
                if (ev && ev.target.closest('a, button')) return;
                card.classList.toggle('flipped');
                if (card.classList.contains('flipped') && !card.hasAttribute('data-no-confetti')) {
                    confettiFrom(card, { count: 90, emoji: card.classList.contains('is-riddle') ? '💡' : '😂' });
                }
            };
            card.addEventListener('click', flip);
            card.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); flip(); }
            });
        });
    }

    // ------------------------------------------------------------ 3D tilt on hover
    function initTilt() {
        if (reduceMotion || !window.matchMedia('(hover: hover)').matches) return;
        $$('.tilt').forEach(function (card) {
            card.addEventListener('mousemove', function (e) {
                var r = card.getBoundingClientRect();
                var px = (e.clientX - r.left) / r.width - .5, py = (e.clientY - r.top) / r.height - .5;
                card.style.transform = 'perspective(900px) rotateX(' + (-py * 7) + 'deg) rotateY(' + (px * 9) + 'deg) translateY(-4px)';
            });
            card.addEventListener('mouseleave', function () { card.style.transform = ''; });
        });
    }

    // ------------------------------------------------------------ animated counters + live polling
    function animateNumber(el, to) {
        var from = parseInt(el.textContent.replace(/\D/g, ''), 10) || 0;
        if (reduceMotion || from === to) { el.textContent = to.toLocaleString(); return; }
        var start = performance.now(), dur = 1200;
        (function step(now) {
            var t = Math.min((now - start) / dur, 1), eased = 1 - Math.pow(1 - t, 3);
            el.textContent = Math.round(from + (to - from) * eased).toLocaleString();
            if (t < 1) requestAnimationFrame(step);
        })(start);
    }
    function initStats() {
        var row = $('[data-stats-url]');
        if (!row) return;
        var nums = $$('[data-stat]', row);
        // count up when visible
        var started = false;
        var go = function () {
            if (started) return; started = true;
            nums.forEach(function (n) { animateNumber(n, parseInt(n.getAttribute('data-value'), 10) || 0); });
        };
        if ('IntersectionObserver' in window) {
            new IntersectionObserver(function (entries, obs) {
                if (entries[0].isIntersecting) { go(); obs.disconnect(); }
            }).observe(row);
        } else go();

        // poll for live updates every 10 seconds while the tab is visible
        var url = row.getAttribute('data-stats-url');
        setInterval(function () {
            if (document.hidden) return;
            fetch(url, { cache: 'no-store' }).then(function (r) { return r.ok ? r.json() : null; }).then(function (s) {
                if (!s) return;
                nums.forEach(function (n) {
                    var key = n.getAttribute('data-stat');
                    var val = s[key];
                    if (typeof val !== 'number') return;
                    var old = parseInt(n.getAttribute('data-value'), 10) || 0;
                    if (val !== old) {
                        n.setAttribute('data-value', val);
                        animateNumber(n, val);
                        n.classList.remove('bump'); void n.offsetWidth; n.classList.add('bump');
                    }
                });
            }).catch(function () { });
        }, 10000);
    }

    // ------------------------------------------------------------ hero: floating emoji particles that dodge the mouse
    function initHero() {
        var canvas = $('#hero-particles');
        if (!canvas) return;
        var ctx = canvas.getContext('2d');
        var emojis = ['😂', '🤣', '🧩', '❓', '💡', '🎉', '😜', '🤔', '✨', '🥁'];
        var parts = [], mouse = { x: -999, y: -999 }, w = 0, h = 0;

        function size() {
            var dpr = window.devicePixelRatio || 1;
            w = canvas.clientWidth; h = canvas.clientHeight;
            canvas.width = w * dpr; canvas.height = h * dpr;
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            var target = Math.round(Math.min(34, Math.max(14, w / 38)));
            while (parts.length < target) parts.push(spawn(true));
            parts.length = target;
        }
        function spawn(anywhere) {
            return {
                x: Math.random() * w, y: anywhere ? Math.random() * h : h + 30,
                vy: -(.25 + Math.random() * .6), vx: (Math.random() - .5) * .3,
                size: 16 + Math.random() * 22, rot: Math.random() * 6.28, vr: (Math.random() - .5) * .01,
                e: emojis[Math.floor(Math.random() * emojis.length)], a: .25 + Math.random() * .45
            };
        }
        canvas.parentElement.addEventListener('mousemove', function (e) {
            var r = canvas.getBoundingClientRect(); mouse.x = e.clientX - r.left; mouse.y = e.clientY - r.top;
        });
        canvas.parentElement.addEventListener('mouseleave', function () { mouse.x = mouse.y = -999; });
        canvas.parentElement.addEventListener('click', function (e) {
            if (e.target.closest('a, button')) return;
            confetti(e.clientX, e.clientY, { count: 50, emoji: '😂' });
        });
        addEventListener('resize', size);
        size();

        function draw() {
            ctx.clearRect(0, 0, w, h);
            parts.forEach(function (p, i) {
                var dx = p.x - mouse.x, dy = p.y - mouse.y, d2 = dx * dx + dy * dy;
                if (d2 < 12000) { var f = (12000 - d2) / 12000; p.x += dx * f * .08; p.y += dy * f * .08; }
                p.x += p.vx; p.y += p.vy; p.rot += p.vr;
                if (p.y < -40 || p.x < -40 || p.x > w + 40) parts[i] = spawn(false);
                ctx.save(); ctx.globalAlpha = p.a; ctx.translate(p.x, p.y); ctx.rotate(Math.sin(p.rot) * .4);
                ctx.font = p.size + 'px serif'; ctx.textAlign = 'center'; ctx.fillText(p.e, 0, 0); ctx.restore();
            });
            if (!document.hidden) requestAnimationFrame(draw); else setTimeout(function () { requestAnimationFrame(draw); }, 500);
        }
        if (!reduceMotion) draw();
    }

    // ------------------------------------------------------------ typewriter word rotator
    function initRotator() {
        var el = $('.rotator');
        if (!el || reduceMotion) return;
        var words = (el.getAttribute('data-words') || '').split('|').filter(Boolean);
        var wi = 0, ci = words[0].length, deleting = true;
        setTimeout(function tick() {
            var word = words[wi];
            if (deleting) {
                ci--; el.textContent = word.slice(0, ci);
                if (ci === 0) { deleting = false; wi = (wi + 1) % words.length; }
                setTimeout(tick, 60);
            } else {
                var next = words[wi]; ci++; el.textContent = next.slice(0, ci);
                if (ci === next.length) { deleting = true; setTimeout(tick, 1800); }
                else setTimeout(tick, 90);
            }
        }, 2200);
    }

    // ------------------------------------------------------------ details page: reveal, guess, share, shortcuts
    function initDetails() {
        var stage = $('#stage');
        if (!stage) return;
        var answer = $('#stage-answer'), btn = $('#reveal-btn');
        var isRiddle = stage.classList.contains('is-riddle');
        var reveal = function () {
            if (!answer.hidden) return;
            answer.hidden = false;
            btn.textContent = isRiddle ? '💡 There it is!' : '🥁 Ba-dum-tss!';
            btn.disabled = true;
            confettiFrom(answer, { count: 140, emoji: isRiddle ? '💡' : '😂' });
        };
        btn && btn.addEventListener('click', reveal);

        var form = $('#guess-form');
        if (form) {
            var norm = function (s) {
                return (s || '').toLowerCase().replace(/[^a-z0-9 ]/g, ' ')
                    .replace(/\b(a|an|the|your|my|of|it|is)\b/g, ' ').replace(/\s+/g, ' ').trim();
            };
            var target = norm(form.getAttribute('data-answer'));
            var tries = 0;
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                var input = $('#guess-input', form), fb = $('.guess-feedback', form);
                var g = norm(input.value);
                if (!g) return;
                tries++;
                var hit = g === target || (g.length > 2 && (target.indexOf(g) >= 0 || g.indexOf(target) >= 0));
                if (hit) {
                    fb.className = 'guess-feedback ok';
                    fb.textContent = '🎉 Yes! You got it' + (tries === 1 ? ' first try!' : ' in ' + tries + ' tries!');
                    reveal();
                } else {
                    fb.className = 'guess-feedback nope';
                    fb.textContent = ['Not quite, try again!', 'Ooh, close? Have another go.', 'Nope! Think sideways…', 'Still no. Press "Show the answer" if you give up 😉'][Math.min(tries - 1, 3)];
                    input.classList.remove('shake'); void input.offsetWidth; input.classList.add('shake');
                }
            });
        }

        var share = $('#share-btn');
        share && share.addEventListener('click', function () {
            var data = { title: "Andrea's Jokes & Riddles", text: share.getAttribute('data-share-text'), url: location.href };
            if (navigator.share) { navigator.share(data).catch(function () { }); return; }
            if (navigator.clipboard) {
                navigator.clipboard.writeText(data.text + ' ' + data.url).then(function () {
                    share.textContent = '✅ Link copied';
                    setTimeout(function () { share.textContent = '🔗 Share'; }, 2000);
                });
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.target.closest('input, textarea, select') || e.ctrlKey || e.metaKey || e.altKey) return;
            if (e.key === ' ') { e.preventDefault(); reveal(); }
            else if (e.key === 'ArrowLeft' && $('#prev-link')) location.href = $('#prev-link').href;
            else if (e.key === 'ArrowRight' && $('#next-link')) location.href = $('#next-link').href;
            else if (e.key.toLowerCase() === 'r' && $('#random-link')) location.href = $('#random-link').href;
        });
    }

    // ------------------------------------------------------------ create/edit: live preview + counters
    function initForm() {
        var q = $('[data-preview="q"]'), a = $('[data-preview="a"]');
        if (!q || !a) return;
        var outQ = $('[data-preview-out="q"]'), outA = $('[data-preview-out="a"]'), badge = $('[data-preview-badge]');
        var card = $('.preview-card');
        var update = function () {
            outQ.textContent = q.value.trim() || 'Your question appears here…';
            outA.textContent = a.value.trim() || '…and the answer here.';
            $$('[data-counter-for]').forEach(function (c) {
                var field = document.getElementById(c.getAttribute('data-counter-for'));
                if (field) c.textContent = field.value.length + '/500';
            });
            var riddle = $('input[name="Category"][value="Riddle"]');
            var isRiddle = riddle && riddle.checked;
            badge.textContent = isRiddle ? '🧩 Riddle' : '😂 Joke';
            card.classList.toggle('is-riddle', !!isRiddle);
            card.classList.toggle('is-joke', !isRiddle);
        };
        [q, a].forEach(function (el) { el.addEventListener('input', update); });
        a.addEventListener('focus', function () { card.classList.add('flipped'); });
        q.addEventListener('focus', function () { card.classList.remove('flipped'); });
        $$('input[name="Category"]').forEach(function (r) { r.addEventListener('change', update); });
        update();
    }

    // ------------------------------------------------------------ toast after create/edit/delete
    function initToast() {
        var t = $('[data-toast]');
        if (!t) return;
        if (/live|🎉/.test(t.textContent)) setTimeout(function () { confetti(innerWidth / 2, innerHeight - 60, { count: 160, angle: -Math.PI / 2, spread: Math.PI / 1.4 }); }, 250);
        setTimeout(function () { t.remove(); }, 5000);
    }

    document.addEventListener('DOMContentLoaded', function () {
        initTheme(); initLaughs(); initCardReveals(); initFlips(); initTilt();
        initStats(); initHero(); initRotator(); initDetails(); initForm(); initToast();
    });
})();
