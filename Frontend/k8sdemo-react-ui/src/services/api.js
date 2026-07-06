const API_BASE = "/api";

export async function publishMaterialPicked() {
    const response = await fetch(
        `${API_BASE}/wms/material-picked`,
        {
            method: "POST"
        }
    );

    return await response.json();
}

export async function publishMaterialFail() {
    const response = await fetch(
        `${API_BASE}/wms/material-fail`,
        {
            method: "POST"
        }
    );

    return await response.json();
}

export async function getDashboardStatus() {
    const response = await fetch(`${API_BASE}/dashboard/status`);

    return await response.json();
}

export async function getDlqMessages() {
    const response = await fetch(`${API_BASE}/dashboard/dlq`);

    return await response.json();
}

export async function requeueDlq(workOrder, reelId) {
    await fetch(
        "/api/dashboard/dlq/requeue",
        {
            method: "POST",
            headers: {
                "Content-Type":
                    "application/json"
            },
            body: JSON.stringify({
                workOrder,
                reelId
            })
        });
}