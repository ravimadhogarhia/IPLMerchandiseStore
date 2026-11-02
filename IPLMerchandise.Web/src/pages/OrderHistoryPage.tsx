
import { useEffect, useState } from "react";
import { getCustomerOrders } from "../services/orderService";
import { DEMO_CUSTOMER_ID } from "../config";
import type { Order } from "../types/order";

function OrderHistoryPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadOrders() {
      try {
        setLoading(true);
        const result = await getCustomerOrders(DEMO_CUSTOMER_ID);

        if (!cancelled) {
          setOrders(result);
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error ? err.message : "Unable to load orders."
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadOrders();

    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) return <p className="cart-message">Loading orders...</p>;

  if (error) return <p className="error-message">{error}</p>;

  return (
    <section className="orders-page">
      <h2>My Orders</h2>

      {orders.length === 0 ? (
        <p>You haven't placed any orders yet.</p>
      ) : (
        <div className="orders-list">
          {orders.map((order) => (
            <article className="order-card" key={order.id}>
              <div>
                <p className="order-label">ORDER NUMBER</p>
                <strong>{order.orderNumber}</strong>
              </div>

              <div>
                <p className="order-label">ORDER DATE</p>
                <span>
                  {new Date(order.createdAtUtc).toLocaleDateString("en-IN")}
                </span>
              </div>

              <div>
                <p className="order-label">TOTAL</p>
                <strong>
                  ₹{order.totalAmount.toLocaleString("en-IN")}
                </strong>
              </div>

              <div>
                <p className="order-label">STATUS</p>
                <span className="order-status">{order.status}</span>
              </div>

              <button
                className="view-order-button"
                onClick={() => setSelectedOrder(order)}
              >
                View Details
              </button>
            </article>
          ))}
        </div>
      )}

      {selectedOrder && (
        <div className="order-details">
          <div className="order-details-header">
            <h3>Order Details</h3>
            <button onClick={() => setSelectedOrder(null)}>
              Close
            </button>
          </div>

          <p>
            Order Number: <strong>{selectedOrder.orderNumber}</strong>
          </p>
          <p>
            Order Status: <strong>{selectedOrder.status}</strong>
          </p>
          <p>
            Payment Status: <strong>{selectedOrder.paymentStatus}</strong>
          </p>

          <h4>Products</h4>

          {selectedOrder.items.map((item) => (
            <div
              className="order-detail-item"
              key={`${item.franchiseId}-${item.productId}`}
            >
              <span>
                {item.productName} × {item.quantity}
              </span>
              <strong>
                ₹{item.lineTotal.toLocaleString("en-IN")}
              </strong>
            </div>
          ))}

          <div className="order-detail-item">
            <span>Subtotal</span>
            <strong>
              ₹{selectedOrder.subtotal.toLocaleString("en-IN")}
            </strong>
          </div>

          <div className="order-detail-item">
            <span>Shipping</span>
            <strong>
              ₹{selectedOrder.shippingCharges.toLocaleString("en-IN")}
            </strong>
          </div>

          <div className="order-detail-item order-total">
            <span>Total</span>
            <strong>
              ₹{selectedOrder.totalAmount.toLocaleString("en-IN")}
            </strong>
          </div>

          <h4>Shipping Address</h4>
          <p>{selectedOrder.shippingAddress.fullName}</p>
          <p>{selectedOrder.shippingAddress.addressLine1}</p>
          {selectedOrder.shippingAddress.addressLine2 && (
            <p>{selectedOrder.shippingAddress.addressLine2}</p>
          )}
          <p>
            {selectedOrder.shippingAddress.city},{" "}
            {selectedOrder.shippingAddress.state} -{" "}
            {selectedOrder.shippingAddress.postalCode}
          </p>
          <p>{selectedOrder.shippingAddress.country}</p>
        </div>
      )}
    </section>
  );
}

export default OrderHistoryPage;
