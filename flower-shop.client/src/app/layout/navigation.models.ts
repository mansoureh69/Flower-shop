export interface NavItem {
  readonly label: string;
  readonly fragment: string;
}

export const STOREFRONT_NAVIGATION: readonly NavItem[] = [
  { label: 'Fresh Picks', fragment: 'fresh-picks' },
  { label: 'Occasions', fragment: 'occasions' },
  { label: 'Delivery', fragment: 'delivery' },
  { label: 'Our Story', fragment: 'story' },
];
