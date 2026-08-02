import { Component } from '@angular/core';

import { BrandStorySection } from '../brand-story-section/brand-story-section';
import { DeliveryPromiseSection } from '../delivery-promise-section/delivery-promise-section';
import { FeaturedProductsSection } from '../featured-products-section/featured-products-section';
import { HeroSection } from '../hero-section/hero-section';
import { DELIVERY_BENEFITS, OCCASIONS, TESTIMONIALS } from '../home.content';
import { OccasionGrid } from '../occasion-grid/occasion-grid';
import { TestimonialsSection } from '../testimonials-section/testimonials-section';

@Component({
  selector: 'app-home-page',
  imports: [
    BrandStorySection,
    DeliveryPromiseSection,
    FeaturedProductsSection,
    HeroSection,
    OccasionGrid,
    TestimonialsSection,
  ],
  templateUrl: './home-page.html',
  styleUrl: './home-page.css',
})
export class HomePage {
  protected readonly occasions = OCCASIONS;
  protected readonly deliveryBenefits = DELIVERY_BENEFITS;
  protected readonly testimonials = TESTIMONIALS;
}
