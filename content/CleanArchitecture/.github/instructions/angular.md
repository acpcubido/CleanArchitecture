---
description: Angular development standards for the frontend (default)
applyTo: 'src/Frontend/**/*.{ts,html,css,scss}'
---

# Angular Instructions

## Purpose

This document defines standards and patterns for Angular development in this project. Angular is the **default frontend framework** for the Cubido Clean Architecture template.

## Project Structure

```
src/Frontend/
├── src/
│   ├── app/
│   │   ├── core/                 # Singleton services, guards, interceptors
│   │   ├── shared/               # Shared components, directives, pipes
│   │   ├── features/             # Feature modules
│   │   │   ├── todo-lists/
│   │   │   │   ├── components/
│   │   │   │   ├── services/
│   │   │   │   └── models/
│   │   │   └── todo-items/
│   │   ├── layout/               # Layout components
│   │   └── app.component.ts
│   ├── assets/
│   ├── environments/
│   └── styles/
├── angular.json
├── package.json
└── tsconfig.json
```

## Angular Version

This project uses **Angular 17+** with:
- Standalone components (default)
- Signals for reactive state
- New control flow syntax (`@if`, `@for`, `@switch`)
- Inject function for dependency injection

## Component Architecture

### Standalone Components (Default)

```typescript
import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.scss']
})
export class TodoListComponent {
  // Use signals for reactive state
  private readonly todos = signal<TodoItemDto[]>([]);
  private readonly filter = signal<'all' | 'active' | 'completed'>('all');

  // Computed values
  readonly filteredTodos = computed(() => {
    const todos = this.todos();
    const filter = this.filter();

    switch (filter) {
      case 'active':
        return todos.filter(t => !t.done);
      case 'completed':
        return todos.filter(t => t.done);
      default:
        return todos;
    }
  });

  readonly completedCount = computed(() => 
    this.todos().filter(t => t.done).length
  );

  // Inject dependencies
  private readonly todoService = inject(TodoService);

  ngOnInit(): void {
    this.loadTodos();
  }

  private async loadTodos(): Promise<void> {
    const todos = await this.todoService.getAll();
    this.todos.set(todos);
  }

  addTodo(title: string): void {
    // Implementation
  }

  toggleTodo(id: string): void {
    this.todos.update(todos => 
      todos.map(t => t.id === id ? { ...t, done: !t.done } : t)
    );
  }
}
```

### Template Syntax (New Control Flow)

```html
<!-- ✅ New @if syntax -->
@if (filteredTodos().length > 0) {
  <ul class="todo-list">
    <!-- ✅ New @for syntax -->
    @for (todo of filteredTodos(); track todo.id) {
      <li [class.completed]="todo.done">
        <input 
          type="checkbox" 
          [checked]="todo.done"
          (change)="toggleTodo(todo.id)" />
        <span>{{ todo.title }}</span>
      </li>
    }
  </ul>
} @else {
  <p class="empty-state">No todos yet!</p>
}

<!-- ✅ New @switch syntax -->
@switch (currentView()) {
  @case ('list') {
    <app-todo-list />
  }
  @case ('grid') {
    <app-todo-grid />
  }
  @default {
    <app-todo-list />
  }
}
```

### Component Best Practices

✅ **DO:**

```typescript
// ✅ Use standalone components
@Component({
  selector: 'app-feature',
  standalone: true,
  imports: [CommonModule],
  template: `...`
})
export class FeatureComponent { }

// ✅ Use signals for state
private readonly items = signal<Item[]>([]);

// ✅ Use computed for derived state
readonly itemCount = computed(() => this.items().length);

// ✅ Use inject() function
private readonly service = inject(MyService);

// ✅ Use OnPush change detection
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})

// ✅ Implement lifecycle interfaces
export class MyComponent implements OnInit, OnDestroy {
  ngOnInit(): void { }
  ngOnDestroy(): void { }
}

// ✅ Use trackBy functions
readonly trackById = (index: number, item: Item) => item.id;
```

❌ **DON'T:**

```typescript
// ❌ Don't use NgModule components (unless necessary)
@NgModule({
  declarations: [MyComponent],
  exports: [MyComponent]
})
export class MyModule { }

// ❌ Don't use old *ngIf syntax in new projects
<div *ngIf="condition">...</div>  // Use @if instead

// ❌ Don't mutate signals directly
this.todos()[0].done = true;  // NO!
this.todos.mutate(todos => todos[0].done = true);  // YES

// ❌ Don't forget trackBy in @for
@for (item of items(); track $index) { }  // Bad - use unique ID
```

## Services

### Service Pattern

```typescript
import { Injectable, inject, signal } from '@angular/core';
import { TodoItemsClient, TodoItemDto, CreateTodoItemCommand } from './api-client';

@Injectable({
  providedIn: 'root'  // Singleton service
})
export class TodoService {
  // Inject generated API client
  private readonly client = inject(TodoItemsClient);

  // State management with signals
  private readonly todosState = signal<TodoItemDto[]>([]);
  readonly todos = this.todosState.asReadonly();

  async loadTodos(listId: number): Promise<void> {
    const todos = await this.client.getTodoItems(listId);
    this.todosState.set(todos);
  }

  async createTodo(command: CreateTodoItemCommand): Promise<void> {
    const id = await this.client.createTodoItem(command);
    await this.loadTodos(command.listId);
  }

  async updateTodo(id: string, updates: Partial<TodoItemDto>): Promise<void> {
    // Call API
    await this.client.updateTodoItem(id, updates);
    
    // Update local state
    this.todosState.update(todos => 
      todos.map(t => t.id === id ? { ...t, ...updates } : t)
    );
  }

  async deleteTodo(id: string): Promise<void> {
    await this.client.deleteTodoItem(id);
    this.todosState.update(todos => todos.filter(t => t.id !== id));
  }
}
```

### Service Best Practices

✅ **DO:**

```typescript
// ✅ Use providedIn: 'root' for singletons
@Injectable({ providedIn: 'root' })

// ✅ Use signals for service state
private readonly state = signal<State>(initialState);
readonly state$ = this.state.asReadonly();

// ✅ Handle errors properly
async loadData(): Promise<void> {
  try {
    const data = await this.client.getData();
    this.state.set(data);
  } catch (error) {
    console.error('Failed to load data', error);
    this.errorState.set(error);
  }
}

// ✅ Use dependency injection
private readonly http = inject(HttpClient);
private readonly config = inject(APP_CONFIG);
```

❌ **DON'T:**

```typescript
// ❌ Don't create service instances manually
const service = new MyService();  // NO!

// ❌ Don't use RxJS for simple state (use signals)
private todos$ = new BehaviorSubject<TodoItem[]>([]);  // Prefer signals

// ❌ Don't mutate state directly
this.state().items.push(newItem);  // NO!
this.state.update(s => ({ ...s, items: [...s.items, newItem] }));  // YES
```

## API Integration

### Using NSwag Generated Client

The project uses **NSwag** to generate TypeScript clients from the backend API.

```typescript
// Generated client is automatically imported
import { 
  TodoItemsClient, 
  TodoItemDto, 
  CreateTodoItemCommand 
} from '@api/api-client';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private readonly client = inject(TodoItemsClient);

  // Use generated methods
  async getAll(): Promise<TodoItemDto[]> {
    return this.client.getTodoItems();
  }

  async create(command: CreateTodoItemCommand): Promise<number> {
    return this.client.createTodoItem(command);
  }
}
```

### HTTP Interceptors

```typescript
import { HttpInterceptorFn } from '@angular/common/http';

// ✅ Functional interceptor (Angular 17+)
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('auth_token');
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req);
};

// Register in app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor])
    )
  ]
};
```

### Error Handling

```typescript
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  handleError(error: unknown): void {
    if (error instanceof HttpErrorResponse) {
      // API error
      console.error('API Error:', error.status, error.message);
      
      switch (error.status) {
        case 400:
          this.showError('Invalid request');
          break;
        case 401:
          this.redirectToLogin();
          break;
        case 404:
          this.showError('Resource not found');
          break;
        default:
          this.showError('An error occurred');
      }
    } else {
      // Client-side error
      console.error('Client Error:', error);
      this.showError('An unexpected error occurred');
    }
  }
}
```

## Reactive State with Signals

### Signal Patterns

```typescript
export class TodoStore {
  // Writable signal
  private readonly _todos = signal<TodoItemDto[]>([]);
  
  // Read-only signal
  readonly todos = this._todos.asReadonly();

  // Computed signals
  readonly completedTodos = computed(() => 
    this._todos().filter(t => t.done)
  );

  readonly activeTodos = computed(() => 
    this._todos().filter(t => !t.done)
  );

  readonly stats = computed(() => ({
    total: this._todos().length,
    completed: this.completedTodos().length,
    active: this.activeTodos().length,
    percentComplete: this._todos().length > 0 
      ? (this.completedTodos().length / this._todos().length) * 100 
      : 0
  }));

  // Methods to update state
  addTodo(todo: TodoItemDto): void {
    this._todos.update(todos => [...todos, todo]);
  }

  removeTodo(id: string): void {
    this._todos.update(todos => todos.filter(t => t.id !== id));
  }

  updateTodo(id: string, updates: Partial<TodoItemDto>): void {
    this._todos.update(todos =>
      todos.map(t => t.id === id ? { ...t, ...updates } : t)
    );
  }

  clearCompleted(): void {
    this._todos.update(todos => todos.filter(t => !t.done));
  }
}
```

### Effects

```typescript
import { effect } from '@angular/core';

export class TodoComponent {
  private readonly todos = signal<TodoItemDto[]>([]);

  constructor() {
    // Effect runs whenever todos changes
    effect(() => {
      const todos = this.todos();
      console.log(`Todo count: ${todos.length}`);
      
      // Save to localStorage
      localStorage.setItem('todos', JSON.stringify(todos));
    });
  }
}
```

## Forms

### Reactive Forms with Signals

```typescript
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-todo-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <input 
        type="text" 
        formControlName="title" 
        placeholder="What needs to be done?" />
      
      <select formControlName="priority">
        <option value="0">None</option>
        <option value="1">Low</option>
        <option value="2">Medium</option>
        <option value="3">High</option>
      </select>

      <button type="submit" [disabled]="!form.valid">
        Add Todo
      </button>

      @if (form.get('title')?.invalid && form.get('title')?.touched) {
        <div class="error">Title is required</div>
      }
    </form>
  `
})
export class TodoFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly todoService = inject(TodoService);

  readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    priority: [0, Validators.required],
    listId: [1, Validators.required]
  });

  async onSubmit(): Promise<void> {
    if (this.form.valid) {
      const command: CreateTodoItemCommand = this.form.value;
      await this.todoService.createTodo(command);
      this.form.reset();
    }
  }
}
```

### Form Validation

```typescript
// Custom validator
function titleValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) return null;
    
    if (value.length < 3) {
      return { minLength: { requiredLength: 3, actualLength: value.length } };
    }
    
    return null;
  };
}

// Usage
this.form = this.fb.group({
  title: ['', [Validators.required, titleValidator()]]
});
```

## Routing

### Route Configuration

```typescript
// app.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'todos',
    pathMatch: 'full'
  },
  {
    path: 'todos',
    canActivate: [authGuard],
    loadComponent: () => 
      import('./features/todo-lists/todo-lists.component').then(m => m.TodoListsComponent)
  },
  {
    path: 'todos/:id',
    canActivate: [authGuard],
    loadComponent: () => 
      import('./features/todo-items/todo-items.component').then(m => m.TodoItemsComponent)
  },
  {
    path: 'login',
    loadComponent: () => 
      import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: '**',
    loadComponent: () => 
      import('./shared/not-found.component').then(m => m.NotFoundComponent)
  }
];
```

### Guards (Functional)

```typescript
// auth.guard.ts
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};
```

### Navigation

```typescript
export class MyComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  navigateToTodo(id: string): void {
    this.router.navigate(['/todos', id]);
  }

  navigateWithQueryParams(): void {
    this.router.navigate(['/todos'], {
      queryParams: { filter: 'completed' }
    });
  }

  // Get route params
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    // Or subscribe to params
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      this.loadTodo(id);
    });
  }
}
```

## Styling

### Component Styles

```typescript
@Component({
  selector: 'app-todo-item',
  styleUrls: ['./todo-item.component.scss'],
  // Or inline
  styles: [`
    .todo-item {
      padding: 1rem;
      border-bottom: 1px solid #ddd;
    }
  `]
})
```

### SCSS Best Practices

```scss
// ✅ Use :host for component styles
:host {
  display: block;
  padding: 1rem;
}

// ✅ Use BEM naming
.todo-item {
  &__title {
    font-size: 1.2rem;
    font-weight: bold;
  }

  &__checkbox {
    margin-right: 0.5rem;
  }

  &--completed {
    opacity: 0.6;
    text-decoration: line-through;
  }
}

// ✅ Use CSS custom properties
:root {
  --primary-color: #007bff;
  --danger-color: #dc3545;
}

.button {
  background-color: var(--primary-color);
}
```

## Testing

### Component Testing

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TodoListComponent } from './todo-list.component';
import { TodoService } from './todo.service';

describe('TodoListComponent', () => {
  let component: TodoListComponent;
  let fixture: ComponentFixture<TodoListComponent>;
  let mockTodoService: jasmine.SpyObj<TodoService>;

  beforeEach(async () => {
    mockTodoService = jasmine.createSpyObj('TodoService', ['getAll', 'create']);

    await TestBed.configureTestingModule({
      imports: [TodoListComponent],
      providers: [
        { provide: TodoService, useValue: mockTodoService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TodoListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load todos on init', async () => {
    const mockTodos = [
      { id: '1', title: 'Test', done: false, priority: 0 }
    ];
    mockTodoService.getAll.and.returnValue(Promise.resolve(mockTodos));

    await component.ngOnInit();

    expect(component.todos()).toEqual(mockTodos);
  });
});
```

### Service Testing

```typescript
describe('TodoService', () => {
  let service: TodoService;
  let mockClient: jasmine.SpyObj<TodoItemsClient>;

  beforeEach(() => {
    mockClient = jasmine.createSpyObj('TodoItemsClient', ['getTodoItems', 'createTodoItem']);

    TestBed.configureTestingModule({
      providers: [
        TodoService,
        { provide: TodoItemsClient, useValue: mockClient }
      ]
    });

    service = TestBed.inject(TodoService);
  });

  it('should load todos', async () => {
    const mockTodos: TodoItemDto[] = [
      { id: '1', title: 'Test', done: false, priority: 0, listId: 1 }
    ];
    mockClient.getTodoItems.and.returnValue(Promise.resolve(mockTodos));

    await service.loadTodos(1);

    expect(service.todos()).toEqual(mockTodos);
  });
});
```

## Performance Optimization

### OnPush Change Detection

```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OptimizedComponent {
  // Use signals for automatic change detection
  readonly items = signal<Item[]>([]);
}
```

### Lazy Loading

```typescript
// Lazy load feature modules
const routes: Routes = [
  {
    path: 'admin',
    loadChildren: () => 
      import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
  }
];
```

### Virtual Scrolling

```typescript
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';

@Component({
  selector: 'app-large-list',
  standalone: true,
  imports: [ScrollingModule],
  template: `
    <cdk-virtual-scroll-viewport itemSize="50" class="viewport">
      @for (item of items(); track item.id) {
        <div class="item">{{ item.title }}</div>
      }
    </cdk-virtual-scroll-viewport>
  `
})
```

## Best Practices Summary

### ✅ DO

- Use standalone components
- Use signals for reactive state
- Use new control flow syntax (`@if`, `@for`)
- Use inject() function
- Use OnPush change detection
- Lazy load routes
- Use trackBy with @for
- Handle errors properly
- Write tests

### ❌ DON'T

- Don't use NgModules (unless integrating legacy code)
- Don't use old `*ngIf` syntax
- Don't mutate signal state directly
- Don't forget to unsubscribe (use takeUntilDestroyed())
- Don't put business logic in components
- Don't skip error handling

## Resources

- [Angular Documentation](https://angular.dev/)
- [Angular Signals Guide](https://angular.dev/guide/signals)
- [Angular CLI](https://angular.dev/cli)
- [RxJS Documentation](https://rxjs.dev/)
