import { NgOptimizedImage } from '@angular/common';
import { Component, computed, signal } from '@angular/core';

interface CarouselImage {
  readonly src: string;
  readonly alt: string;
}

interface Service {
  readonly number: string;
  readonly title: string;
  readonly description: string;
  readonly image: string;
  readonly alt: string;
  readonly imageClass: string;
}

@Component({
  selector: 'app-root',
  imports: [NgOptimizedImage],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly carouselImages = signal<readonly CarouselImage[]>([
    { src: '/assets/flowers/carousel-pink-roses.png', alt: 'Pink roses' },
    { src: '/assets/flowers/carousel-white-rose.png', alt: 'White rose' },
    { src: '/assets/flowers/carousel-pink-blooms.png', alt: 'Pink rose blooms' },
    { src: '/assets/flowers/carousel-white-tulips.png', alt: 'White tulips' },
    { src: '/assets/flowers/carousel-orchid.png', alt: 'Orchid' },
  ]);

  protected readonly carouselImageCount = computed(() => this.carouselImages().length);

  protected readonly services = signal<readonly Service[]>([
    {
      number: '1',
      title: 'Floral installations',
      description: 'Living art for homes, businesses, and events.',
      image: '/assets/flowers/service-installations.png',
      alt: 'Person carrying a bouquet of flowers',
      imageClass: 'service__image--rounded',
    },
    {
      number: '2',
      title: 'Native plant arrangements',
      description:
        'Whether it’s a private retreat or a public space, we craft floral experiences that bloom beyond expectations.',
      image: '/assets/flowers/service-native-arrangements.png',
      alt: 'Florist making a bouquet',
      imageClass: 'service__image--soft',
    },
    {
      number: '3',
      title: 'Custom floral concepts',
      description:
        'Your vision, our blooms. We build arrangements that are both personal and exquisitely simple. Whether it’s a private retreat or a public space, we craft floral experiences that bloom beyond expectations.',
      image: '/assets/flowers/service-custom-concepts.png',
      alt: 'Pink tulips in a vase',
      imageClass: 'service__image--rounded',
    },
  ]);
}
