import { TestBed } from '@angular/core/testing';

import { NavItem } from '../navigation.models';
import { SiteHeader } from './site-header';

const navigationItems: readonly NavItem[] = [
  { label: 'Fresh Picks', fragment: 'fresh-picks' },
  { label: 'Our Story', fragment: 'story' },
];

describe('SiteHeader', () => {
  it('renders the provided navigation destinations', async () => {
    const fixture = TestBed.createComponent(SiteHeader);
    fixture.componentRef.setInput('navigationItems', navigationItems);
    await fixture.whenStable();

    const links = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('.desktop-navigation a'),
    );

    expect(links.map((link) => link.getAttribute('href'))).toEqual(['#fresh-picks', '#story']);
  });

  it('opens and closes the mobile navigation while returning focus', async () => {
    const fixture = TestBed.createComponent(SiteHeader);
    fixture.componentRef.setInput('navigationItems', navigationItems);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    const menuButton = element.querySelector<HTMLButtonElement>('.menu-button');
    const dialog = element.querySelector<HTMLDialogElement>('dialog');

    menuButton?.click();
    await fixture.whenStable();
    expect(dialog?.hasAttribute('open')).toBe(true);
    expect(menuButton?.getAttribute('aria-expanded')).toBe('true');

    dialog?.dispatchEvent(new Event('cancel', { cancelable: true }));
    await fixture.whenStable();
    expect(dialog?.hasAttribute('open')).toBe(false);
    expect(menuButton?.getAttribute('aria-expanded')).toBe('false');
    expect(document.activeElement).toBe(menuButton);
  });

  it('closes the mobile navigation after an anchor is selected', async () => {
    const fixture = TestBed.createComponent(SiteHeader);
    fixture.componentRef.setInput('navigationItems', navigationItems);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    element.querySelector<HTMLButtonElement>('.menu-button')?.click();
    element.querySelector<HTMLAnchorElement>('.mobile-navigation nav a')?.click();
    await fixture.whenStable();

    expect(element.querySelector('dialog')?.hasAttribute('open')).toBe(false);
  });
});
