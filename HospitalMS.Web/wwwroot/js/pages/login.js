(function () {
    'use strict';
    var EYE = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>';
    var EYE_OFF = '<line x1="1" y1="1" x2="23" y2="23"/><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/><path d="M6.53 6.53A10 10 0 0 0 5 10"/>';

    var btn = document.getElementById('passwordToggle');
    var input = document.getElementById('passwordInput');
    if (!btn || !input) return;

    btn.addEventListener('click', function () {
        var isHidden = input.type === 'password';
        input.type = isHidden ? 'text' : 'password';
        btn.querySelector('svg').innerHTML = isHidden ? EYE_OFF : EYE;
    });
})();
