import { Component, computed, inject } from '@angular/core';

import { ProductsApi } from '../../../core/products/products.api';
import { ProductCard } from '../product-card/product-card';

@Component({
  selector: 'app-featured-products-section',
  imports: [ProductCard],
  templateUrl: './featured-products-section.html',
  styleUrl: './featured-products-section.css',
})
export class FeaturedProductsSection {
  protected readonly productsApi = inject(ProductsApi);

  protected readonly featuredProducts = computed(() =>
    [...this.productsApi.products()]
      .filter((product) => product.isAvailable)
      .sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt))
      .slice(0, 4),
  );

  protected reloadProducts(): void {
    this.productsApi.reload();
  }
}
