document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("[data-confirm]").forEach(function (element) {
        element.addEventListener("click", function (event) {
            const poruka = element.getAttribute("data-confirm")
                || "Da li ste sigurni?";

            if (!window.confirm(poruka)) {
                event.preventDefault();
            }
        });
    });
});
