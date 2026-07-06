import { useEffect, useState } from "react";
import { getDlqMessages, requeueDlq } from "../services/api";

export default function DlqPage() {

    const [items, setItems] = useState([]);

    useEffect(() => {
        load();
    }, []);

    async function load() {
        const data = await getDlqMessages();
        setItems(data);
    }

    return (
        <>
            <h1>DLQ Center</h1>

            <table>
                <thead>
                    <tr>
                        <th>Time</th>
                        <th>WorkOrder</th>
                        <th>ReelId</th>
                        <th>Material</th>
                        <th>Retry</th>
                        <th>Error</th>
                        <th>Action</th>
                    </tr>
                </thead>

                <tbody>
                    {items.map((x, i) => (
                        <tr key={i}>
                            <td>
                                {new Date(x.time)
                                    .toLocaleString()}
                            </td>

                            <td>{x.workOrder}</td>

                            <td>{x.reelId}</td>

                            <td>{x.material}</td>

                            <td>{x.retryCount}</td>

                            <td>{x.errorMessage}</td>

                            <td>
                                <button
                                    onClick={async () => {
                                        await requeueDlq(x.workOrder, x.reelId);
                                        await load();
                                    }}
                                >
                                    Requeue
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </>
    );
}