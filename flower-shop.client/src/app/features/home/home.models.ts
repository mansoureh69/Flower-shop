export interface OccasionCardContent {
  readonly id: string;
  readonly eyebrow: string;
  readonly title: string;
  readonly description: string;
  readonly image: string;
  readonly imageAlt: string;
}

export interface DeliveryBenefit {
  readonly number: string;
  readonly title: string;
  readonly description: string;
}

export interface Testimonial {
  readonly id: string;
  readonly customerName: string;
  readonly headline: string;
  readonly quote: string;
}
