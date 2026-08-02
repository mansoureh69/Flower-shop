import { Routes } from '@angular/router';

import { HomePage } from './features/home/home-page/home-page';
import { StorefrontShell } from './layout/storefront-shell/storefront-shell';

export const routes: Routes = [
  {
    path: '',
    component: StorefrontShell,
    children: [
      {
        path: '',
        component: HomePage,
        title: 'Sweet Flower Shop | Thoughtful flowers, beautifully delivered',
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
