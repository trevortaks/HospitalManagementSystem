(function () {
    'use strict';
    var searchInput = document.getElementById('patientSearch');
    var searchClear = document.getElementById('searchClear');
    var resultCount = document.getElementById('searchResultCount');
    var noResults = document.getElementById('noResults');
    var noResultsTerm = document.getElementById('noResultsTerm');
    var footerCount = document.getElementById('footerCount');
    var rows = document.querySelectorAll('.patient-row');
    var total = rows.length;
    if (!searchInput) return;

    function plural(n) { return n === 1 ? '' : 's'; }

    function filterTable() {
        var q = searchInput.value.trim().toLowerCase();
        if (searchClear) searchClear.classList.toggle('d-none', q === '');

        var visible = 0;
        rows.forEach(function (row) {
            var match = !q || row.dataset.name.includes(q) || row.dataset.mrn.includes(q) || row.dataset.email.includes(q);
            row.classList.toggle('d-none', !match);
            if (match) visible++;
        });

        var hasQuery = q !== '';
        if (noResults) noResults.classList.toggle('d-none', !hasQuery || visible > 0);
        if (resultCount) resultCount.classList.toggle('d-none', !hasQuery);
        if (hasQuery) {
            if (noResultsTerm) noResultsTerm.textContent = '"' + searchInput.value.trim() + '"';
            if (resultCount) resultCount.textContent = visible === 0 ? 'No results' : visible + ' of ' + total + ' patient' + plural(total);
        }
        if (footerCount) {
            var shown = hasQuery ? visible : total;
            footerCount.innerHTML = 'Showing <strong>' + shown + '</strong> patient' + plural(shown);
        }
    }

    searchInput.addEventListener('input', filterTable);
    if (searchClear) searchClear.addEventListener('click', function () { searchInput.value = ''; filterTable(); searchInput.focus(); });
})();
