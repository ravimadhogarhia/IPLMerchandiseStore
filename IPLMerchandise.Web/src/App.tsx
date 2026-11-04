
import { useCallback, useEffect, useState } from "react";
import ProductsPage from "./pages/ProductsPage";
import CartPage from "./pages/CartPage";
import CheckoutPage from "./pages/CheckoutPage";
import OrderHistoryPage from "./pages/OrderHistoryPage";
import { getCart } from "./services/cartService";
import { DEMO_CUSTOMER_ID } from "./config";
import { useAuth } from "./context/AuthContext";
import "./styles/App.css";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

type Page =
  | "store"
  | "cart"
  | "checkout"
  | "orders"
  | "login"
  | "register";

function App() {
  const [page, setPage] = useState<Page>("store");
  const [cartCount, setCartCount] = useState(0);

  const { user, isAuthenticated, logout } = useAuth();

  const refreshCartCount = useCallback(async () => {
    try {
      const cart = await getCart(DEMO_CUSTOMER_ID);

      const count = cart.items.reduce(
        (total, item) => total + item.quantity,
        0
      );

      setCartCount(count);
    } catch (error) {
      console.error("Unable to refresh cart count:", error);
    }
  }, []);

  useEffect(() => {
    void refreshCartCount();
  }, [refreshCartCount]);

  const handleLogout = () => {
    logout();
    setPage("store");
  };

  return (
    <div className="app">
      <header className="navbar">
        <button
          className="brand brand-button"
          onClick={() => setPage("store")}
        >
          IPL STORE
        </button>

        <nav>
          <button onClick={() => setPage("store")}>
            Products
          </button>

          <button onClick={() => setPage("orders")}>
            My Orders
          </button>

          <button onClick={() => setPage("cart")}>
            My Cart ({cartCount})
          </button>
        </nav>

        <div className="auth-menu">
  {isAuthenticated ? (
    <>
      <span className="welcome-user">
        Welcome, {user?.fullName}
      </span>

      <button
        className="logout-button"
        onClick={handleLogout}
      >
        Logout
      </button>
    </>
  ) : (
    <button
      className="login-button"
      onClick={() => setPage("login")}
    >
      Login
    </button>
  )}
</div>
      </header>

      <main>
        {page === "login" && (
  <LoginPage
    onRegisterClick={() => setPage("register")}
    onLoginSuccess={() => setPage("store")}
  />
)}

{page === "register" && (
  <RegisterPage
    onBackToLogin={() => setPage("login")}
  />
)}
        {page === "store" && (
          <ProductsPage onCartUpdated={refreshCartCount} />
        )}

        {page === "cart" && (
          <CartPage
            onCheckout={() => setPage("checkout")}
            onCartUpdated={refreshCartCount}
          />
        )}

        {page === "checkout" && (
          <CheckoutPage
            onBack={() => setPage("cart")}
            onOrderCompleted={() => {
              setPage("store");
              void refreshCartCount();
            }}
          />
        )}

        {page === "orders" && <OrderHistoryPage />}
      </main>

      <footer>
        <p>IPL Merchandise Store · Demo project</p>
      </footer>
    </div>
  );
}

export default App;
