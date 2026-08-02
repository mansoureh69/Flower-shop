import { ComponentFixture, TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  let fixture: ComponentFixture<App>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [App]
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    fixture.detectChanges();
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
});
