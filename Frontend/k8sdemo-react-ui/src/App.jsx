import { Link, Routes, Route } from "react-router-dom";

import Dashboard from "./pages/Dashboard";
import WmsPage from "./pages/WmsPage";
import DlqPage from "./pages/DlqPage";

function App() {
  return (
    <>
      <nav
        style={{
          display: "flex",
          gap: "20px",
          marginBottom: "20px"
        }}
      >
        <Link to="/">
          Dashboard
        </Link>

        <Link to="/wms">
          WMS Events
        </Link>

        <Link to="/dlq">
          DLQ Center
        </Link>
      </nav>

      <Routes>

        <Route
          path="/"
          element={<Dashboard />}
        />

        <Route
          path="/wms"
          element={<WmsPage />}
        />

        <Route
          path="/dlq"
          element={<DlqPage />}
        />

      </Routes>
    </>
  );
}

export default App;