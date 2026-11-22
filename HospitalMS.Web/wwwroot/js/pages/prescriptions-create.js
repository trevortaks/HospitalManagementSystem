(function () {
    'use strict';
    var medSelect = document.getElementById('medSelect');
    var controlledAlert = document.getElementById('controlledAlert');
    if (!medSelect || !controlledAlert) return;
    medSelect.addEventListener('change', function () {
        var opt = medSelect.selectedOptions[0];
        controlledAlert.classList.toggle('d-none', !opt || opt.dataset.controlled !== 'true');
    });
})();
