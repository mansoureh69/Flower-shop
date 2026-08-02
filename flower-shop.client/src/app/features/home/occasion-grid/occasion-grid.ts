import { Component, input } from '@angular/core';

import { OccasionCard } from '../occasion-card/occasion-card';
import { OccasionCardContent } from '../home.models';

@Component({
  selector: 'app-occasion-grid',
  imports: [OccasionCard],
  templateUrl: './occasion-grid.html',
  styleUrl: './occasion-grid.css',
})
export class OccasionGrid {
  readonly occasions = input.required<readonly OccasionCardContent[]>();
}
