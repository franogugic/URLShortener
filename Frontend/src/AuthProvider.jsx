import React, { createContext, useContext, useState, useEffect } from "react";

const API_URL = import.meta.env.VITE_API_URL

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchMe = async () => {
            try {
                const res = await fetch(`${API_URL}/api/user/me`, { credentials: "include" });
                if (!res.ok) return setUser(null);
                const data = await res.json();
                setUser(data);
            } catch {
                setUser(null);
            } finally {
                setLoading(false);
            }
        };
        fetchMe();
    }, []);

    const login = async (username, password) => {
        const res = await fetch(`${API_URL}/api/user/login`, {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });
        if (res.ok) {
            const meRes = await fetch(`${API_URL}/api/user/me`, { credentials: "include" });
            setUser(await meRes.json());
            return true;
        }
        return false;
    };

    const logout = async () => {
        try {
            const res = await fetch(`${API_URL}/api/user/logout`, {
                method: "POST",
                credentials: "include",
            });
            if (res.ok) {
                setUser(null); // odmah očisti auth state
            }
        } catch (err) {
            console.error("Logout failed:", err);
        }
    };

    const register = async (username, password) => {
        const res = await fetch(`${API_URL}/api/user/register`, {
            method: "POST",
            credentials: "include", 
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });
        return res.ok; 
    };

    return (
        <AuthContext.Provider value={{ user, loading, login, logout, register }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
