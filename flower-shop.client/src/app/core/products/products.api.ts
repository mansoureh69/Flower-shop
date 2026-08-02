import { httpResource } from '@angular/common/http';
import { computed, Injectable } from '@angular/core';

import { ProductDto } from './product.models';

const NO_PRODUCTS: readonly ProductDto[] = [];

@Injectable({ providedIn: 'root' })
export class ProductsApi {
  private readonly availableProductsResource = httpResource<readonly ProductDto[]>(
    () => '/api/products?availableOnly=true',
  );

  readonly products = computed(() =>
    this.availableProductsResource.hasValue()
      ? this.availableProductsResource.value()
      : NO_PRODUCTS,
  );
  readonly isLoading = this.availableProductsResource.isLoading;
  readonly error = this.availableProductsResource.error;

  reload(): void {
    this.availableProductsResource.reload();
  }
}
