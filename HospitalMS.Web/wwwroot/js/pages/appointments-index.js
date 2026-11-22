(function () {
    'use strict';
    // ── Search / status filter ────────────────────────────────────────────
    var rows = document.querySelectorAll('.appt-row');
    var searchInput = document.getElementById('apptSearch');
    var searchClear = document.getElementById('searchClear');
    var noResults = document.getElementById('noResults');
    var noResultsTerm = document.getElementById('noResultsTerm');
    var footerCount = document.getElementById('footerCount');
    var statusSel = document.getElementById('statusFilter');

    function filterTable() {
        var q = (searchInput ? searchInput.value : '').trim().toLowerCase();
        var s = statusSel ? statusSel.value.toLowerCase() : '';
        if (searchClear) searchClear.classList.toggle('d-none', q === '');
        var visible = 0;
        rows.forEach(function (row) {
            var matchQ = !q || row.dataset.patient.includes(q) || row.dataset.doctor.includes(q);
            var matchS = !s || row.dataset.status === s;
            var show = matchQ && matchS;
            row.classList.toggle('d-none', !show);
            if (show) visible++;
        });
        if (noResults) noResults.classList.toggle('d-none', visible > 0);
        if (noResultsTerm) noResultsTerm.textContent = '"' + (searchInput ? searchInput.value.trim() : '') + '"';
        if (footerCount) footerCount.innerHTML = 'Showing <strong>' + visible + '</strong> appointment' + (visible === 1 ? '' : 's');
    }

    if (searchInput) searchInput.addEventListener('input', filterTable);
    if (searchClear) searchClear.addEventListener('click', function () {
        if (!searchInput) return;
        searchInput.value = '';
        filterTable();
        searchInput.focus();
    });
    if (statusSel) statusSel.addEventListener('change', filterTable);

    // ── Cancel modal ──────────────────────────────────────────────────────
    // Trigger: <button data-cancel-appt="{id}" data-patient-name="{name}">
    var modalEl = document.getElementById('cancelModal');
    if (!modalEl) return;
    var cancelModal = bootstrap.Modal.getOrCreateInstance(modalEl);
    var pendingCancelId = null;

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-cancel-appt]');
        if (!btn) return;
        pendingCancelId = btn.dataset.cancelAppt;
        document.getElementById('cancelPatientName').textContent = btn.dataset.patientName || '';
        cancelModal.show();
    });

    document.getElementById('confirmCancelBtn').addEventListener('click', function () {
        if (!pendingCancelId) return;
        var form = document.getElementById('cancelForm');
        form.action = '/appointments/' + encodeURIComponent(pendingCancelId) + '/cancel';

        var input = form.querySelector('input[name="reason"]');
        if (!input) {
            input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'reason';
            form.appendChild(input);
        }
        input.value = document.getElementById('cancelReason').value;

        cancelModal.hide();
        form.submit();
    });
})();
