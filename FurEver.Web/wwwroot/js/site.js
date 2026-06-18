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
