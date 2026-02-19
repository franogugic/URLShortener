import { useEffect, useMemo, useState } from "react";
import { ApiError } from "../lib/http";
import { getMe, loginUser, logoutUser, registerUser } from "../api/authApi";
import { AuthContext } from "./auth-context";

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let active = true;

    async function bootstrap() {
      try {
        const me = await getMe();
        if (active) {
          setUser({
            id: me?.id || me?.Id,
            username: me?.username || me?.Username,
          });
        }
      } catch {
        if (active) setUser(null);
      } finally {
        if (active) setIsLoading(false);
      }
    }

    bootstrap();
    return () => {
      active = false;
    };
  }, []);

  const value = useMemo(
    () => ({
      user,
      isLoading,
      async login(username, password) {
        await loginUser({ username, password });
        const me = await getMe();
        setUser({
          id: me?.id || me?.Id,
          username: me?.username || me?.Username,
        });
      },
      async register(username, password) {
        await registerUser({ username, password });
      },
      async logout() {
        try {
          await logoutUser();
        } catch (error) {
          if (!(error instanceof ApiError) || error.status !== 401) {
            throw error;
          }
        }
        setUser(null);
      },
      setUser,
    }),
    [isLoading, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
