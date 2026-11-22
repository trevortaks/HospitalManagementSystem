// Pre-fills the "Record vitals" modal (Appointments index, Nurse dashboard)
// from the data-* attributes of the button that opened it.
(function () {
    'use strict';
    var modal = document.getElementById('vitalsModal');
    if (!modal) return;

    var fields = {
        fHeight: 'existingHeight', fWeight: 'existingWeight', fTemp: 'existingTemp',
        fSbp: 'existingSbp', fDbp: 'existingDbp', fHr: 'existingHr',
        fRr: 'existingRr', fSpo2: 'existingSpo2', fNotes: 'existingNotes'
    };

    modal.addEventListener('show.bs.modal', function (e) {
        var btn = e.relatedTarget;
        if (!btn) return;
        var form = document.getElementById('vitalsForm');
        document.getElementById('vitalsPatientName').textContent = btn.dataset.patientName || '';
        form.action = '/appointments/' + encodeURIComponent(btn.dataset.apptId) + '/vitals';
        form.querySelector('input[name="returnUrl"]').value = btn.dataset.returnUrl || modal.dataset.defaultReturnUrl || '/';
        Object.keys(fields).forEach(function (id) {
            var input = document.getElementById(id);
            if (input) input.value = btn.dataset[fields[id]] || '';
        });
    });
})();
