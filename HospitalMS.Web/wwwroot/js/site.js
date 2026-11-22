// Site-wide behaviour, wired up with data attributes so the pages work under a
// Content-Security-Policy that forbids inline scripts and on* handler attributes.
//
//   data-confirm="Message"         click is cancelled unless the user confirms
//   data-action="toggle-sidebar"   collapse/expand the sidebar (off-canvas on mobile)
//   data-action="close-sidebar"    close the mobile sidebar
//   data-action="print"            window.print()
//   data-autosubmit                submit the element's form when its value changes
//   data-nav-param="name"          on change, navigate to ?name=<value>
//   .toast                         Bootstrap toasts are shown on page load
(function () {
    'use strict';

    function onReady(fn) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', fn);
        } else {
            fn();
        }
    }

    // ── Confirmation prompts ──────────────────────────────────────────────
    // Capture phase so the prompt runs before any other click handler.
    document.addEventListener('click', function (e) {
        var el = e.target.closest('[data-confirm]');
        if (el && !window.confirm(el.dataset.confirm)) {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    }, true);

    // ── Sidebar ───────────────────────────────────────────────────────────
    var SIDEBAR_KEY = 'sidebarCollapsed';

    function toggleSidebar() {
        var body = document.body;
        if (window.innerWidth < 768) {
            body.classList.toggle('sidebar-open');
        } else {
            body.classList.toggle('sidebar-collapsed');
            try { localStorage.setItem(SIDEBAR_KEY, body.classList.contains('sidebar-collapsed')); } catch (_) { }
        }
    }

    function closeSidebar() {
        document.body.classList.remove('sidebar-open');
    }

    // site.js is loaded at the end of <body>, so restoring here avoids a flash.
    try {
        if (document.body && document.body.classList.contains('dash-body') &&
            window.innerWidth >= 768 && localStorage.getItem(SIDEBAR_KEY) === 'true') {
            document.body.classList.add('sidebar-collapsed');
        }
    } catch (_) { }

    // ── Generic actions ───────────────────────────────────────────────────
    var actions = {
        'toggle-sidebar': toggleSidebar,
        'close-sidebar': closeSidebar,
        'print': function () { window.print(); }
    };

    document.addEventListener('click', function (e) {
        var el = e.target.closest('[data-action]');
        if (!el) return;
        var handler = actions[el.dataset.action];
        if (handler) {
            e.preventDefault();
            handler(el, e);
        }
    });

    document.addEventListener('change', function (e) {
        var el = e.target;
        if (el.matches('[data-autosubmit]') && el.form) {
            el.form.requestSubmit ? el.form.requestSubmit() : el.form.submit();
            return;
        }
        if (el.matches('[data-nav-param]')) {
            var url = new URL(window.location.href);
            if (el.value) url.searchParams.set(el.dataset.navParam, el.value);
            else url.searchParams.delete(el.dataset.navParam);
            window.location.assign(url.toString());
        }
    });

    // ── Toasts (TempData success / error messages) ────────────────────────
    onReady(function () {
        if (!window.bootstrap) return;
        document.querySelectorAll('.toast').forEach(function (el) {
            window.bootstrap.Toast.getOrCreateInstance(el).show();
        });
    });

    // Small helper for page scripts.
    window.HospitalMS = { onReady: onReady };
})();
