import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
} from "chart.js";

import { Line } from "react-chartjs-2";
import { useEffect, useState } from "react";
import { getDashboardStatus } from "../services/api";

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
);

export default function Dashboard() {
    const [status, setStatus] = useState(null);

    const loadStatus = async () => {
        try {
            const data = await getDashboardStatus();
            setStatus(data);
        }
        catch (error) {
            console.error("Load dashboard status failed", error);
        }
    };

    useEffect(() => {
        loadStatus();

        const timer = setInterval(() => {
            loadStatus();
        }, 3000);

        return () => clearInterval(timer);
    }, []);

    const statistics = status?.statistics ?? {
        successCount: 0,
        failCount: 0,
        retryCount: 0,
        dlqCount: 0,
        recentEvents: [],
        trendData: []
    };

    const trendChartData = {
        labels: statistics.trendData?.map(x =>
            new Date(x.time).toLocaleTimeString()
        ) ?? [],

        datasets: [
            {
                label: "Success",
                data: statistics.trendData?.map(x => x.success) ?? [],
                borderColor: "green",
            },
            {
                label: "Retry",
                data: statistics.trendData?.map(x => x.retry) ?? [],
                borderColor: "orange",
            },
            {
                label: "DLQ",
                data: statistics.trendData?.map(x => x.dlq) ?? [],
                borderColor: "purple",
            },
            {
                label: "Fail",
                data: statistics.trendData?.map(x => x.fail) ?? [],
                borderColor: "red",
            }
        ]
    };

    return (
        <>
            <h1>K8s Demo Dashboard</h1>

            <p>
                RabbitMQ +
                Microservices +
                Kubernetes
            </p>

            <h2>Service Status</h2>

            {!status && (
                <p>Loading...</p>
            )}

            {status && (
                <div className="status-grid">
                    <div className="status-card">
                        <h3>WMS API</h3>
                        <p>{status.wmsApi}</p>
                    </div>

                    <div className="status-card">
                        <h3>SAP API</h3>
                        <p>{status.sapApi}</p>
                    </div>

                    <div className="status-card">
                        <h3>RabbitMQ</h3>
                        <p>{status.rabbitMq}</p>
                    </div>

                    <div className="status-card">
                        <h3>SAP Consumer</h3>
                        <p>{status.sapConsumer}</p>
                    </div>

                    <div>
                        Last Update:
                        <br />
                        {new Date(status.time).toLocaleString()}
                    </div>
                </div>
            )}

            <h2>Statistics</h2>

            <div className="status-grid">

                <div className="status-card">
                    <h3>Success</h3>
                    <p>{statistics.successCount}</p>
                </div>

                <div className="status-card">
                    <h3>Fail</h3>
                    <p>{statistics.failCount}</p>
                </div>

                <div className="status-card">
                    <h3>Retry</h3>
                    <p>{statistics.retryCount}</p>
                </div>

                <div className="status-card">
                    <h3>DLQ</h3>
                    <p>{statistics.dlqCount}</p>
                </div>

                <div className="status-card">
                    <h3>Queue</h3>

                    <p style={{
                        color: statistics.queueCount > 0
                            ? "orange"
                            : "green"
                    }}>
                        {statistics.queueCount}
                    </p>
                </div>

                <div className="status-card">
                    <h3>DLQ Queue</h3>

                    <p style={{
                        color: statistics.dlqQueueCount > 0
                            ? "red"
                            : "green"
                    }}>
                        {statistics.dlqQueueCount}
                    </p>
                </div>

            </div>

            <h2>Event Trend</h2>

            <div className="chart-box">
                <Line
                    data={trendChartData}
                    options={{
                        responsive: true,
                        maintainAspectRatio: false
                    }}
                />
            </div>

            <h2>Recent Events</h2>

            <table>
                <thead>
                    <tr>
                        <th>Time</th>
                        <th>WorkOrder</th>
                        <th>ReelId</th>
                        <th>Result</th>
                    </tr>
                </thead>

                <tbody>
                    {statistics.recentEvents.map((event, index) => (
                        <tr key={index}>
                            <td>{new Date(event.time).toLocaleString()}</td>
                            <td>{event.workOrder}</td>
                            <td>{event.reelId}</td>
                            <td>{event.result}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </>
    );
}