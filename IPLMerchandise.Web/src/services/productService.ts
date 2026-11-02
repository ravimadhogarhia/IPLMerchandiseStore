
import type { Product, PagedResult } from "../types/product";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface ProductSearchParams {
  franchiseId?: string;
  category?: string;
  search?: string;
  pageSize?: number;
}

export async function getProducts(
  params: ProductSearchParams = {}
): Promise<PagedResult<Product>> {
  const query = new URLSearchParams();

  if (params.franchiseId) query.set("franchiseId", params.franchiseId);
  if (params.category) query.set("category", params.category);
  if (params.search) query.set("search", params.search);

  query.set("pageSize", String(params.pageSize ?? 20));

  const response = await fetch(
    `${API_BASE_URL}/api/products?${query.toString()}`
  );

  if (!response.ok) {
    throw new Error(`Unable to load products. HTTP ${response.status}`);
  }

  return response.json() as Promise<PagedResult<Product>>;
}
