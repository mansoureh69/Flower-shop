import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { ProductDto } from './product.models';
import { ProductsApi } from './products.api';

const products: readonly ProductDto[] = [
  {
    id: 'product-1',
    name: 'Garden Blush',
    description: 'A textured pink bouquet.',
    price: 75,
    currency: 'USD',
    categoryId: 'category-1',
    isAvailable: true,
    createdAt: '2026-08-01T12:00:00Z',
    images: [],
  },
];

describe('ProductsApi', () => {
  let productsApi: ProductsApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    productsApi = TestBed.inject(ProductsApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('loads available products from the server endpoint', async () => {
    TestBed.tick();
    const request = httpTesting.expectOne('/api/products?availableOnly=true');
    expect(request.request.method).toBe('GET');

    request.flush(products);
    await TestBed.inject(ApplicationRef).whenStable();

    expect(productsApi.products()).toEqual(products);
    expect(productsApi.error()).toBeUndefined();
  });

  it('can reload the available product collection', async () => {
    TestBed.tick();
    httpTesting.expectOne('/api/products?availableOnly=true').flush([]);
    await TestBed.inject(ApplicationRef).whenStable();

    productsApi.reload();
    TestBed.tick();
    httpTesting.expectOne('/api/products?availableOnly=true').flush(products);
    await TestBed.inject(ApplicationRef).whenStable();

    expect(productsApi.products()).toEqual(products);
  });
});
