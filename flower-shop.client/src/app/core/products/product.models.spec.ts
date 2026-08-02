import {
  PRODUCT_IMAGE_FALLBACK,
  ProductDto,
  selectProductImage,
} from './product.models';

const product: ProductDto = {
  id: 'product-1',
  name: 'Garden Blush',
  description: 'A textured pink bouquet.',
  price: 75,
  currency: 'USD',
  categoryId: 'category-1',
  isAvailable: true,
  createdAt: '2026-08-01T12:00:00Z',
  images: [
    { id: 'secondary', url: '/secondary.jpg', isPrimary: false },
    { id: 'primary', url: '/primary.jpg', isPrimary: true },
  ],
};

describe('selectProductImage', () => {
  it('selects the primary product image', () => {
    expect(selectProductImage(product)).toBe('/primary.jpg');
  });

  it('uses the first image when no image is primary', () => {
    const withoutPrimary = {
      ...product,
      images: product.images.map((image) => ({ ...image, isPrimary: false })),
    };

    expect(selectProductImage(withoutPrimary)).toBe('/secondary.jpg');
  });

  it('uses the local flower fallback when a product has no images', () => {
    expect(selectProductImage({ ...product, images: [] })).toBe(PRODUCT_IMAGE_FALLBACK);
  });
});
