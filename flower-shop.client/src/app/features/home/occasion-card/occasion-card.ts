import { NgOptimizedImage } from '@angular/common';
import { Component, input } from '@angular/core';

import { OccasionCardContent } from '../home.models';

@Component({
  selector: 'app-occasion-card',
  imports: [NgOptimizedImage],
  templateUrl: './occasion-card.html',
  styleUrl: './occasion-card.css',
})
export class OccasionCard {
  readonly content = input.required<OccasionCardContent>();
}
