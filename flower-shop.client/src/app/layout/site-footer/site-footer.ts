import { Component, input } from '@angular/core';

import { NavItem } from '../navigation.models';

@Component({
  selector: 'app-site-footer',
  templateUrl: './site-footer.html',
  styleUrl: './site-footer.css',
})
export class SiteFooter {
  readonly navigationItems = input.required<readonly NavItem[]>();
  protected readonly currentYear = new Date().getFullYear();
}
