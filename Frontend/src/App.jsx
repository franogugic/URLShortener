import React from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./AuthProvider";
import ProtectedRoute from "./ProtectedRoute";
import Login from "./views/Login.jsx";
import Home from "./views/Home.jsx";

function App() {
    return (
        <AuthProvider>
                <Routes>
                    <Route path="/login" element={<Login />} />

                    <Route
                        path="/home"
                        element={
                            <ProtectedRoute>
                                <Home />
                            </ProtectedRoute>
                        }
                    />

                    {/* fallback: ako user pokuša ići negdje drugdje */}
                    <Route path="*" element={<Login />} />
                </Routes>
        </AuthProvider>
    );
}

export default App;
