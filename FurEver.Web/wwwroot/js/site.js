// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Logout confirmation modal.
(function () {
    var dialog = document.getElementById('logout-dialog');
    if (!dialog) return;

    function open() {
        if (typeof dialog.showModal === 'function') {
            dialog.showModal();
        } else {
            dialog.setAttribute('open', '');
        }
    }

    function close() {
        if (typeof dialog.close === 'function') {
            dialog.close();
        } else {
            dialog.removeAttribute('open');
        }
    }

    document.querySelectorAll('[data-logout-open]').forEach(function (btn) {
        btn.addEventListener('click', open);
    });

    dialog.querySelectorAll('[data-logout-cancel]').forEach(function (btn) {
        btn.addEventListener('click', close);
    });

    // Click on the backdrop (outside the card) dismisses the dialog.
    dialog.addEventListener('click', function (e) {
        if (e.target === dialog) close();
    });
})();

// Generic dialog helpers.
function openDialog(dialog) {
    if (!dialog) return;
    if (typeof dialog.showModal === 'function') dialog.showModal();
    else dialog.setAttribute('open', '');
}
function closeDialog(dialog) {
    if (!dialog) return;
    if (typeof dialog.close === 'function') dialog.close();
    else dialog.removeAttribute('open');
}

// Confirmation modal — replaces native confirm() for forms marked
// with data-confirm (e.g. delete buttons).
(function () {
    var dialog = document.getElementById('confirm-dialog');
    if (!dialog) return;

    var titleEl = dialog.querySelector('[data-confirm-title]');
    var bodyEl = dialog.querySelector('[data-confirm-body]');
    var okBtn = dialog.querySelector('[data-confirm-ok]');
    var cancelBtn = dialog.querySelector('[data-confirm-cancel]');
    var pendingForm = null;

    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form.hasAttribute || !form.hasAttribute('data-confirm')) return;
        if (form.dataset.confirmed === 'true') {
            form.removeAttribute('data-confirmed');
            return; // allow the real submit through
        }
        e.preventDefault();
        pendingForm = form;
        titleEl.textContent = form.getAttribute('data-confirm-title') || 'Are you sure?';
        bodyEl.textContent = form.getAttribute('data-confirm') || "This action can't be undone.";
        okBtn.textContent = form.getAttribute('data-confirm-ok') || 'Delete';
        openDialog(dialog);
    });

    okBtn.addEventListener('click', function () {
        if (!pendingForm) return;
        var form = pendingForm;
        pendingForm = null;
        form.dataset.confirmed = 'true';
        closeDialog(dialog);
        if (typeof form.requestSubmit === 'function') form.requestSubmit();
        else form.submit();
    });

    cancelBtn.addEventListener('click', function () { pendingForm = null; closeDialog(dialog); });
    dialog.addEventListener('click', function (e) {
        if (e.target === dialog) { pendingForm = null; closeDialog(dialog); }
    });
})();

// Admin add/edit modal — fetches a form fragment and shows it in a dialog
// instead of navigating to a separate page.
(function () {
    var dialog = document.getElementById('admin-modal');
    if (!dialog) return;
    var content = dialog.querySelector('[data-modal-content]');

    function load(url) {
        content.innerHTML = '<p class="modal-loading">Loading…</p>';
        openDialog(dialog);
        fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                content.innerHTML = html;
                if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
                    window.jQuery.validator.unobtrusive.parse(content);
                }
                var first = content.querySelector('input:not([type=hidden]):not([type=file]), select, textarea');
                if (first) first.focus();
            })
            .catch(function () {
                content.innerHTML = '<p class="modal-loading">Couldn\u2019t load the form. Please try again.</p>';
            });
    }

    document.addEventListener('click', function (e) {
        var trigger = e.target.closest ? e.target.closest('[data-modal-url]') : null;
        if (!trigger) {
            if (e.target === dialog || (e.target.closest && e.target.closest('[data-modal-close]'))) {
                closeDialog(dialog);
            }
            return;
        }
        e.preventDefault();
        load(trigger.getAttribute('data-modal-url') || trigger.getAttribute('href'));
    });
})();

// Live filtering for the "Find a pet" page. Selects auto-submit the GET form;
// the search box filters the already-rendered cards instantly.
(function () {
    var form = document.querySelector('[data-pet-filter]');
    if (!form) return;

    form.querySelectorAll('select').forEach(function (sel) {
        sel.addEventListener('change', function () {
            if (typeof form.requestSubmit === 'function') form.requestSubmit();
            else form.submit();
        });
    });

    var input = form.querySelector('[data-live-search]');
    var grid = document.querySelector('[data-pet-results]');
    if (!input || !grid) return;

    var cards = Array.prototype.slice.call(grid.querySelectorAll('.pet-card'));
    var countEl = document.querySelector('[data-result-count]');
    var emptyEl = document.querySelector('[data-no-match]');

    function apply() {
        var q = input.value.trim().toLowerCase();
        var shown = 0;
        cards.forEach(function (card) {
            var match = q === '' || card.textContent.toLowerCase().indexOf(q) !== -1;
            card.classList.toggle('is-hidden', !match);
            if (match) shown++;
        });
        if (countEl) countEl.textContent = shown + ' pet' + (shown === 1 ? '' : 's');
        if (emptyEl) emptyEl.classList.toggle('is-hidden', shown !== 0 || cards.length === 0);
        grid.classList.toggle('is-hidden', shown === 0 && cards.length > 0);
    }

    input.addEventListener('input', apply);
})();

// Generic dialog triggers — any element with data-dialog-open="some-id"
// opens that <dialog>; [data-dialog-close] or a backdrop click closes it.
(function () {
    document.addEventListener('click', function (e) {
        var opener = e.target.closest ? e.target.closest('[data-dialog-open]') : null;
        if (opener) {
            e.preventDefault();
            openDialog(document.getElementById(opener.getAttribute('data-dialog-open')));
            return;
        }
        var closer = e.target.closest ? e.target.closest('[data-dialog-close]') : null;
        if (closer) {
            e.preventDefault();
            closeDialog(closer.closest('dialog'));
        }
    });

    document.querySelectorAll('dialog[data-dialog]').forEach(function (dialog) {
        dialog.addEventListener('click', function (e) {
            if (e.target === dialog) closeDialog(dialog);
        });
    });
})();

// Shrink large stat figures (e.g. revenue) so they never overflow their card.
(function () {
    var values = Array.prototype.slice.call(document.querySelectorAll('.stat-value'));
    if (!values.length) return;

    function fit(el) {
        var parent = el.parentElement;
        if (!parent) return;
        el.style.fontSize = '';
        var size = parseFloat(window.getComputedStyle(el).fontSize);
        var min = 14;
        while (el.scrollWidth > parent.clientWidth && size > min) {
            size -= 1;
            el.style.fontSize = size + 'px';
        }
    }

    function run() { values.forEach(fit); }

    run();
    window.addEventListener('resize', run);
    if (document.fonts && document.fonts.ready) {
        document.fonts.ready.then(run);
    }
})();
