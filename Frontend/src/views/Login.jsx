import React, { useState } from "react";
import { useAuth } from "../AuthProvider";
import { useNavigate } from "react-router-dom";
import { Link2, Mail, Lock, ArrowRight } from 'lucide-react';


export default function Login() {
    const { login } = useAuth();
    const navigate = useNavigate();
    const [username, setUsername] = useState("test");
    const [password, setPassword] = useState("test");

    const handleSubmit = async (e) => {
        e.preventDefault();
        const success = await login(username, password);
        if (success) navigate("/home");
        else alert("Login failed");
    };

    return (
        <div className="relative z-10">
            <div className="min-h-screen flex items-center justify-center p-4">
                <div className="w-full max-w-md">
                    <div className="text-center mb-8">
                        <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-br from-purple-500 to-pink-500 mb-4 shadow-lg shadow-purple-500/50">
                            <Link2 className="w-8 h-8 text-white" />
                        </div>
                        <h1 className="text-4xl font-semibold text-white mb-2">Welcome Back</h1>
                        <p className="text-slate-400">Login to continue shortening URLs</p>
                    </div>

                    <div className="backdrop-blur-xl bg-white/10 rounded-3xl p-8 shadow-2xl border border-white/20">
                        <form onSubmit={handleSubmit} className="space-y-6">
                            <div>
                                <label className="block text-sm font-medium text-slate-300 mb-2">
                                    Username
                                </label>
                                <div className="relative">
                                    <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
                                    <input
                                        type="text"
                                        value={username}
                                        onChange={(e) => setUsername(e.target.value)}
                                        className="w-full pl-12 pr-4 py-3.5 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                                        placeholder="Username"
                                        required
                                    />
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-slate-300 mb-2">
                                    Password
                                </label>
                                <div className="relative">
                                    <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
                                    <input
                                        type="password"
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        className="w-full pl-12 pr-4 py-3.5 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent transition-all"
                                        placeholder="••••••••"
                                        required
                                    />
                                </div>
                            </div>

                            {/* Submit Button */}
                            <button
                                type="submit"
                                className="w-full bg-gradient-to-r from-purple-500 to-pink-500 text-white py-3.5 rounded-xl font-medium hover:shadow-lg hover:shadow-purple-500/50 transition-all duration-300 flex items-center justify-center gap-2 group"
                            >
                                Login
                                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                            </button>
                        </form>

                        {/* *********************************** DOVDEEEEEEEEEEEEEEEE */}

                        <div className="mt-6 text-center">
                            <p className="text-slate-400">
                                Don't have an account?{' '}
                                <button
                                    //onClick={onSwitchToRegister}
                                    className="text-purple-400 hover:text-purple-300 font-medium transition-colors"
                                >
                                    Sign up
                                </button>
                            </p>
                        </div>
                    </div>

                    <div className="mt-8 grid grid-cols-3 gap-4 text-center">
                        <div className="backdrop-blur-xl bg-white/5 rounded-2xl p-4 border border-white/10">
                            <div className="text-2xl font-semibold text-white mb-1">1M+</div>
                            <div className="text-xs text-slate-400">Links Shortened</div>
                        </div>
                        <div className="backdrop-blur-xl bg-white/5 rounded-2xl p-4 border border-white/10">
                            <div className="text-2xl font-semibold text-white mb-1">99.9%</div>
                            <div className="text-xs text-slate-400">Uptime</div>
                        </div>
                        <div className="backdrop-blur-xl bg-white/5 rounded-2xl p-4 border border-white/10">
                            <div className="text-2xl font-semibold text-white mb-1">Fast</div>
                            <div className="text-xs text-slate-400">Redirects</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
