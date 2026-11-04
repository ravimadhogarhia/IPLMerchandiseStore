
import { apiRequest } from "./apiClient";
import type {
  CheckoutRequest,
  Order,
  PaymentRequest
} from "../types/order";

export async function checkout(
  customerId: string,
  request: CheckoutRequest
): Promise<Order> {
  return apiRequest<Order>(
    `/api/customers/${encodeURIComponent(customerId)}/orders/checkout`,
    {
      method: "POST",
      body: JSON.stringify(request)
    }
  );
}

export async function getCustomerOrders(
  customerId: string
): Promise<Order[]> {
  return apiRequest<Order[]>(
    `/api/customers/${encodeURIComponent(customerId)}/orders`
  );
}

export async function processPayment(
  customerId: string,
  orderId: string,
  request: PaymentRequest
): Promise<Order> {
  return apiRequest<Order>(
    `/api/customers/${encodeURIComponent(customerId)}/orders/${encodeURIComponent(orderId)}/payment`,
    {
      method: "POST",
      body: JSON.stringify(request)
    }
  );
}
