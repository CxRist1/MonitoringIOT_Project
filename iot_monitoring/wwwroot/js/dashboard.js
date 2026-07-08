const temperatureValue = document.getElementById("temperatureValue");
const humidityValue = document.getElementById("humidityValue");
const soilValue = document.getElementById("soilValue");
const statusValue = document.getElementById("statusValue");

const ctx = document.getElementById("sensorChart");

const sensorChart = new Chart(ctx, {
    type: "line",
    data: {
        labels: [],
        datasets: [
            {
                label: "Temperature (°C)",
                data: [],
                borderWidth: 2,
                tension: 0.4
            },
            {
                label: "Humidity (%)",
                data: [],
                borderWidth: 2,
                tension: 0.4
            }
        ]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: "bottom"
            }
        },
        scales: {
            y: {
                beginAtZero: false
            }
        }
    }
});

async function loadSensorData() {
    const response = await fetch("/api/sensorapi");
    const data = await response.json();

    temperatureValue.textContent = `${data.temperature} °C`;
    humidityValue.textContent = `${data.humidity} %`;
    soilValue.textContent = `${data.soilMoisture} %`;
    statusValue.textContent = data.status;

    sensorChart.data.labels.push(data.time);
    sensorChart.data.datasets[0].data.push(data.temperature);
    sensorChart.data.datasets[1].data.push(data.humidity);

    if (sensorChart.data.labels.length > 10) {
        sensorChart.data.labels.shift();
        sensorChart.data.datasets[0].data.shift();
        sensorChart.data.datasets[1].data.shift();
    }

    sensorChart.update();
}

loadSensorData();
setInterval(loadSensorData, 2000);