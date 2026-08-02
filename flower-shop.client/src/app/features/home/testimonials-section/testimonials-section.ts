import { Component, input } from '@angular/core';

import { Testimonial } from '../home.models';

@Component({
  selector: 'app-testimonials-section',
  templateUrl: './testimonials-section.html',
  styleUrl: './testimonials-section.css',
})
export class TestimonialsSection {
  readonly testimonials = input.required<readonly Testimonial[]>();
}
