document.addEventListener("DOMContentLoaded", function () {

    if (typeof chartLabels === "undefined" || typeof chartValues === "undefined") {
        console.warn("Chart data not found");
        return;
    }

    const ctx = document.getElementById('gpaChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: chartLabels,
            datasets: [{
                label: 'GPA trung bình',
                data: chartValues,
                borderWidth: 3,
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4
                }
            }
        }
    });
});
document.getElementById("termSelect")
    .addEventListener("change", function () {

        const termId = this.value;

        fetch(`/Giangvien/GetDashboardData?termId=${termId}`)
            .then(res => res.json())
            .then(data => {
                updateChart(data.labels, data.values);
                updateSummary(data.totalStudent, data.avgGpa);
            });
    });
