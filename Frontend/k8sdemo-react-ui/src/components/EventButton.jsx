export default function EventButton({
    text,
    onClick
}) {
    return (
        <button
            onClick={onClick}
            style={{
                marginRight: "10px",
                padding: "10px"
            }}
        >
            {text}
        </button>
    );
}