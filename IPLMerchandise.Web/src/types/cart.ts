
export interface CartItem {
  productId: string;
  franchiseId: string;
  productName: string;
  imageUrl: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Cart {
  customerId: string;
  items: CartItem[];
  subtotal: number;
  currency: string;
}

export interface AddToCartRequest {
  franchiseId: string;
  productId: string;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}
