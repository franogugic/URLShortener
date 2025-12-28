import React, { useState } from "react";
import { useAuth } from "../AuthProvider";
import { useNavigate } from "react-router-dom";

export default function Login() {
    const { login } = useAuth();
    const navigate = useNavigate();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();
        const success = await login(username, password);
        if (success) {
            navigate("/home");
        } else {
            alert("Login failed");
        }
    };

    return (
        <div className="flex items-center justify-center flex-col mt-50">
            <h2>Login</h2>
                <form onSubmit={handleSubmit} className="flex items-center justify-center flex-col mt-8">
                    <label>username</label>
                    <input
                        placeholder="Username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        className="w-full bg-blue-400 placeholder-white"
                    />
                    <label>password</label>
                    <input
                        placeholder="Password"
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full bg-blue-400 placeholder-white"
                    />
                <button type="submit" className="bg-blue-800 text-white w-full">Login</button>
                </form>
        </div>
    );
}
