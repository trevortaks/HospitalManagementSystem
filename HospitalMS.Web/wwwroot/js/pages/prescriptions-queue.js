(function () {
    'use strict';
    var modal = document.getElementById('dispenseModal');
    if (!modal) return;
    modal.addEventListener('show.bs.modal', function (e) {
        var btn = e.relatedTarget;
        if (!btn) return;
        document.getElementById('modalRxId').value = btn.dataset.rxId;
        document.getElementById('modalMedLabel').textContent = btn.dataset.rxLabel || '';
        document.getElementById('modalPatient').textContent = btn.dataset.patient || '';
        document.getElementById('dispenseForm').action = '/Prescriptions/Dispense/' + encodeURIComponent(btn.dataset.rxId);
    });
})();
