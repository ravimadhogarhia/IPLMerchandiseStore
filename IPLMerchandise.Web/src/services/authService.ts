import { apiRequest, setAccessToken } from "./apiClient";
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  AuthenticatedUser
} from "../types/auth";

export async function register(
  request: RegisterRequest
): Promise<AuthenticatedUser> {
  return apiRequest<AuthenticatedUser>(
    "/api/auth/register",
    {
      method: "POST",
      body: JSON.stringify(request)
    }
  );
}

export async function login(
  request: LoginRequest
): Promise<LoginResponse> {
  const response = await apiRequest<LoginResponse>(
    "/api/auth/login",
    {
      method: "POST",
      body: JSON.stringify(request)
    }
  );

  setAccessToken(response.accessToken);

  return response;
}

export async function getCurrentUser(): Promise<AuthenticatedUser> {
  return apiRequest<AuthenticatedUser>("/api/auth/me");
}

export function logout(): void {
  setAccessToken(null);
}