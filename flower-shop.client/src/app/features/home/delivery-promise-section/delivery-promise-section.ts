import { Component, input } from '@angular/core';

import { DeliveryBenefit } from '../home.models';

@Component({
  selector: 'app-delivery-promise-section',
  templateUrl: './delivery-promise-section.html',
  styleUrl: './delivery-promise-section.css',
})
export class DeliveryPromiseSection {
  readonly benefits = input.required<readonly DeliveryBenefit[]>();
}
