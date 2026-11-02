
export interface Product {
  id: string;
  name: string;
  description: string;
  franchiseId: string;
  franchiseName: string;
  category: string;
  price: number;
  currency: string;
  imageUrl: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  pageSize: number;
  continuationToken?: string | null;
  hasMoreResults?: boolean;
}
