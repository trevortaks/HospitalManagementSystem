(function () {
    'use strict';
    var modal = document.getElementById('dischargeModal');
    if (!modal) return;
    modal.addEventListener('show.bs.modal', function (e) {
        var btn = e.relatedTarget;
        if (!btn) return;
        document.getElementById('dischargeAllocationId').value = btn.dataset.allocationId;
        document.getElementById('dischargePatientName').textContent = btn.dataset.patient || '';
        document.getElementById('dischargeForm').action = '/beds/discharge/' + encodeURIComponent(btn.dataset.allocationId);
    });
})();
