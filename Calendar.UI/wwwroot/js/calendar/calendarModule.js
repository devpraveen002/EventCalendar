//const CalendarModule = (function () {
//    // Private variables
//    let addEventUrl = '';

//    // Configuration object
//    const config = {
//        selectors: {
//            calendarCell: '.calendar td',
//            eventModal: '#eventModal',
//            eventForm: '#eventForm',
//            eventDate: '#eventDate',
//            eventTitle: '#eventTitle',
//            eventDescription: '#eventDescription',
//            saveEventBtn: '#saveEvent',
//            closeModalBtn: '[data-bs-dismiss="modal"]'
//        },
//        classes: {
//            invalidFeedback: 'invalid-feedback',
//            isInvalid: 'is-invalid'
//        },
//        messages: {
//            titleRequired: 'Please enter an event title',
//            dateInvalid: 'Please select a valid date',
//            serverError: 'An error occurred while saving the event',
//            networkError: 'Network error occurred. Please try again.'
//        }
//    };

//    // Private functions
//    const handleDateClick = function (e) {
//        const date = $(this).data('date');
//        if (!date) return;

//        const formattedDate = new Date(date).toLocaleDateString();
//        $(config.selectors.eventDate).val(date);
//        $(config.selectors.eventModal).modal('show');

//        // Update modal title to include selected date
//        $('.modal-title').text(`Add Event - ${formattedDate}`);

//        // Clear previous validation states
//        resetFormValidation();
//    };

//    const resetFormValidation = function () {
//        const form = $(config.selectors.eventForm);
//        form.find('.is-invalid').removeClass('is-invalid');
//        form.find('.invalid-feedback').remove();
//    };

//    const showValidationError = function (element, message) {
//        const $element = $(element);
//        $element.addClass(config.classes.isInvalid);

//        if (!$element.next('.' + config.classes.invalidFeedback).length) {
//            $element.after(`<div class="${config.classes.invalidFeedback}">${message}</div>`);
//        }
//    };

//    const handleEventSave = function () {
//        resetFormValidation();

//        const eventData = {
//            date: $(config.selectors.eventDate).val(),
//            title: $(config.selectors.eventTitle).val(),
//            description: $(config.selectors.eventDescription).val()
//        };

//        if (validateEventData(eventData)) {
//            submitEvent(eventData);
//        }
//    };

//    const validateEventData = function (eventData) {
//        let isValid = true;

//        if (!eventData.title.trim()) {
//            showValidationError(config.selectors.eventTitle, config.messages.titleRequired);
//            isValid = false;
//        }

//        if (!eventData.date) {
//            showValidationError(config.selectors.eventDate, config.messages.dateInvalid);
//            isValid = false;
//        }

//        return isValid;
//    };

//    const submitEvent = function (eventData) {
//        // Disable save button and show loading state
//        const $saveBtn = $(config.selectors.saveEventBtn);
//        const originalText = $saveBtn.text();
//        $saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');

//        // Convert local date to UTC
//        const localDate = new Date(eventData.date);
//        const utcDate = new Date(localDate.getTime() - localDate.getTimezoneOffset() * 60000);
//        eventData.date = utcDate.toISOString();

//        $.ajax({
//            url: addEventUrl,
//            type: 'POST',
//            contentType: 'application/json',
//            data: JSON.stringify(eventData),
//            success: function (response) {
//                if (response.success) {
//                    showToast('Success', 'Event saved successfully', 'success');
//                    $(config.selectors.eventModal).modal('hide');
//                    location.reload();
//                } else {
//                    handleError(response.errors);
//                }
//            },
//            error: function (xhr, status, error) {
//                handleError(error);
//            },
//            complete: function () {
//                // Reset save button state
//                $saveBtn.prop('disabled', false).text(originalText);
//            }
//        });
//    };

//    const handleError = function (error) {
//        console.error(error);

//        let errorMessage = typeof error === 'string'
//            ? error
//            : Array.isArray(error)
//                ? error.join(', ')
//                : config.messages.serverError;

//        showToast('Error', errorMessage, 'error');
//    };

//    const showToast = function (title, message, type = 'info') {
//        // You can implement your preferred toast notification here
//        // Example using Bootstrap Toast
//        const toastHtml = `
//            <div class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : 'success'} border-0" role="alert">
//                <div class="d-flex">
//                    <div class="toast-body">
//                        <strong>${title}</strong><br>
//                        ${message}
//                    </div>
//                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
//                </div>
//            </div>
//        `;

//        const $toast = $(toastHtml);
//        $('.toast-container').append($toast);
//        const toast = new bootstrap.Toast($toast);
//        toast.show();

//        // Remove toast after it's hidden
//        $toast.on('hidden.bs.toast', function () {
//            $(this).remove();
//        });
//    };

//    const clearForm = function () {
//        $(config.selectors.eventTitle).val('');
//        $(config.selectors.eventDescription).val('');
//        resetFormValidation();
//    };

//    const initializeTooltips = function () {
//        $('[data-toggle="tooltip"]').tooltip();
//    };

//    // Public methods
//    return {
//        init: function (options) {
//            // Merge custom options with defaults
//            addEventUrl = options.addEventUrl;

//            // Create toast container if it doesn't exist
//            if (!$('.toast-container').length) {
//                $('body').append('<div class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
//            }

//            // Attach event listeners
//            $(document).ready(function () {
//                $(config.selectors.calendarCell).click(handleDateClick);
//                $(config.selectors.saveEventBtn).click(handleEventSave);

//                // Handle modal events
//                $(config.selectors.eventModal)
//                    .on('hidden.bs.modal', clearForm)
//                    .on('shown.bs.modal', function () {
//                        $(config.selectors.eventTitle).focus();
//                    });

//                // Initialize tooltips
//                initializeTooltips();

//                // Handle form submission on enter key
//                $(config.selectors.eventForm).on('submit', function (e) {
//                    e.preventDefault();
//                    handleEventSave();
//                });
//            });
//        }
//    };
//})();

//// Add to global namespace
//window.CalendarModule = CalendarModule;

// wwwroot/js/calendar/calendarModule.js
const CalendarModule = (function () {
    const config = {
        selectors: {
            addEventBtn: '.add-event-btn',
            eventModal: '#eventModal',
            modalContent: '#modalContent'
        }
    };

    const handleAddEventClick = function (e) {
        e.preventDefault();
        const date = $(this).data('date');
        loadAddEventForm(date);
    };

    const loadAddEventForm = function (date) {
        $.get('/Calendar/AddEvent', { date: date }, function (data) {
            $(config.selectors.modalContent).html(data);
            $(config.selectors.eventModal).modal('show');
        });
    };

    return {
        init: function () {
            $(document).ready(function () {
                $(config.selectors.addEventBtn).click(handleAddEventClick);

                // Handle form submission
                $(document).on('submit', 'form', function (e) {
                    if (this.checkValidity() === false) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    $(this).addClass('was-validated');
                });
            });
        }
    };
})();

// Add to global namespace
window.CalendarModule = CalendarModule;