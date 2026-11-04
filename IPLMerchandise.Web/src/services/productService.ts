
import { apiRequest } from "./apiClient";
import type { Product, PagedResult } from "../types/product";

interface ProductFilters {
  franchiseId?: string;
  category?: string;
  search?: string;
  pageSize?: number;
}

export async function getProducts(
  filters: ProductFilters = {}
): Promise<PagedResult<Product>> {
  const params = new URLSearchParams();

  if (filters.franchiseId) {
    params.set("franchiseId", filters.franchiseId);
  }

  if (filters.category) {
    params.set("category", filters.category);
  }

  if (filters.search) {
    params.set("search", filters.search);
  }

  params.set("pageSize", String(filters.pageSize ?? 100));

  return apiRequest<PagedResult<Product>>(
    `/api/products?${params.toString()}`
  );
}
