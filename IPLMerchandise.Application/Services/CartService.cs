
using IPLMerchandise.Application.DTOs.Carts;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Services;

public sealed class CartService : ICartService
{
    private const int MaxQuantityPerItem = 99;

    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
    }

    // GET CART
    public async Task<CartDto> GetCartAsync(
        string customerId,
        CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);

        var cart = await _cartRepository.GetByCustomerIdAsync(
            customerId, cancellationToken);

        return MapToDto(cart ?? CreateEmptyCart(customerId));
    }

    // ADD ITEM TO CART
    public async Task<CartDto> AddItemAsync(
        string customerId,
        AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);

        if (request.Quantity <= 0 ||
            request.Quantity > MaxQuantityPerItem)
        {
            throw new ArgumentException(
                $"Quantity must be between 1 and {MaxQuantityPerItem}.");
        }

        if (string.IsNullOrWhiteSpace(request.FranchiseId) ||
            string.IsNullOrWhiteSpace(request.ProductId))
        {
            throw new ArgumentException(
                "Franchise ID and Product ID are required.");
        }

        var franchiseId = request.FranchiseId.Trim().ToUpperInvariant();
        var productId = request.ProductId.Trim();

        // Validate product
        var product = await _productRepository.GetByIdAsync(
            franchiseId,
            productId,
            cancellationToken);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        if (!product.IsActive)
            throw new InvalidOperationException(
                "This product is currently unavailable.");

        if (product.Price <= 0)
            throw new InvalidOperationException(
                "Product has an invalid price.");

        // Validate inventory
        var inventory = await _inventoryRepository.GetByProductAsync(
            franchiseId,
            productId,
            cancellationToken);

        if (inventory is null)
            throw new InvalidOperationException(
                "Inventory is not available for this product.");

        var inStockQuantity = GetInStockQuantity(inventory);

        if (inStockQuantity <= 0)
            throw new InvalidOperationException(
                "This product is out of stock.");

        // Retrieve existing cart
        var cart = await _cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        cart ??= CreateEmptyCart(customerId);

        if (cart.Items.Count > 0 &&
            !string.Equals(
                cart.Currency,
                product.Currency,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Products with different currencies cannot be added to the same cart.");
        }

        var existingItem = cart.Items.FirstOrDefault(item =>
            item.FranchiseId == franchiseId &&
            item.ProductId == productId);

        var existingQuantity = existingItem?.Quantity ?? 0;

        // Validate total requested quantity against available stock
        if (existingQuantity + request.Quantity > inStockQuantity)
        {
            throw new InvalidOperationException(
                $"Only {inStockQuantity} units are currently available.");
        }

        cart.Currency = product.Currency;

        if (existingItem is not null)
        {
            if (existingItem.Quantity + request.Quantity >
                MaxQuantityPerItem)
            {
                throw new ArgumentException(
                    $"Maximum quantity per product is {MaxQuantityPerItem}.");
            }

            existingItem.Quantity += request.Quantity;
            existingItem.ProductName = product.Name;
            existingItem.ImageUrl = product.ImageUrl;
            existingItem.UnitPrice = product.Price;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                FranchiseId = franchiseId,
                ProductId = productId,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _cartRepository.UpsertAsync(
            cart, cancellationToken);

        return MapToDto(cart);
    }

    // UPDATE CART ITEM QUANTITY
    public async Task<CartDto> UpdateItemAsync(
        string customerId,
        string franchiseId,
        string productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);
        ValidateProductIdentifiers(franchiseId, productId);

        if (request.Quantity <= 0 ||
            request.Quantity > MaxQuantityPerItem)
        {
            throw new ArgumentException(
                $"Quantity must be between 1 and {MaxQuantityPerItem}.");
        }

        franchiseId = franchiseId.Trim().ToUpperInvariant();
        productId = productId.Trim();

        var cart = await _cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (cart is null)
            throw new KeyNotFoundException("Cart not found.");

        var item = cart.Items.FirstOrDefault(x =>
            x.FranchiseId == franchiseId &&
            x.ProductId == productId);

        if (item is null)
            throw new KeyNotFoundException(
                "Product was not found in the cart.");

        // Validate inventory
        var inventory = await _inventoryRepository.GetByProductAsync(
            franchiseId,
            productId,
            cancellationToken);

        if (inventory is null)
            throw new InvalidOperationException(
                "Inventory is not available for this product.");

        var inStockQuantity = GetInStockQuantity(inventory);

        if (request.Quantity > inStockQuantity)
        {
            throw new InvalidOperationException(
                $"Only {inStockQuantity} units are currently available.");
        }

        item.Quantity = request.Quantity;
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _cartRepository.UpsertAsync(
            cart, cancellationToken);

        return MapToDto(cart);
    }

    // REMOVE ITEM FROM CART
    public async Task RemoveItemAsync(
        string customerId,
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);
        ValidateProductIdentifiers(franchiseId, productId);

        franchiseId = franchiseId.Trim().ToUpperInvariant();
        productId = productId.Trim();

        var cart = await _cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (cart is null)
            throw new KeyNotFoundException("Cart not found.");

        var item = cart.Items.FirstOrDefault(x =>
            x.FranchiseId == franchiseId &&
            x.ProductId == productId);

        if (item is null)
            throw new KeyNotFoundException(
                "Product was not found in the cart.");

        cart.Items.Remove(item);
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _cartRepository.UpsertAsync(
            cart, cancellationToken);
    }

    // CALCULATE AVAILABLE STOCK
    private static int GetInStockQuantity(Inventory inventory)
    {
        if (inventory.AvailableQuantity < 0 ||
            inventory.ReservedQuantity < 0 ||
            inventory.ReservedQuantity > inventory.AvailableQuantity)
        {
            throw new InvalidOperationException(
                "Inventory data is invalid.");
        }

        return inventory.AvailableQuantity -
               inventory.ReservedQuantity;
    }

    // CREATE EMPTY CART
    private static Cart CreateEmptyCart(string customerId)
    {
        return new Cart
        {
            Id = customerId,
            CustomerId = customerId,
            Currency = "INR",
            Items = new List<CartItem>(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    // MAP DOMAIN ENTITY TO DTO
    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(item => new CartItemDto
        {
            ProductId = item.ProductId,
            FranchiseId = item.FranchiseId,
            ProductName = item.ProductName,
            ImageUrl = item.ImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        }).ToList();

        return new CartDto
        {
            CustomerId = cart.CustomerId,
            Items = items,
            Currency = cart.Currency,
            Subtotal = items.Sum(x => x.LineTotal)
        };
    }

    // VALIDATE CUSTOMER
    private static void ValidateCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.");
    }

    // VALIDATE PRODUCT IDENTIFIERS
    private static void ValidateProductIdentifiers(
        string franchiseId,
        string productId)
    {
        if (string.IsNullOrWhiteSpace(franchiseId) ||
            string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException(
                "Franchise ID and Product ID are required.");
        }
    }
}
