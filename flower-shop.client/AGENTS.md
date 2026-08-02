# Angular client code-generation rules

Scope: every file under `flower-shop.client`.

Source: https://angular.dev/ai/develop-with-ai

## Current framework

- The client currently targets Angular 21.2 with strict TypeScript and strict Angular templates.
- Apply Angular 21-compatible guidance immediately.
- Treat Angular 22-only APIs and defaults as upgrade gates; do not generate code that the installed packages cannot compile.

## TypeScript

- Keep strict type checking enabled.
- Prefer inference where the type is obvious.
- Never introduce `any`; use a precise type or `unknown`.
- Keep transformations pure and model immutable data with `readonly` types.

## Angular

- Generate standalone components, directives, and pipes. Since standalone is the default, omit `standalone: true`.
- Bootstrap with `bootstrapApplication`; do not add new NgModules.
- Use signals for local mutable state and `computed()` for derived state.
- Use `linkedSignal()` only when writable state must remain synchronized with reactive sources.
- Use native template control flow: `@if`, `@for`, and `@switch`. Always provide a stable `track` expression for `@for`.
- Lazy-load routed feature components.
- Use `input()`, `output()`, and `model()` instead of decorator-based inputs and outputs.
- Put host bindings and listeners in the decorator's `host` object; do not use `@HostBinding` or `@HostListener`.
- Use class and style bindings instead of `ngClass` and `ngStyle`.
- Use `NgOptimizedImage` for static non-base64 images and provide correct dimensions or `fill`.
- Use relative `templateUrl` and `styleUrl` paths.
- Keep templates declarative and move non-trivial transformations into TypeScript.

## Forms and services

- On Angular 21, use Reactive Forms for production forms unless the task explicitly accepts the Signal Forms preview API.
- After upgrading to Angular 22, prefer stable Signal Forms for new forms.
- On Angular 21, use `@Injectable({ providedIn: 'root' })` for singleton services because `@Service` is not available.
- Prefer `inject()` over constructor injection.
- Design each service around one responsibility.

## Accessibility

- Meet WCAG 2.2 AA and pass automated AXE checks.
- Preserve semantic HTML, keyboard access, visible focus, accessible names, sufficient contrast, and correct focus management.
- Do not add redundant ARIA when native HTML already supplies the required semantics.

## Verification

- Run `npm run build` after Angular code changes.
- Run `npm test -- --watch=false` after implementation or test changes.
- For UI changes, verify responsive behavior and browser console output; run an AXE check when the browser test environment supports it.
