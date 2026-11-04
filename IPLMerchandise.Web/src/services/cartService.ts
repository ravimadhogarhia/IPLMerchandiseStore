
import { apiRequest } from "./apiClient";
import type {
  Cart,
  AddToCartRequest,
  UpdateCartItemRequest
} from "../types/cart";

export async function getCart(customerId: string): Promise<Cart> {
  return apiRequest<Cart>(
    `/api/customers/${encodeURIComponent(customerId)}/cart`
  );
}

export async function addToCart(
  customerId: string,
  request: AddToCartRequest
): Promise<Cart> {
  return apiRequest<Cart>(
    `/api/customers/${encodeURIComponent(customerId)}/cart/items`,
    {
      method: "POST",
      body: JSON.stringify(request)
    }
  );
}

export async function updateCartItem(
  customerId: string,
  franchiseId: string,
  productId: string,
  request: UpdateCartItemRequest
): Promise<Cart> {
  return apiRequest<Cart>(
    `/api/customers/${encodeURIComponent(customerId)}/cart/items/${encodeURIComponent(franchiseId)}/${encodeURIComponent(productId)}`,
    {
      method: "PUT",
      body: JSON.stringify(request)
    }
  );
}

export async function removeCartItem(
  customerId: string,
  franchiseId: string,
  productId: string
): Promise<void> {
  return apiRequest<void>(
    `/api/customers/${encodeURIComponent(customerId)}/cart/items/${encodeURIComponent(franchiseId)}/${encodeURIComponent(productId)}`,
    {
      method: "DELETE"
    }
  );
}
