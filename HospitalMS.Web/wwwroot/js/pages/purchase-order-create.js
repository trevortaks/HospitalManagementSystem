(function () {
    'use strict';
    var body = document.getElementById('linesBody');
    var addBtn = document.getElementById('addLineBtn');
    if (!body || !addBtn) return;

    // Keep a pristine copy of the server-rendered first row as the template
    // for new rows (it already contains every item <option>).
    var firstRow = body.querySelector('.line-row');
    if (!firstRow) return;
    var template = firstRow.cloneNode(true);

    function syncCost(select) {
        var opt = select.selectedOptions[0];
        var costInput = select.closest('tr').querySelector('input[name="UnitCosts"]');
        if (opt && opt.dataset.cost && costInput) costInput.value = opt.dataset.cost;
    }

    addBtn.addEventListener('click', function () {
        body.appendChild(template.cloneNode(true));
    });

    body.addEventListener('change', function (e) {
        if (e.target.matches('select[name="ItemIds"]')) syncCost(e.target);
    });

    body.addEventListener('click', function (e) {
        var btn = e.target.closest('.remove-line');
        if (btn) btn.closest('tr').remove();
    });
})();
