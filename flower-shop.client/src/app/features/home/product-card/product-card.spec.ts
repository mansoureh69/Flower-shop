import { TestBed } from '@angular/core/testing';

import { PRODUCT_IMAGE_FALLBACK, ProductDto } from '../../../core/products/product.models';
import { ProductCard } from './product-card';

const product: ProductDto = {
  id: 'product-1',
  name: 'Garden Blush',
  description: 'A textured pink bouquet.',
  price: 75,
  currency: 'USD',
  categoryId: 'category-1',
  isAvailable: true,
  createdAt: '2026-08-01T12:00:00Z',
  images: [{ id: 'image-1', url: '/garden-blush.jpg', isPrimary: true }],
};

describe('ProductCard', () => {
  it('renders product content and formats its currency', async () => {
    const fixture = TestBed.createComponent(ProductCard);
    fixture.componentRef.setInput('product', product);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('h3')?.textContent).toContain('Garden Blush');
    expect(element.querySelector('.product-card__price')?.textContent).toContain('$75');
    expect(element.querySelector('img')?.getAttribute('src')).toBe('/garden-blush.jpg');
  });

  it('replaces a broken remote image with the local fallback', async () => {
    const fixture = TestBed.createComponent(ProductCard);
    fixture.componentRef.setInput('product', product);
    await fixture.whenStable();

    const image = (fixture.nativeElement as HTMLElement).querySelector('img');
    image?.dispatchEvent(new Event('error'));

    expect(image?.src.endsWith(PRODUCT_IMAGE_FALLBACK)).toBe(true);
  });
});
