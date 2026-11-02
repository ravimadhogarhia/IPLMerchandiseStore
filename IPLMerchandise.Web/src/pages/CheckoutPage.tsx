
import { useState } from "react";
import { checkout, processPayment } from "../services/orderService";
import { DEMO_CUSTOMER_ID } from "../config";
import type { Order, ShippingAddress } from "../types/order";

interface CheckoutPageProps {
  onBack: () => void;
  onOrderCompleted: () => void;
}

const initialAddress: ShippingAddress = {
  fullName: "",
  phoneNumber: "",
  addressLine1: "",
  addressLine2: "",
  city: "",
  state: "",
  postalCode: "",
  country: "India",
};

function CheckoutPage({
  onBack,
  onOrderCompleted,
}: CheckoutPageProps) {
  const [address, setAddress] = useState<ShippingAddress>(initialAddress);
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(false);
  const [paymentLoading, setPaymentLoading] = useState(false);
  const [error, setError] = useState("");

  // Keep the same key if the checkout request needs to be retried.
  const [idempotencyKey] = useState(() => crypto.randomUUID());

  function updateAddress(
    field: keyof ShippingAddress,
    value: string
  ) {
    setAddress((current) => ({
      ...current,
      [field]: value,
    }));
  }

  async function handleCheckout(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    try {
      setLoading(true);
      setError("");

      const createdOrder = await checkout(DEMO_CUSTOMER_ID, {
        idempotencyKey,
        shippingAddress: address,
      });

      setOrder(createdOrder);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Checkout failed."
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleDemoPayment() {
    if (!order) return;

    try {
      setPaymentLoading(true);
      setError("");

      const updatedOrder = await processPayment(
        DEMO_CUSTOMER_ID,
        order.id,
        {
          paymentReference: `DEMO-PAY-${order.id}`,
          isSuccessful: true,
        }
      );

      setOrder(updatedOrder);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Payment processing failed."
      );
    } finally {
      setPaymentLoading(false);
    }
  }

  if (order) {
    const paymentCompleted = order.paymentStatus === "Paid";

    return (
      <section className="checkout-page">
        <div className="checkout-confirmation">
          <h2>
            {paymentCompleted
              ? "Order Confirmed!"
              : "Order Created Successfully"}
          </h2>

          <p>
            Order Number: <strong>{order.orderNumber}</strong>
          </p>

          <p>
            Order Status: <strong>{order.status}</strong>
          </p>

          <p>
            Payment Status: <strong>{order.paymentStatus}</strong>
          </p>

          <h3>
            Total: ₹{order.totalAmount.toLocaleString("en-IN")}
          </h3>

          {!paymentCompleted && (
            <>
              <p>
                This is a demonstration payment. No actual money will be
                charged.
              </p>

              <button
                className="checkout-submit"
                onClick={() => void handleDemoPayment()}
                disabled={paymentLoading}
              >
                {paymentLoading
                  ? "Processing..."
                  : "Simulate Successful Payment"}
              </button>
            </>
          )}

          {paymentCompleted && (
            <button
              className="checkout-submit"
              onClick={onOrderCompleted}
            >
              Continue Shopping
            </button>
          )}
        </div>

        {error && <p className="error-message">{error}</p>}
      </section>
    );
  }

  return (
    <section className="checkout-page">
      <h2>Checkout</h2>
      <p>Enter your shipping address to place your order.</p>

      {error && <p className="error-message">{error}</p>}

      <form onSubmit={handleCheckout} className="checkout-form">
        <label>
          Full Name
          <input
            required
            maxLength={100}
            value={address.fullName}
            onChange={(e) => updateAddress("fullName", e.target.value)}
          />
        </label>

        <label>
          Phone Number
          <input
            required
            type="tel"
            maxLength={20}
            value={address.phoneNumber}
            onChange={(e) => updateAddress("phoneNumber", e.target.value)}
          />
        </label>

        <label>
          Address Line 1
          <input
            required
            maxLength={200}
            value={address.addressLine1}
            onChange={(e) => updateAddress("addressLine1", e.target.value)}
          />
        </label>

        <label>
          Address Line 2 (Optional)
          <input
            maxLength={200}
            value={address.addressLine2}
            onChange={(e) => updateAddress("addressLine2", e.target.value)}
          />
        </label>

        <div className="checkout-form-row">
          <label>
            City
            <input
              required
              maxLength={100}
              value={address.city}
              onChange={(e) => updateAddress("city", e.target.value)}
            />
          </label>

          <label>
            State
            <input
              required
              maxLength={100}
              value={address.state}
              onChange={(e) => updateAddress("state", e.target.value)}
            />
          </label>
        </div>

        <div className="checkout-form-row">
          <label>
            Postal Code
            <input
              required
              maxLength={20}
              value={address.postalCode}
              onChange={(e) => updateAddress("postalCode", e.target.value)}
            />
          </label>

          <label>
            Country
            <input
              required
              maxLength={100}
              value={address.country}
              onChange={(e) => updateAddress("country", e.target.value)}
            />
          </label>
        </div>

        <div className="checkout-actions">
          <button type="button" className="back-button" onClick={onBack}>
            Back to Cart
          </button>

          <button
            type="submit"
            className="checkout-submit"
            disabled={loading}
          >
            {loading ? "Placing Order..." : "Place Order"}
          </button>
        </div>
      </form>
    </section>
  );
}

export default CheckoutPage;
