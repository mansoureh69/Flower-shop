export interface ProductImageDto {
  readonly id: string;
  readonly url: string;
  readonly isPrimary: boolean;
}

export interface ProductDto {
  readonly id: string;
  readonly name: string;
  readonly description: string;
  readonly price: number;
  readonly currency: string;
  readonly categoryId: string;
  readonly isAvailable: boolean;
  readonly createdAt: string;
  readonly images: readonly ProductImageDto[];
}

export const PRODUCT_IMAGE_FALLBACK = '/assets/flowers/carousel-pink-roses.png';

export function selectProductImage(product: ProductDto): string {
  return (
    product.images.find((image) => image.isPrimary)?.url ??
    product.images[0]?.url ??
    PRODUCT_IMAGE_FALLBACK
  );
}
