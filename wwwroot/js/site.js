// Conference Delegate Management System JS

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips if Bootstrap is loaded
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Initialize popovers if Bootstrap is loaded
    if (typeof bootstrap !== 'undefined' && bootstrap.Popover) {
        var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.map(function (popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl);
        });
    }

    // Handle datetime-local inputs for browsers that don't support it
    var dateTimeInputs = document.querySelectorAll('input[type="datetime-local"]');
    if (dateTimeInputs.length > 0) {
        dateTimeInputs.forEach(function (input) {
            // If the browser doesn't support datetime-local
            if (input.type !== 'datetime-local') {
                // Create a fallback UI here if needed
                console.log('Browser does not support datetime-local input type');
            }
        });
    }

    // Confirmation dialogs for delete buttons
    var deleteButtons = document.querySelectorAll('form[action*="Delete"] button[type="submit"]');
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    // Disable form submissions if there are invalid fields
    var forms = document.querySelectorAll('.needs-validation');
    Array.prototype.slice.call(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // Responsive tables handling
    var tables = document.querySelectorAll('.table-responsive table');
    if (window.innerWidth < 768) {
        tables.forEach(function (table) {
            if (!table.classList.contains('table-sm')) {
                table.classList.add('table-sm');
            }
        });
    }

    // Handle search box clear button
    var searchClearButtons = document.querySelectorAll('.search-clear-btn');
    searchClearButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            var input = button.previousElementSibling;
            if (input && input.tagName === 'INPUT') {
                input.value = '';
                input.focus();
            }
        });
    });

    // Add animation for alert dismissal
    var alerts = document.querySelectorAll('.alert');
    alerts.forEach(function (alert) {
        if (alert.querySelector('.btn-close')) {
            alert.querySelector('.btn-close').addEventListener('click', function () {
                alert.classList.add('fade');
                setTimeout(function () {
                    alert.style.display = 'none';
                }, 150);
            });
        }
    });

    // Function to format date time inputs to local format
    function formatDateTimeDisplay() {
        document.querySelectorAll('[data-format-date]').forEach(function (element) {
            try {
                var date = new Date(element.textContent.trim());
                if (!isNaN(date)) {
                    element.textContent = date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
                }
            } catch (e) {
                console.error('Error formatting date:', e);
            }
        });
    }

    formatDateTimeDisplay();

    // Print registration details
    var printButtons = document.querySelectorAll('.print-registration');
    printButtons.forEach(function (button) {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            window.print();
        });
    });
});