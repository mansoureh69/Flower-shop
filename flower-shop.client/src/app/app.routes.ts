import { Routes } from '@angular/router';

import { LoginPage } from './features/auth/login-page/login-page';
import { RegisterPage } from './features/auth/register-page/register-page';
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
      {
        path: 'login',
        component: LoginPage,
        title: 'Login | Sweet Flower Shop',
      },
      {
        path: 'register',
        component: RegisterPage,
        title: 'Register | Sweet Flower Shop',
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
