
import type {
  Cart,
  AddToCartRequest,
  UpdateCartItemRequest,
} from "../types/cart";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function getCart(customerId: string): Promise<Cart> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/cart`
  );

  if (!response.ok) {
    throw new Error("Unable to retrieve shopping cart.");
  }

  return response.json();
}

export async function addToCart(
  customerId: string,
  request: AddToCartRequest
): Promise<Cart> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/cart/items`,
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

export async function updateCartItem(
  customerId: string,
  franchiseId: string,
  productId: string,
  request: UpdateCartItemRequest
): Promise<Cart> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/cart/items/${encodeURIComponent(franchiseId)}/${encodeURIComponent(productId)}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }
  );

  if (!response.ok) {
    throw new Error(await response.text());
  }

  return response.json();
}

export async function removeCartItem(
  customerId: string,
  franchiseId: string,
  productId: string
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/customers/${encodeURIComponent(customerId)}/cart/items/${encodeURIComponent(franchiseId)}/${encodeURIComponent(productId)}`,
    { method: "DELETE" }
  );

  if (!response.ok) {
    throw new Error(await response.text());
  }
}
