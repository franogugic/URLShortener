import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Link as LinkIcon,
  Copy,
  Check,
  Trash2,
  Plus,
  X,
  LogOut,
  TrendingUp,
  MousePointerClick,
  Eye,
} from "lucide-react";
import { useAuth } from "../context/useAuth";
import { buildRedirectUrl, createUrl, deleteUrl, getUrlById, getUserUrls } from "../api/urlApi";
import { normalizeUrlList, normalizeUrlRecord } from "../utils/urlModel";

function generateShortCode() {
  const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
  let code = "";
  for (let i = 0; i < 6; i += 1) {
    code += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return code;
}

export function DashboardPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const [urls, setUrls] = useState([]);
  const [loading, setLoading] = useState(true);

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newUrl, setNewUrl] = useState("");
  const [creating, setCreating] = useState(false);

  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [selectedDetails, setSelectedDetails] = useState(null);

  const [copiedId, setCopiedId] = useState(null);
  const [error, setError] = useState("");

  const loadUrls = useCallback(async () => {
    setLoading(true);
    try {
      const storedUrls = await getUserUrls();
      setUrls(normalizeUrlList(storedUrls));
    } catch (loadError) {
      setError(loadError.message || "Failed to load URLs");
      if (loadError.status === 401) {
        navigate("/login", { replace: true });
      }
    } finally {
      setLoading(false);
    }
  }, [navigate]);

  useEffect(() => {
    loadUrls();
  }, [loadUrls]);

  const isValidUrl = (urlString) => {
    try {
      new URL(urlString);
      return true;
    } catch {
      return false;
    }
  };

  const handleCreate = async () => {
    setError("");

    if (!newUrl.trim()) {
      setError("Please enter a URL");
      return;
    }

    if (!isValidUrl(newUrl)) {
      setError("Please enter a valid URL");
      return;
    }

    setCreating(true);
    try {
      const created = await createUrl({
        longUrl: newUrl,
        shortUrlCode: generateShortCode(),
        description: "Created from dashboard",
      });

      const normalized = normalizeUrlRecord(created);
      setUrls((prev) => [normalized, ...prev]);
      setNewUrl("");
      setShowCreateModal(false);
    } catch (createError) {
      setError(createError.message || "Failed to create URL");
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteUrl(id);
      setUrls((prev) => prev.filter((url) => url.id !== id));
      if (selectedDetails?.id === id) {
        setShowDetailsModal(false);
        setSelectedDetails(null);
      }
    } catch (deleteError) {
      setError(deleteError.message || "Delete failed");
    }
  };

  const handleCopy = async (text, id) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopiedId(id);
      setTimeout(() => setCopiedId(null), 2000);
    } catch {
      setError("Copy failed");
    }
  };

  const handleOpenDetails = async (id) => {
    setShowDetailsModal(true);
    setDetailsLoading(true);

    try {
      const details = await getUrlById(id);
      setSelectedDetails(normalizeUrlRecord(details));
    } catch (detailsError) {
      setError(detailsError.message || "Failed to load details");
      if (detailsError.status === 401) {
        navigate("/login", { replace: true });
      }
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  const totalClicks = useMemo(() => urls.reduce((sum, url) => sum + url.clicks, 0), [urls]);

  return (
    <div className="min-h-screen bg-gray-950 text-white">
      <header className="border-b border-gray-800 bg-gray-900">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center gap-2">
              <div className="w-10 h-10 rounded-lg bg-gray-800 flex items-center justify-center">
                <LinkIcon className="w-6 h-6 text-gray-100" />
              </div>
              <span className="text-xl font-bold text-white">ShortLink</span>
            </div>

            <div className="flex items-center gap-4">
              <div className="hidden sm:flex items-center gap-3 px-4 py-2 bg-gray-800 rounded-lg border border-gray-700">
                <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center">
                  <span className="text-white text-sm font-semibold">{user?.username?.charAt(0)?.toUpperCase() || "U"}</span>
                </div>
                <div>
                  <p className="text-sm font-medium text-white">{user?.username}</p>
                </div>
              </div>
              <button
                onClick={handleLogout}
                className="flex items-center gap-2 px-4 py-2 text-gray-400 hover:text-white hover:bg-gray-800 rounded-lg transition-all"
                type="button"
              >
                <LogOut className="w-5 h-5" />
                <span className="hidden sm:inline">Logout</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          <div className="bg-gray-900 rounded-lg border border-gray-800 p-6">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 bg-gray-800 rounded-lg flex items-center justify-center">
                <LinkIcon className="w-5 h-5 text-blue-500" />
              </div>
              <span className="text-sm text-gray-400">Total Links</span>
            </div>
            <p className="text-3xl font-bold text-white">{urls.length}</p>
          </div>

          <div className="bg-gray-900 rounded-lg border border-gray-800 p-6">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 bg-gray-800 rounded-lg flex items-center justify-center">
                <MousePointerClick className="w-5 h-5 text-blue-500" />
              </div>
              <span className="text-sm text-gray-400">Total Clicks</span>
            </div>
            <p className="text-3xl font-bold text-white">{totalClicks}</p>
          </div>

          <div className="bg-gray-900 rounded-lg border border-gray-800 p-6">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 bg-gray-800 rounded-lg flex items-center justify-center">
                <TrendingUp className="w-5 h-5 text-blue-500" />
              </div>
              <span className="text-sm text-gray-400">Avg. Clicks</span>
            </div>
            <p className="text-3xl font-bold text-white">{urls.length > 0 ? Math.round(totalClicks / urls.length) : 0}</p>
          </div>
        </div>

        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold text-white">My Links</h2>
          <button
            onClick={() => {
              setError("");
              setShowCreateModal(true);
            }}
            className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-all"
            type="button"
          >
            <Plus className="w-5 h-5" />
            Create Link
          </button>
        </div>

        {error ? (
          <div className="mb-4 p-4 bg-red-950 border border-red-900 rounded-lg text-red-400 text-sm">{error}</div>
        ) : null}

        <div className="space-y-4">
          {loading ? (
            <div className="text-center py-16 bg-gray-900 rounded-lg border border-gray-800">
              <h3 className="text-xl font-semibold text-white">Loading...</h3>
            </div>
          ) : urls.length === 0 ? (
            <div className="text-center py-16 bg-gray-900 rounded-lg border border-gray-800">
              <LinkIcon className="w-16 h-16 text-gray-700 mx-auto mb-4" />
              <h3 className="text-xl font-semibold text-white mb-2">No links yet</h3>
              <p className="text-gray-400 mb-6">Create your first shortened URL to get started</p>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-all"
                type="button"
              >
                Create Your First Link
              </button>
            </div>
          ) : (
            urls.map((url) => {
              const shortened = buildRedirectUrl(url.shortUrlCode);
              return (
                <div
                  key={url.id}
                  className="bg-gray-900 rounded-lg border border-gray-800 p-6 hover:border-gray-700 transition-all group"
                >
                  <div className="flex items-start gap-4">
                    <div className="w-12 h-12 bg-gray-800 rounded-lg flex items-center justify-center flex-shrink-0">
                      <LinkIcon className="w-6 h-6 text-blue-500" />
                    </div>

                    <div className="flex-1 min-w-0">
                      <a
                        href={shortened}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-lg font-semibold text-blue-500 hover:text-blue-400 block mb-2"
                      >
                        {shortened}
                      </a>
                      <p className="text-gray-400 truncate mb-3">{url.longUrl}</p>
                      <div className="flex items-center gap-4 text-sm text-gray-500">
                        <span>{url.clicks} clicks</span>
                        <span>•</span>
                        <span>{new Date(url.createdAt).toLocaleDateString()}</span>
                      </div>
                    </div>

                    <div className="flex items-center gap-2 flex-shrink-0">
                      <button
                        onClick={() => handleOpenDetails(url.id)}
                        className="w-10 h-10 rounded-lg bg-gray-800 hover:bg-gray-700 flex items-center justify-center transition-all"
                        type="button"
                        aria-label="Details"
                      >
                        <Eye className="w-5 h-5 text-gray-300" />
                      </button>
                      <button
                        onClick={() => handleCopy(shortened, url.id)}
                        className="w-10 h-10 rounded-lg bg-gray-800 hover:bg-gray-700 flex items-center justify-center transition-all"
                        type="button"
                        aria-label="Copy"
                      >
                        {copiedId === url.id ? (
                          <Check className="w-5 h-5 text-green-500" />
                        ) : (
                          <Copy className="w-5 h-5 text-gray-400" />
                        )}
                      </button>
                      <button
                        onClick={() => handleDelete(url.id)}
                        className="w-10 h-10 rounded-lg bg-gray-800 hover:bg-red-950 flex items-center justify-center transition-all opacity-0 group-hover:opacity-100"
                        type="button"
                        aria-label="Delete"
                      >
                        <Trash2 className="w-5 h-5 text-red-500" />
                      </button>
                    </div>
                  </div>
                </div>
              );
            })
          )}
        </div>
      </main>

      {showCreateModal ? (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4" onClick={() => setShowCreateModal(false)}>
          <div
            onClick={(event) => event.stopPropagation()}
            className="bg-gray-900 rounded-lg border border-gray-800 shadow-2xl w-full max-w-md"
          >
            <div className="flex items-center justify-between p-6 border-b border-gray-800">
              <h2 className="text-2xl font-bold text-white">Create New Link</h2>
              <button
                onClick={() => setShowCreateModal(false)}
                className="w-10 h-10 rounded-lg hover:bg-gray-800 flex items-center justify-center transition-colors"
                type="button"
              >
                <X className="w-5 h-5 text-gray-400" />
              </button>
            </div>

            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">Enter your long URL</label>
                <input
                  type="text"
                  value={newUrl}
                  onChange={(event) => {
                    setNewUrl(event.target.value);
                    setError("");
                  }}
                  onKeyDown={(event) => event.key === "Enter" && handleCreate()}
                  placeholder="https://example.com/your-long-url"
                  className="w-full px-4 py-3 bg-gray-950 border border-gray-800 rounded-lg text-white placeholder:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                />
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 px-6 py-3 border border-gray-800 text-gray-300 rounded-lg hover:bg-gray-800 transition-colors font-medium"
                  type="button"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreate}
                  className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-all font-medium"
                  type="button"
                  disabled={creating}
                >
                  {creating ? "Creating..." : "Create Link"}
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}

      {showDetailsModal ? (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4" onClick={() => setShowDetailsModal(false)}>
          <div
            onClick={(event) => event.stopPropagation()}
            className="bg-gray-900 rounded-lg border border-gray-800 shadow-2xl w-full max-w-xl"
          >
            <div className="flex items-center justify-between p-6 border-b border-gray-800">
              <h2 className="text-2xl font-bold text-white">Link Details</h2>
              <button
                onClick={() => setShowDetailsModal(false)}
                className="w-10 h-10 rounded-lg hover:bg-gray-800 flex items-center justify-center transition-colors"
                type="button"
              >
                <X className="w-5 h-5 text-gray-400" />
              </button>
            </div>

            <div className="p-6">
              {detailsLoading ? (
                <p className="text-gray-300">Loading details...</p>
              ) : selectedDetails ? (
                <div className="grid gap-3">
                  <div className="p-3 bg-gray-950 rounded-lg border border-gray-800">
                    <p className="text-xs text-gray-500 mb-1">Short Code</p>
                    <p className="text-white font-medium">{selectedDetails.shortUrlCode}</p>
                  </div>
                  <div className="p-3 bg-gray-950 rounded-lg border border-gray-800">
                    <p className="text-xs text-gray-500 mb-1">Clicks</p>
                    <p className="text-white font-medium">{selectedDetails.clicks}</p>
                  </div>
                  <div className="p-3 bg-gray-950 rounded-lg border border-gray-800">
                    <p className="text-xs text-gray-500 mb-1">Created At</p>
                    <p className="text-white font-medium">{new Date(selectedDetails.createdAt).toLocaleString()}</p>
                  </div>
                  <div className="p-3 bg-gray-950 rounded-lg border border-gray-800">
                    <p className="text-xs text-gray-500 mb-1">Description</p>
                    <p className="text-white font-medium">{selectedDetails.description}</p>
                  </div>
                  <div className="p-3 bg-gray-950 rounded-lg border border-gray-800">
                    <p className="text-xs text-gray-500 mb-1">Long URL</p>
                    <a href={selectedDetails.longUrl} target="_blank" rel="noreferrer" className="text-blue-400 break-all">
                      {selectedDetails.longUrl}
                    </a>
                  </div>
                  <div className="p-3 bg-gray-950 rounded-lg border border-gray-800">
                    <p className="text-xs text-gray-500 mb-1">Redirect URL</p>
                    <a
                      href={buildRedirectUrl(selectedDetails.shortUrlCode)}
                      target="_blank"
                      rel="noreferrer"
                      className="text-blue-400 break-all"
                    >
                      {buildRedirectUrl(selectedDetails.shortUrlCode)}
                    </a>
                  </div>
                </div>
              ) : (
                <p className="text-gray-300">No details available.</p>
              )}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}

export default DashboardPage;
