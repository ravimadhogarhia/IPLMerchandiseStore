
import type {
  CheckoutRequest,
  Order,
  PaymentRequest,
} from "../types/order";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function checkout(
  customerId: string,
  request: CheckoutRequest
): Promise<Order> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/orders/checkout`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }
  );

  if (!response.ok) {
    throw new Error(await response.text());
  }

  return response.json();
}

export async function getCustomerOrders(
  customerId: string
): Promise<Order[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/orders`
  );

  if (!response.ok) {
    throw new Error("Unable to retrieve orders.");
  }

  return response.json();
}

export async function processPayment(
  customerId: string,
  orderId: string,
  request: PaymentRequest
): Promise<Order> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/orders/${encodeURIComponent(orderId)}/payment`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }
  );

  if (!response.ok) {
    throw new Error(await response.text());
  }

  return response.json();
}
