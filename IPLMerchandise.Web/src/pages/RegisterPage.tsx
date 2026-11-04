
import { useState, type FormEvent } from "react";
import { useAuth } from "../context/AuthContext";

interface RegisterPageProps {
  onBackToLogin: () => void;
}

function RegisterPage({ onBackToLogin }: RegisterPageProps) {
  const { register, isLoading } = useAuth();

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    if (new TextEncoder().encode(password).length > 72) {
      setError("Password must not exceed 72 UTF-8 bytes.");
      return;
    }

    try {
      await register({ fullName, email, password });

      setSuccess("Registration successful! Please log in.");

      setTimeout(() => {
        onBackToLogin();
      }, 1200);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Registration failed."
      );
    }
  }

  return (
    <div className="auth-container">
      <form className="auth-card" onSubmit={handleSubmit}>
        <h2>Create Account</h2>
        <p>Join the IPL Merchandise Store.</p>

        {error && <div className="auth-error">{error}</div>}
        {success && <div className="auth-success">{success}</div>}

        <label>Full Name</label>
        <input
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
          placeholder="Enter your full name"
          minLength={2}
          maxLength={100}
          required
        />

        <label>Email Address</label>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="Enter your email"
          maxLength={254}
          required
        />

        <label>Password</label>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Minimum 8 characters"
          minLength={8}
          required
        />

        <label>Confirm Password</label>
        <input
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          placeholder="Confirm your password"
          required
        />

        <button type="submit" disabled={isLoading}>
          {isLoading ? "Creating account..." : "Register"}
        </button>

        <p>
          Already registered?{" "}
          <button type="button" onClick={onBackToLogin}>
            Login
          </button>
        </p>
      </form>
    </div>
  );
}

export default RegisterPage;
