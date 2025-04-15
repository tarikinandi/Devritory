document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".reply-link").forEach(link => {
        link.addEventListener("click", (e) => {
            e.preventDefault();
            const id = link.dataset.id;
            const form = document.getElementById(`reply-form-${id}`);
            form.classList.toggle("d-none");
        });
    });
});