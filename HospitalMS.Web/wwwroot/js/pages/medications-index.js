(function () {
    'use strict';
    var rows = document.querySelectorAll('.med-row');
    var searchInput = document.getElementById('medSearch');
    var searchClear = document.getElementById('searchClear');
    var noResults = document.getElementById('noResults');
    var noResultsTerm = document.getElementById('noResultsTerm');
    var footerCount = document.getElementById('footerCount');
    if (!searchInput) return;

    function filter() {
        var q = searchInput.value.trim().toLowerCase();
        if (searchClear) searchClear.classList.toggle('d-none', q === '');
        var visible = 0;
        rows.forEach(function (row) {
            var match = !q || row.dataset.name.includes(q) || row.dataset.brand.includes(q) || row.dataset.form.includes(q);
            row.classList.toggle('d-none', !match);
            if (match) visible++;
        });
        if (noResults) noResults.classList.toggle('d-none', visible > 0);
        if (noResultsTerm) noResultsTerm.textContent = '"' + searchInput.value.trim() + '"';
        if (footerCount) footerCount.innerHTML = 'Showing <strong>' + visible + '</strong> medication' + (visible === 1 ? '' : 's');
    }

    searchInput.addEventListener('input', filter);
    if (searchClear) searchClear.addEventListener('click', function () { searchInput.value = ''; filter(); searchInput.focus(); });
})();
