import { DeliveryBenefit, OccasionCardContent, Testimonial } from './home.models';

export const OCCASIONS: readonly OccasionCardContent[] = [
  {
    id: 'birthday',
    eyebrow: 'Make a wish',
    title: 'Birthday blooms',
    description: 'Joyful color for their next trip around the sun.',
    image: '/assets/flowers/carousel-pink-blooms.png',
    imageAlt: 'A generous arrangement of layered pink blooms',
  },
  {
    id: 'romance',
    eyebrow: 'Say it warmly',
    title: 'Love notes',
    description: 'Soft, expressive stems for your favorite person.',
    image: '/assets/flowers/carousel-white-tulips.png',
    imageAlt: 'White tulips gathered into an elegant bouquet',
  },
  {
    id: 'gratitude',
    eyebrow: 'A thoughtful thanks',
    title: 'Gratitude',
    description: 'A beautiful reminder that their kindness mattered.',
    image: '/assets/flowers/carousel-orchid.png',
    imageAlt: 'A delicate orchid in soft natural light',
  },
  {
    id: 'just-because',
    eyebrow: 'No reason needed',
    title: 'Just because',
    description: 'Everyday flowers for an unexpectedly lovely moment.',
    image: '/assets/flowers/purple-flowers.png',
    imageAlt: 'Airy purple flowers against a quiet background',
  },
];

export const DELIVERY_BENEFITS: readonly DeliveryBenefit[] = [
  {
    number: '01',
    title: 'Chosen with intention',
    description: 'A focused collection makes finding the right feeling beautifully simple.',
  },
  {
    number: '02',
    title: 'Designed to delight',
    description: 'Each bouquet balances color, texture, and shape for an artful first impression.',
  },
  {
    number: '03',
    title: 'Gifting made personal',
    description: 'Thoughtful details help your flowers feel unmistakably meant for them.',
  },
];

export const TESTIMONIALS: readonly Testimonial[] = [
  {
    id: 'maya',
    customerName: 'Maya R.',
    headline: 'The easiest lovely decision',
    quote:
      'The collection felt considered instead of overwhelming, and the bouquet looked wonderfully special.',
  },
  {
    id: 'leila',
    customerName: 'Leila S.',
    headline: 'A gift that felt personal',
    quote:
      'Every detail made the flowers feel like they had been chosen just for the moment and the person.',
  },
  {
    id: 'daniel',
    customerName: 'Daniel K.',
    headline: 'Beautiful from first click',
    quote:
      'Finding the right arrangement was simple, warm, and every bit as thoughtful as the gift itself.',
  },
];
