import { Component, ElementRef, input, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';

import { NavItem } from '../navigation.models';

@Component({
  selector: 'app-site-header',
  imports: [RouterLink],
  templateUrl: './site-header.html',
  styleUrl: './site-header.css',
})
export class SiteHeader
{
  readonly brandName = input('Sweet Flower Shop');
  readonly navigationItems = input.required<readonly NavItem[]>();

  private readonly menuButton = viewChild.required<ElementRef<HTMLButtonElement>>('menuButton');
  private readonly menuDialog = viewChild.required<ElementRef<HTMLDialogElement>>('menuDialog');

  protected readonly menuOpen = signal(false);

  protected openMenu(): void
  {
    const dialog = this.menuDialog().nativeElement;

    if (typeof dialog.showModal === 'function')
    {
      dialog.showModal();
    } else
    {
      dialog.setAttribute('open', '');
    }

    this.menuOpen.set(true);
  }

  protected closeMenu(): void
  {
    const dialog = this.menuDialog().nativeElement;

    if (typeof dialog.close === 'function' && dialog.open)
    {
      dialog.close();
    } else
    {
      dialog.removeAttribute('open');
      this.handleDialogClosed();
    }
  }

  protected handleDialogClosed(): void
  {
    this.menuOpen.set(false);
    this.menuButton().nativeElement.focus();
  }

  protected handleDialogCancelled(event: Event): void
  {
    event.preventDefault();
    this.closeMenu();
  }

  protected closeFromBackdrop(event: MouseEvent): void
  {
    if (event.target === event.currentTarget)
    {
      this.closeMenu();
    }
  }
}
