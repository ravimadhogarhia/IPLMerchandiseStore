import {
  createContext,
  useContext,
  useState,
  type ReactNode
} from "react";

import type {
  AuthenticatedUser,
  LoginRequest,
  RegisterRequest
} from "../types/auth";

import {
  login as loginService,
  register as registerService,
  logout as logoutService
} from "../services/authService";

interface AuthContextType {
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  async function login(request: LoginRequest): Promise<void> {
    setIsLoading(true);

    try {
      const response = await loginService(request);
      setUser(response.user);
    } finally {
      setIsLoading(false);
    }
  }

  async function register(request: RegisterRequest): Promise<void> {
    setIsLoading(true);

    try {
      await registerService(request);
    } finally {
      setIsLoading(false);
    }
  }

  function logout(): void {
    logoutService();
    setUser(null);
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isLoading,
        login,
        register,
        logout
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}