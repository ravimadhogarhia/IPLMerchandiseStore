
import { useCallback, useEffect, useState } from "react";
import {
  getCart,
  updateCartItem,
  removeCartItem,
} from "../services/cartService";
import { DEMO_CUSTOMER_ID } from "../config";
import type { Cart } from "../types/cart";

interface CartPageProps {
  onCheckout: () => void;
  onCartUpdated: () => void | Promise<void>;
}

function CartPage({ onCheckout, onCartUpdated }: CartPageProps) {
  const [cart, setCart] = useState<Cart | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadCart = useCallback(async () => {
    try {
      setLoading(true);
      setError("");

      const result = await getCart(DEMO_CUSTOMER_ID);
      setCart(result);
      void onCartUpdated();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Unable to load cart."
      );
    } finally {
      setLoading(false);
    }
  }, [onCartUpdated]);

  useEffect(() => {
    void loadCart();
  }, [loadCart]);

  async function changeQuantity(
    franchiseId: string,
    productId: string,
    quantity: number
  ) {
    if (quantity < 1 || quantity > 99) return;

    try {
      setError("");

      const updatedCart = await updateCartItem(
        DEMO_CUSTOMER_ID,
        franchiseId,
        productId,
        { quantity }
      );

      setCart(updatedCart);
      void onCartUpdated();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Unable to update quantity."
      );
    }
  }

  async function deleteItem(franchiseId: string, productId: string) {
    try {
      setError("");

      await removeCartItem(
        DEMO_CUSTOMER_ID,
        franchiseId,
        productId
      );

      await loadCart();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Unable to remove item."
      );
    }
  }

  if (loading) return <p className="cart-message">Loading cart...</p>;

  if (!cart) {
    return <p className="cart-message">{error || "Unable to load cart."}</p>;
  }

  return (
    <section className="cart-page">
      <h2>My Shopping Cart</h2>

      {error && <p className="error-message">{error}</p>}

      {cart.items.length === 0 ? (
        <p>Your shopping cart is empty.</p>
      ) : (
        <div className="cart-layout">
          <div className="cart-items">
            {cart.items.map((item) => (
              <article
                className="cart-item"
                key={`${item.franchiseId}-${item.productId}`}
              >
                <div className="cart-product-image">
                  {item.imageUrl ? (
                    <img src={item.imageUrl} alt={item.productName} />
                  ) : (
                    <span>IPL STORE</span>
                  )}
                </div>

                <div className="cart-item-details">
                  <h3>{item.productName}</h3>
                  <p>{item.franchiseId}</p>
                  <p>₹{item.unitPrice.toLocaleString("en-IN")}</p>

                  <div className="quantity-control">
                    <button
                      disabled={item.quantity <= 1}
                      onClick={() =>
                        void changeQuantity(
                          item.franchiseId,
                          item.productId,
                          item.quantity - 1
                        )
                      }
                    >
                      −
                    </button>

                    <span>{item.quantity}</span>

                    <button
                      disabled={item.quantity >= 99}
                      onClick={() =>
                        void changeQuantity(
                          item.franchiseId,
                          item.productId,
                          item.quantity + 1
                        )
                      }
                    >
                      +
                    </button>
                  </div>

                  <button
                    className="remove-button"
                    onClick={() =>
                      void deleteItem(item.franchiseId, item.productId)
                    }
                  >
                    Remove
                  </button>
                </div>

                <strong>
                  ₹{item.lineTotal.toLocaleString("en-IN")}
                </strong>
              </article>
            ))}
          </div>

          <aside className="cart-summary">
            <h3>Order Summary</h3>

            <div className="summary-row">
              <span>Subtotal</span>
              <strong>
                ₹{cart.subtotal.toLocaleString("en-IN")}
              </strong>
            </div>

            <p>Shipping charges will be calculated during checkout.</p>

            <button
              className="checkout-button"
              onClick={onCheckout}
              disabled={cart.items.length === 0}
            >
              Proceed to Checkout
            </button>
          </aside>
        </div>
      )}
    </section>
  );
}

export default CartPage;
