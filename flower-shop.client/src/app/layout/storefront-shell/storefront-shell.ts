import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { STOREFRONT_NAVIGATION } from '../navigation.models';
import { SiteFooter } from '../site-footer/site-footer';
import { SiteHeader } from '../site-header/site-header';

@Component({
  selector: 'app-storefront-shell',
  imports: [RouterOutlet, SiteFooter, SiteHeader],
  templateUrl: './storefront-shell.html',
  styleUrl: './storefront-shell.css',
})
export class StorefrontShell {
  protected readonly navigationItems = STOREFRONT_NAVIGATION;
}
