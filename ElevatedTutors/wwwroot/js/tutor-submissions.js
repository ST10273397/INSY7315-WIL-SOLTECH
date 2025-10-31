document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("submissionModal");

    modal.addEventListener("show.bs.modal", event => {
        const button = event.relatedTarget;
        const student = button.getAttribute("data-student");
        const title = button.getAttribute("data-title");
        const grade = button.getAttribute("data-grade");
        const notes = button.getAttribute("data-notes");
        const id = button.getAttribute("data-id");

        modal.querySelector("#viewStudentName").value = student;
        modal.querySelector("#viewTitle").value = title;
        modal.querySelector("#viewGrade").value = grade || "—";
        modal.querySelector("#viewFeedback").value = notes || "";

        modal.querySelector("#editSubmissionId").value = id;
        modal.querySelector("#feedbackSubmissionId").value = id;

        switchTo("view");
    });

    function switchTo(mode) {
        const view = modal.querySelector("#viewSection");
        const edit = modal.querySelector("#editMarksForm");
        const feedback = modal.querySelector("#feedbackForm");
        const buttons = modal.querySelector("#defaultButtons");

        [view, edit, feedback].forEach(e => e.classList.add("d-none"));
        buttons.classList.add("d-none");

        if (mode === "view") {
            view.classList.remove("d-none");
            buttons.classList.remove("d-none");
        } else if (mode === "edit") {
            edit.classList.remove("d-none");
            modal.querySelector("#editGrade").value = modal.querySelector("#viewGrade").value;
        } else if (mode === "feedback") {
            feedback.classList.remove("d-none");
            modal.querySelector("#editFeedback").value = modal.querySelector("#viewFeedback").value;
        }
    }

    modal.querySelector("#openEditMarksBtn").addEventListener("click", () => switchTo("edit"));
    modal.querySelector("#openFeedbackBtn").addEventListener("click", () => switchTo("feedback"));

    modal.querySelectorAll(".cancelBtn").forEach(btn => {
        btn.addEventListener("click", () => switchTo("view"));
    });
});
