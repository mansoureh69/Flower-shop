import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import * as axe from 'axe-core';

import { App } from './app';
import { routes } from './app.routes';
import { ProductDto } from './core/products/product.models';
import { ProductsApi } from './core/products/products.api';

class FakeProductsApi {
  readonly products = signal<readonly ProductDto[]>([]).asReadonly();
  readonly isLoading = signal(false).asReadonly();
  readonly error = signal<unknown>(undefined).asReadonly();

  reload(): void {}
}

describe('App', () => {
  let fixture: ComponentFixture<App>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter(routes),
        { provide: ProductsApi, useClass: FakeProductsApi },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    await TestBed.inject(Router).navigateByUrl('/');
    await fixture.whenStable();
  });

  it('creates the routed storefront homepage', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('app-storefront-shell')).toBeTruthy();
    expect(element.querySelector('app-home-page')).toBeTruthy();
  });

  it('renders every homepage section and the reusable shell', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('app-site-header')).toBeTruthy();
    expect(element.querySelector('#occasions')).toBeTruthy();
    expect(element.querySelector('#fresh-picks')).toBeTruthy();
    expect(element.querySelector('#delivery')).toBeTruthy();
    expect(element.querySelector('#story')).toBeTruthy();
    expect(element.querySelector('app-site-footer')).toBeTruthy();
  });

  it('provides working page anchors for primary navigation', () => {
    const element = fixture.nativeElement as HTMLElement;
    const navigation = element.querySelector('.desktop-navigation');
    const destinations = Array.from(navigation?.querySelectorAll('a') ?? []).map((anchor) =>
      anchor.getAttribute('href'),
    );

    expect(destinations).toEqual(['#fresh-picks', '#occasions', '#delivery', '#story']);
    for (const destination of destinations) {
      expect(element.querySelector(destination ?? '')).toBeTruthy();
    }
  });

  it('exposes one primary heading with meaningful section headings', () => {
    const element = fixture.nativeElement as HTMLElement;
    const headingText = Array.from(element.querySelectorAll('h1, h2')).map((heading) =>
      heading.textContent?.trim(),
    );

    expect(element.querySelectorAll('h1')).toHaveLength(1);
    expect(headingText).toContain('Send a little beauty into someone’s day.');
    expect(headingText).toContain('Flowers worth making room for.');
    expect(headingText).toContain('Less catalogue. More character.');
  });

  it('has no automated WCAG AA accessibility violations', async () => {
    const results = await axe.run(fixture.nativeElement as HTMLElement, {
      runOnly: {
        type: 'tag',
        values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'],
      },
    });

    expect(results.violations).toEqual([]);
  });
});
