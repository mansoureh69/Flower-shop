import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as axe from 'axe-core';

import { App } from './app';

describe('App', () => {
  let fixture: ComponentFixture<App>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    await fixture.whenStable();
  });

  it('creates the flower shop landing page', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders all gallery and service images', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelectorAll('.flower-carousel__item')).toHaveLength(5);
    expect(element.querySelectorAll('.service')).toHaveLength(3);
  });

  it('exposes the primary content headings', () => {
    const text = (fixture.nativeElement as HTMLElement).textContent;

    expect(text).toContain('Who we are');
    expect(text).toContain('What we do');
    expect(text).toContain('Work with us');
  });

  it('provides an accessible name and keyboard access for the flower gallery', () => {
    const gallery = (fixture.nativeElement as HTMLElement).querySelector('.flower-carousel');

    expect(gallery?.getAttribute('aria-label')).toContain('Scroll horizontally');
    expect(gallery?.getAttribute('tabindex')).toBe('0');
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
