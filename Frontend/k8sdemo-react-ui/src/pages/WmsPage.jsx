import { useState } from "react";
import EventButton from "../components/EventButton";
import {
    publishMaterialPicked,
    publishMaterialFail
} from "../services/api";

export default function WmsPage() {

    const [result, setResult] = useState("");

    const sendMaterial = async () => {
        const data =
            await publishMaterialPicked();

        setResult(
            JSON.stringify(data, null, 2)
        );
    };

    const sendFail = async () => {
        const data =
            await publishMaterialFail();

        setResult(
            JSON.stringify(data, null, 2)
        );
    };

    return (
        <>
            <h1>WMS Event Center</h1>

            <EventButton
                text="送出領料事件"
                onClick={sendMaterial}
            />

            <EventButton
                text="送出 FAIL 事件"
                onClick={sendFail}
            />

            <pre>
                {result}
            </pre>
        </>
    );
}