import React, { useEffect, useState } from 'react';
import { useAuth } from '../AuthProvider';
import { useNavigate } from 'react-router-dom';

const Home = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const [urls, setUrls] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [longUrl, setLongUrl] = useState('');
    const [shortCode, setShortCode] = useState('');
    const [description, setDescription] = useState('');
    const [creating, setCreating] = useState(false);

    const backendBaseUrl = 'http://localhost:5010';

    const fetchUrls = async () => {
        setLoading(true);
        try {
            const response = await fetch(`${backendBaseUrl}/url/getAllUrlsByUserId`, {
                method: 'GET',
                credentials: 'include',
            });
            if (!response.ok) throw new Error('Failed to fetch URLs');
            const data = await response.json();
            setUrls(data);
        } catch (err) {
            console.error(err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (!user) navigate("/login");
        else fetchUrls();
    }, [user]);

    const handleLogout = async () => {
        await logout();
        navigate("/login");
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Are you sure you want to delete this URL?')) return;
        try {
            const response = await fetch(`${backendBaseUrl}/url/delete/${id}`, {
                method: 'DELETE',
                credentials: 'include',
            });
            if (!response.ok) throw new Error('Failed to delete URL');
            setUrls(urls.filter(u => u.id !== id));
        } catch (err) {
            console.error(err);
            alert('Failed to delete URL');
        }
    };

    const handleCreate = async (e) => {
        e.preventDefault();
        if (!longUrl) return alert('Long URL is required');
        setCreating(true);
        try {
            const response = await fetch(`${backendBaseUrl}/url/create`, {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    LongUrl: longUrl,
                    ShortUrlCode: shortCode || undefined,
                    Description: description || undefined
                }),
            });
            if (!response.ok) {
                const text = await response.text();
                throw new Error(text || 'Failed to create URL');
            }
            const newUrl = await response.json();
            setUrls([newUrl, ...urls]);
            setLongUrl('');
            setShortCode('');
            setDescription('');
        } catch (err) {
            console.error(err);
            alert('Error creating URL: ' + err.message);
        } finally {
            setCreating(false);
        }
    };

    if (loading) return <p>Loading URLs...</p>;
    if (error) return <p>Error: {error}</p>;
    if (urls.length === 0) return <p>No URLs found</p>;

    return (
        <div className="min-h-screen bg-gray-100 p-6">
            <div className="max-w-3xl mx-auto">
                <div className="flex justify-between items-center mb-6">
                    <h2 className="text-2xl font-bold text-gray-800">Your Shortened URLs</h2>
                    <button
                        onClick={handleLogout}
                        className="bg-red-600 text-white px-4 py-2 rounded-md hover:bg-red-700 transition duration-200"
                    >
                        Logout
                    </button>
                </div>

                {/* Create URL form */}
                <form onSubmit={handleCreate} className="bg-white p-6 rounded-lg shadow-md mb-6 flex flex-col gap-4">
                    <h3 className="text-lg font-semibold text-gray-700">Create a New Short URL</h3>
                    <input
                        type="url"
                        placeholder="Long URL"
                        value={longUrl}
                        onChange={e => setLongUrl(e.target.value)}
                        required
                        className="border rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                    <input
                        type="text"
                        placeholder="Short Code (optional)"
                        value={shortCode}
                        onChange={e => setShortCode(e.target.value)}
                        className="border rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                    <input
                        type="text"
                        placeholder="Description (optional)"
                        value={description}
                        onChange={e => setDescription(e.target.value)}
                        className="border rounded-md p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                    <button
                        type="submit"
                        disabled={creating}
                        className="bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 transition duration-200"
                    >
                        {creating ? 'Creating...' : 'Create URL'}
                    </button>
                </form>

                {/* URL list */}
                <ul className="space-y-3">
                    {urls.filter(u => u.shortUrlCode).map(url => (
                        <li key={url.id} className="bg-white p-4 rounded-lg shadow flex justify-between items-center">
                            <div>
                                <a
                                    href={`${backendBaseUrl}/url/${encodeURIComponent(url.shortUrlCode)}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="text-blue-600 font-medium hover:underline"
                                >
                                    {url.shortUrlCode}
                                </a>
                                <span className="ml-2 text-gray-600">{url.description}</span>
                            </div>
                            <button
                                onClick={() => handleDelete(url.id)}
                                className="bg-red-600 text-white px-3 py-1 rounded-md hover:bg-red-700 transition duration-200"
                            >
                                Delete
                            </button>
                        </li>
                    ))}
                </ul>
            </div>
        </div>
    );
};

export default Home;
