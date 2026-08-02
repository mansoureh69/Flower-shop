import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductDto } from '../../../core/products/product.models';
import { ProductsApi } from '../../../core/products/products.api';
import { FeaturedProductsSection } from './featured-products-section';

function createProduct(id: number, createdAt: string, isAvailable = true): ProductDto {
  return {
    id: `product-${id}`,
    name: `Bouquet ${id}`,
    description: `Description ${id}`,
    price: 60 + id,
    currency: 'USD',
    categoryId: 'category-1',
    isAvailable,
    createdAt,
    images: [],
  };
}

class FakeProductsApi {
  readonly products = signal<readonly ProductDto[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<unknown>(undefined);
  reloadCount = 0;

  reload(): void {
    this.reloadCount += 1;
  }
}

describe('FeaturedProductsSection', () => {
  let fixture: ComponentFixture<FeaturedProductsSection>;
  let productsApi: FakeProductsApi;

  beforeEach(async () => {
    productsApi = new FakeProductsApi();

    await TestBed.configureTestingModule({
      imports: [FeaturedProductsSection],
      providers: [{ provide: ProductsApi, useValue: productsApi }],
    }).compileComponents();

    fixture = TestBed.createComponent(FeaturedProductsSection);
  });

  it('shows at most four newest available products', async () => {
    productsApi.products.set([
      createProduct(1, '2026-08-01T00:00:00Z'),
      createProduct(2, '2026-08-02T00:00:00Z'),
      createProduct(3, '2026-08-03T00:00:00Z'),
      createProduct(4, '2026-08-04T00:00:00Z'),
      createProduct(5, '2026-08-05T00:00:00Z'),
      createProduct(6, '2026-08-06T00:00:00Z', false),
    ]);
    await fixture.whenStable();

    const cards = (fixture.nativeElement as HTMLElement).querySelectorAll('app-product-card');
    const names = Array.from(cards).map((card) => card.querySelector('h3')?.textContent?.trim());

    expect(cards).toHaveLength(4);
    expect(names).toEqual(['Bouquet 5', 'Bouquet 4', 'Bouquet 3', 'Bouquet 2']);
  });

  it('shows an accessible loading state', async () => {
    productsApi.isLoading.set(true);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[role="status"]')?.textContent).toContain(
      'Arranging the latest flower picks',
    );
    expect(element.querySelectorAll('.skeleton-card')).toHaveLength(4);
  });

  it('shows an empty state when no available products exist', async () => {
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Fresh picks are being arranged',
    );
  });

  it('shows a retryable error state', async () => {
    productsApi.error.set(new Error('Unavailable'));
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    const retryButton = element.querySelector<HTMLButtonElement>('button');
    expect(element.querySelector('[role="alert"]')).toBeTruthy();

    retryButton?.click();
    expect(productsApi.reloadCount).toBe(1);
  });
});
