import React, { useState } from "react";
import { useAuth } from "../AuthProvider";
import { useNavigate } from "react-router-dom";

const Register = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { register } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        const success = await register(username, password);
        if (success) {
            alert("Uspješna registracija! Sada se prijavi.");
            navigate("/login");
        } else {
            alert("Registracija nije uspjela. Možda korisnik već postoji?");
        }
    };

    return (
        <div>
            <h2>Registracija</h2>
            <form onSubmit={handleSubmit}>
                <input type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} required />
                <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} required />
                <button type="submit">Registriraj se</button>
            </form>
        </div>
    );
};

export default Register;