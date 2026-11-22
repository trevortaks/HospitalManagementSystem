// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// The Content-Security-Policy (default-src 'self') blocks inline <script> blocks and
// on* attributes, so page behaviour lives here and is wired up through data attributes.

// <button data-confirm="Are you sure?"> — asks before submitting / following the click.
document.addEventListener('click', e => {
    const el = e.target.closest('[data-confirm]');
    if (el && !window.confirm(el.dataset.confirm)) {
        e.preventDefault();
        e.stopImmediatePropagation();
    }
}, true);

// Insurance claim status forms (Billing/InvoiceDetail): show the approved-amount field
// only for Approved/Paid and the rejection reason only for Rejected. Disabled inputs
// aren't submitted, so a rejection never carries an amount.
document.querySelectorAll('.claim-status-form').forEach(form => {
    const select = form.querySelector('.claim-status-select');
    const fields = [
        [form.querySelector('.claim-field--amount'), s => s === 'Approved' || s === 'Paid'],
        [form.querySelector('.claim-field--reason'), s => s === 'Rejected']
    ];
    const sync = () => fields.forEach(([field, applies]) => {
        const on = applies(select.value);
        const input = field.querySelector('input');
        field.hidden = !on;
        input.disabled = !on;
        input.required = on;
    });
    select.addEventListener('change', sync);
    sync();
});
