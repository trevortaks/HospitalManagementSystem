// Delete-patient confirmation modal (Patients index and edit pages).
// Trigger: <button data-delete-patient="{id}" data-patient-name="{name}">
(function () {
    'use strict';
    var modalEl = document.getElementById('deleteModal');
    var confirmBtn = document.getElementById('confirmDeleteBtn');
    if (!modalEl || !confirmBtn) return;

    var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    var pendingId = null;

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-delete-patient]');
        if (!btn) return;
        pendingId = btn.dataset.deletePatient;
        document.getElementById('deletePatientName').textContent = btn.dataset.patientName || '';
        modal.show();
    });

    confirmBtn.addEventListener('click', function () {
        if (!pendingId) return;
        var form = document.getElementById('deleteForm');
        form.action = '/patients/' + encodeURIComponent(pendingId) + '/delete';
        modal.hide();
        form.submit();
    });
})();
