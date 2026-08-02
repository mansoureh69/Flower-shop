import { CurrencyPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

import {
  PRODUCT_IMAGE_FALLBACK,
  ProductDto,
  selectProductImage,
} from '../../../core/products/product.models';

@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard {
  readonly product = input.required<ProductDto>();

  protected readonly imageUrl = computed(() => selectProductImage(this.product()));

  protected useFallbackImage(event: Event): void {
    const image = event.currentTarget;

    if (image instanceof HTMLImageElement && !image.src.endsWith(PRODUCT_IMAGE_FALLBACK)) {
      image.src = PRODUCT_IMAGE_FALLBACK;
    }
  }
}
