(function () {
    'use strict';
    var modal = document.getElementById('bookApptModal');
    var sel = document.getElementById('doctorSelect');
    if (!modal || !sel) return;

    function setSingleOption(text) {
        sel.replaceChildren(new Option(text, ''));
    }

    modal.addEventListener('show.bs.modal', function () {
        if (sel.options.length > 1) return; // already loaded
        fetch('/portal/doctors')
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            })
            .then(function (doctors) {
                setSingleOption('Select a doctor…');
                doctors.forEach(function (d) { sel.appendChild(new Option(d.name, d.id)); });
            })
            .catch(function () { setSingleOption('Failed to load doctors'); });
    });
})();
