
export interface ShippingAddress {
  fullName: string;
  phoneNumber: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export interface CheckoutRequest {
  idempotencyKey: string;
  shippingAddress: ShippingAddress;
}

export interface OrderItem {
  productId: string;
  franchiseId: string;
  productName: string;
  imageUrl: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  items: OrderItem[];
  subtotal: number;
  shippingCharges: number;
  totalAmount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  shippingAddress: ShippingAddress;
  createdAtUtc: string;
}

export interface PaymentRequest {
  paymentReference: string;
  isSuccessful: boolean;
}
