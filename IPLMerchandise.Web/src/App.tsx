
import { useCallback, useEffect, useState } from "react";
import ProductsPage from "./pages/ProductsPage";
import CartPage from "./pages/CartPage";
import CheckoutPage from "./pages/CheckoutPage";
import OrderHistoryPage from "./pages/OrderHistoryPage";
import { getCart } from "./services/cartService";
import { DEMO_CUSTOMER_ID } from "./config";
import "./styles/App.css";

type Page = "store" | "cart" | "checkout" | "orders";

function App() {
  const [page, setPage] = useState<Page>("store");
  const [cartCount, setCartCount] = useState(0);

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
          <button onClick={() => setPage("store")}>Products</button>
          <button onClick={() => setPage("orders")}>My Orders</button>
        </nav>

        <button
          className="cart-button"
          onClick={() => setPage("cart")}
        >
          My Cart
          <span className="cart-count">{cartCount}</span>
        </button>
      </header>

      <main>
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
