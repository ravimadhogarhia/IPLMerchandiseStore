
import { useState, type FormEvent } from "react";
import { useAuth } from "../context/AuthContext";

interface LoginPageProps {
  onRegisterClick: () => void;
  onLoginSuccess: () => void;
}

function LoginPage({
  onRegisterClick,
  onLoginSuccess
}: LoginPageProps) {
  const { login, isLoading } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    try {
      await login({ email, password });
      onLoginSuccess();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Login failed."
      );
    }
  }

  return (
    <div className="auth-container">
      <form className="auth-card" onSubmit={handleSubmit}>
        <h2>Welcome Back!</h2>
        <p>Login to your IPL Merchandise Store account.</p>

        {error && <div className="auth-error">{error}</div>}

        <label>Email Address</label>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="Enter your email"
          required
        />

        <label>Password</label>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Enter your password"
          required
        />

        <button type="submit" disabled={isLoading}>
          {isLoading ? "Logging in..." : "Login"}
        </button>

        <p>
          Don't have an account?{" "}
          <button type="button" onClick={onRegisterClick}>
            Register
          </button>
        </p>
      </form>
    </div>
  );
}

export default LoginPage;
