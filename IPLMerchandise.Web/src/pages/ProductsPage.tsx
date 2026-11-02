
import { useEffect, useState } from "react";
import { getProducts } from "../services/productService";
import { addToCart } from "../services/cartService";
import { DEMO_CUSTOMER_ID } from "../config";
import type { Product } from "../types/product";

const franchises = [
  { id: "ALL", name: "All Teams" },
  { id: "MI", name: "Mumbai Indians" },
  { id: "CSK", name: "Chennai Super Kings" },
  { id: "RCB", name: "Royal Challengers Bengaluru" },
  { id: "KKR", name: "Kolkata Knight Riders" },
  { id: "SRH", name: "Sunrisers Hyderabad" },
  { id: "RR", name: "Rajasthan Royals" },
  { id: "DC", name: "Delhi Capitals" },
  { id: "GT", name: "Gujarat Titans" },
];

interface ProductsPageProps {
  onCartUpdated: () => void | Promise<void>;
}

function ProductsPage({ onCartUpdated }: ProductsPageProps) {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [franchiseId, setFranchiseId] = useState("ALL");

  const [addingProductId, setAddingProductId] = useState<string | null>(null);
  const [cartMessage, setCartMessage] = useState("");
  const [cartError, setCartError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadProducts() {
      try {
        setLoading(true);
        setError("");

        const result = await getProducts({
          search: search.trim() || undefined,
          franchiseId: franchiseId === "ALL" ? undefined : franchiseId,
          pageSize: 100,
        });

        if (!cancelled) {
          setProducts(result.items ?? []);
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error ? err.message : "Unable to load products."
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadProducts();

    return () => {
      cancelled = true;
    };
  }, [search, franchiseId]);

  async function handleAddToCart(product: Product) {
    try {
      setAddingProductId(product.id);
      setCartMessage("");
      setCartError("");

      await addToCart(DEMO_CUSTOMER_ID, {
        franchiseId: product.franchiseId,
        productId: product.id,
        quantity: 1,
      });
      
      await onCartUpdated();
      
      setCartMessage(`${product.name} added to your cart successfully.`);
    } catch (err) {
      setCartError(
        err instanceof Error ? err.message : "Unable to add product to cart."
      );
    } finally {
      setAddingProductId(null);
    }
  }

  return (
    <section className="catalogue">
      <div className="catalogue-header">
        <div>
          <p className="eyebrow">OFFICIAL-STYLE COLLECTION</p>
          <h2>Explore Merchandise</h2>
          <p>Discover merchandise inspired by your favourite franchises.</p>
        </div>
      </div>

      {cartMessage && (
        <p className="success-message" role="status">
          {cartMessage}
        </p>
      )}

      {cartError && (
        <p className="error-message" role="alert">
          {cartError}
        </p>
      )}

      <div className="catalogue-filters">
        <input
          type="search"
          placeholder="Search products..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />

        <select
          value={franchiseId}
          onChange={(event) => setFranchiseId(event.target.value)}
        >
          {franchises.map((franchise) => (
            <option key={franchise.id} value={franchise.id}>
              {franchise.name}
            </option>
          ))}
        </select>
      </div>

      {loading && <p>Loading products...</p>}

      {error && <p className="error-message">{error}</p>}

      {!loading && !error && products.length === 0 && (
        <p>No products found.</p>
      )}

      <div className="product-grid">
        {products.map((product) => (
          <article
            className="product-card"
            key={`${product.franchiseId}-${product.id}`}
          >
            <div className="product-image">
              {product.imageUrl ? (
                <img src={product.imageUrl} alt={product.name} />
              ) : (
                <div className="image-placeholder">IPL STORE</div>
              )}
            </div>

            <div className="product-details">
              <p className="product-team">{product.franchiseName}</p>
              <h3>{product.name}</h3>
              <p className="product-description">{product.description}</p>

              <p className="product-price">
                {product.currency === "INR" ? "₹" : product.currency}
                {product.price.toLocaleString("en-IN")}
              </p>

              <button
                type="button"
                disabled={addingProductId !== null || !product.isActive}
                onClick={() => void handleAddToCart(product)}
              >
                {addingProductId === product.id
                  ? "Adding..."
                  : !product.isActive
                    ? "Unavailable"
                    : "Add to Cart"}
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}

export default ProductsPage;
