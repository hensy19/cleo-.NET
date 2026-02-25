document.addEventListener("DOMContentLoaded", function () {

    function reveal() {
        const reveals = document.querySelectorAll(".reveal");

        reveals.forEach((element) => {
            const windowHeight = window.innerHeight;
            const elementTop = element.getBoundingClientRect().top;
            const revealPoint = 120;

            if (elementTop < windowHeight - revealPoint) {
                element.classList.add("active");
            }
        });
    }

    window.addEventListener("scroll", reveal);
    reveal();
});