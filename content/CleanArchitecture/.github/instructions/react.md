---
description: React + Vite development standards (alternative frontend)
applyTo: 'src/Frontend/**/*.{tsx,jsx}'
---

# React Instructions

## Purpose

This document defines standards and patterns for React development in this project. React with Vite is an **alternative frontend framework** to Angular for the Cubido Clean Architecture template.

## Project Structure

```
src/Frontend/
├── src/
│   ├── app/
│   │   ├── components/          # Reusable components
│   │   ├── features/            # Feature-specific components
│   │   │   ├── todo-lists/
│   │   │   └── todo-items/
│   │   ├── hooks/               # Custom React hooks
│   │   ├── services/            # API services
│   │   ├── stores/              # State management
│   │   ├── types/               # TypeScript types
│   │   └── utils/               # Utility functions
│   ├── assets/
│   ├── styles/
│   ├── main.tsx
│   └── App.tsx
├── package.json
├── vite.config.ts
└── tsconfig.json
```

## React + Vite Setup

### Vite Configuration

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/app/components'),
      '@features': path.resolve(__dirname, './src/app/features'),
      '@hooks': path.resolve(__dirname, './src/app/hooks'),
      '@services': path.resolve(__dirname, './src/app/services'),
      '@stores': path.resolve(__dirname, './src/app/stores'),
      '@types': path.resolve(__dirname, './src/app/types')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
});
```

### TypeScript Configuration

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"],
      "@components/*": ["./src/app/components/*"],
      "@features/*": ["./src/app/features/*"],
      "@hooks/*": ["./src/app/hooks/*"],
      "@services/*": ["./src/app/services/*"],
      "@stores/*": ["./src/app/stores/*"],
      "@types/*": ["./src/app/types/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

## Component Patterns

### Functional Components (Default)

```tsx
import { FC, useState, useEffect } from 'react';
import { TodoItemDto } from '@types/api-client';
import { useTodoService } from '@hooks/useTodoService';

interface TodoListProps {
  listId: number;
  onTodoClick?: (todo: TodoItemDto) => void;
}

export const TodoList: FC<TodoListProps> = ({ listId, onTodoClick }) => {
  const [todos, setTodos] = useState<TodoItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const todoService = useTodoService();

  useEffect(() => {
    loadTodos();
  }, [listId]);

  const loadTodos = async () => {
    try {
      setLoading(true);
      const data = await todoService.getAll(listId);
      setTodos(data);
    } catch (err) {
      setError('Failed to load todos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const toggleTodo = async (id: string) => {
    const todo = todos.find(t => t.id === id);
    if (!todo) return;

    await todoService.update(id, { done: !todo.done });
    setTodos(todos.map(t => 
      t.id === id ? { ...t, done: !t.done } : t
    ));
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <ul className="todo-list">
      {todos.map(todo => (
        <li key={todo.id} className={todo.done ? 'completed' : ''}>
          <input
            type="checkbox"
            checked={todo.done}
            onChange={() => toggleTodo(todo.id)}
          />
          <span onClick={() => onTodoClick?.(todo)}>
            {todo.title}
          </span>
        </li>
      ))}
    </ul>
  );
};
```

### Component Best Practices

✅ **DO:**

```tsx
// ✅ Use functional components
export const MyComponent: FC<Props> = ({ prop1, prop2 }) => {
  return <div>{prop1}</div>;
};

// ✅ Define prop types
interface MyComponentProps {
  title: string;
  onSave?: () => void;
  children?: ReactNode;
}

// ✅ Use const for components
export const MyComponent: FC<MyComponentProps> = () => { };

// ✅ Destructure props
export const MyComponent: FC<Props> = ({ title, onSave }) => {
  // Use title and onSave directly
};

// ✅ Use early returns for loading/error states
if (loading) return <Spinner />;
if (error) return <ErrorMessage error={error} />;

// ✅ Extract complex logic to custom hooks
const { todos, loading, error, addTodo } = useTodos(listId);
```

❌ **DON'T:**

```tsx
// ❌ Don't use class components (unless necessary)
class MyComponent extends React.Component { }

// ❌ Don't forget prop types
export const MyComponent = (props) => { };  // No types!

// ❌ Don't define components inside components
export const Parent = () => {
  const Child = () => <div>Bad!</div>;  // NO!
  return <Child />;
};

// ❌ Don't mutate state directly
todos.push(newTodo);  // NO!
setTodos([...todos, newTodo]);  // YES!
```

## Hooks

### Built-in Hooks

```tsx
import { useState, useEffect, useCallback, useMemo, useRef } from 'react';

export const TodoComponent: FC = () => {
  // State
  const [count, setCount] = useState(0);
  const [todos, setTodos] = useState<TodoItemDto[]>([]);

  // Refs
  const inputRef = useRef<HTMLInputElement>(null);

  // Effects
  useEffect(() => {
    // Run on mount
    console.log('Component mounted');
    
    // Cleanup function
    return () => {
      console.log('Component unmounted');
    };
  }, []); // Empty deps = run once

  useEffect(() => {
    // Run when count changes
    console.log('Count changed:', count);
  }, [count]);

  // Memoized values
  const completedCount = useMemo(
    () => todos.filter(t => t.done).length,
    [todos]
  );

  // Memoized callbacks
  const handleAdd = useCallback((title: string) => {
    setTodos([...todos, { id: Date.now().toString(), title, done: false }]);
  }, [todos]);

  return (
    <div>
      <input ref={inputRef} />
      <p>Completed: {completedCount}</p>
    </div>
  );
};
```

### Custom Hooks

```tsx
// useTodos.ts
import { useState, useEffect } from 'react';
import { TodoItemDto } from '@types/api-client';
import { todoService } from '@services/todoService';

export const useTodos = (listId: number) => {
  const [todos, setTodos] = useState<TodoItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    loadTodos();
  }, [listId]);

  const loadTodos = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await todoService.getAll(listId);
      setTodos(data);
    } catch (err) {
      setError(err as Error);
    } finally {
      setLoading(false);
    }
  };

  const addTodo = async (title: string) => {
    const newTodo = await todoService.create({ listId, title });
    setTodos([...todos, newTodo]);
  };

  const updateTodo = async (id: string, updates: Partial<TodoItemDto>) => {
    await todoService.update(id, updates);
    setTodos(todos.map(t => t.id === id ? { ...t, ...updates } : t));
  };

  const deleteTodo = async (id: string) => {
    await todoService.delete(id);
    setTodos(todos.filter(t => t.id !== id));
  };

  return {
    todos,
    loading,
    error,
    addTodo,
    updateTodo,
    deleteTodo,
    refresh: loadTodos
  };
};

// Usage
const MyComponent: FC = () => {
  const { todos, loading, error, addTodo } = useTodos(1);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div>
      {todos.map(todo => (
        <div key={todo.id}>{todo.title}</div>
      ))}
    </div>
  );
};
```

### Hook Best Practices

✅ **DO:**

```tsx
// ✅ Name custom hooks with "use" prefix
export const useTodos = () => { };

// ✅ Return objects from custom hooks
return { todos, loading, error, addTodo };

// ✅ Use dependency arrays correctly
useEffect(() => {
  // Use prop
}, [prop]); // Include prop in deps

// ✅ Clean up side effects
useEffect(() => {
  const subscription = api.subscribe();
  return () => subscription.unsubscribe();
}, []);
```

❌ **DON'T:**

```tsx
// ❌ Don't call hooks conditionally
if (condition) {
  useEffect(() => { }); // NO!
}

// ❌ Don't call hooks in loops
for (let i = 0; i < 10; i++) {
  useState(i); // NO!
}

// ❌ Don't forget dependency arrays
useEffect(() => {
  // Uses 'count' but not in deps
  console.log(count);
}); // Missing deps!

// ❌ Don't mutate state
const [items, setItems] = useState([]);
items.push(newItem); // NO!
```

## State Management

### Local State (useState)

```tsx
export const Counter: FC = () => {
  const [count, setCount] = useState(0);

  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
      <button onClick={() => setCount(prev => prev - 1)}>Decrement</button>
    </div>
  );
};
```

### Context API (For Global State)

```tsx
// TodoContext.tsx
import { createContext, useContext, useState, FC, ReactNode } from 'react';
import { TodoItemDto } from '@types/api-client';

interface TodoContextValue {
  todos: TodoItemDto[];
  addTodo: (todo: TodoItemDto) => void;
  removeTodo: (id: string) => void;
  updateTodo: (id: string, updates: Partial<TodoItemDto>) => void;
}

const TodoContext = createContext<TodoContextValue | undefined>(undefined);

export const TodoProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [todos, setTodos] = useState<TodoItemDto[]>([]);

  const addTodo = (todo: TodoItemDto) => {
    setTodos([...todos, todo]);
  };

  const removeTodo = (id: string) => {
    setTodos(todos.filter(t => t.id !== id));
  };

  const updateTodo = (id: string, updates: Partial<TodoItemDto>) => {
    setTodos(todos.map(t => t.id === id ? { ...t, ...updates } : t));
  };

  return (
    <TodoContext.Provider value={{ todos, addTodo, removeTodo, updateTodo }}>
      {children}
    </TodoContext.Provider>
  );
};

export const useTodoContext = () => {
  const context = useContext(TodoContext);
  if (!context) {
    throw new Error('useTodoContext must be used within TodoProvider');
  }
  return context;
};

// Usage
// In App.tsx
<TodoProvider>
  <App />
</TodoProvider>

// In components
const { todos, addTodo } = useTodoContext();
```

### Zustand (Recommended for Complex State)

```bash
npm install zustand
```

```tsx
// stores/todoStore.ts
import { create } from 'zustand';
import { TodoItemDto } from '@types/api-client';
import { todoService } from '@services/todoService';

interface TodoStore {
  todos: TodoItemDto[];
  loading: boolean;
  error: string | null;
  
  // Actions
  loadTodos: (listId: number) => Promise<void>;
  addTodo: (title: string, listId: number) => Promise<void>;
  updateTodo: (id: string, updates: Partial<TodoItemDto>) => Promise<void>;
  deleteTodo: (id: string) => Promise<void>;
  toggleTodo: (id: string) => void;
}

export const useTodoStore = create<TodoStore>((set, get) => ({
  todos: [],
  loading: false,
  error: null,

  loadTodos: async (listId: number) => {
    set({ loading: true, error: null });
    try {
      const todos = await todoService.getAll(listId);
      set({ todos, loading: false });
    } catch (error) {
      set({ error: 'Failed to load todos', loading: false });
    }
  },

  addTodo: async (title: string, listId: number) => {
    const newTodo = await todoService.create({ title, listId });
    set(state => ({ todos: [...state.todos, newTodo] }));
  },

  updateTodo: async (id: string, updates: Partial<TodoItemDto>) => {
    await todoService.update(id, updates);
    set(state => ({
      todos: state.todos.map(t => t.id === id ? { ...t, ...updates } : t)
    }));
  },

  deleteTodo: async (id: string) => {
    await todoService.delete(id);
    set(state => ({ todos: state.todos.filter(t => t.id !== id) }));
  },

  toggleTodo: (id: string) => {
    const todo = get().todos.find(t => t.id === id);
    if (todo) {
      get().updateTodo(id, { done: !todo.done });
    }
  }
}));

// Usage
export const TodoList: FC = () => {
  const { todos, loading, loadTodos, toggleTodo } = useTodoStore();

  useEffect(() => {
    loadTodos(1);
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <ul>
      {todos.map(todo => (
        <li key={todo.id}>
          <input
            type="checkbox"
            checked={todo.done}
            onChange={() => toggleTodo(todo.id)}
          />
          {todo.title}
        </li>
      ))}
    </ul>
  );
};
```

## API Integration

### Using NSwag Generated Client

```tsx
// services/todoService.ts
import { 
  TodoItemsClient, 
  TodoItemDto, 
  CreateTodoItemCommand 
} from '@types/api-client';

class TodoService {
  private client: TodoItemsClient;

  constructor() {
    // Base URL is handled by Vite proxy
    this.client = new TodoItemsClient();
  }

  async getAll(listId: number): Promise<TodoItemDto[]> {
    return this.client.getTodoItems(listId);
  }

  async getById(id: string): Promise<TodoItemDto> {
    return this.client.getTodoItem(id);
  }

  async create(command: CreateTodoItemCommand): Promise<number> {
    return this.client.createTodoItem(command);
  }

  async update(id: string, updates: Partial<TodoItemDto>): Promise<void> {
    return this.client.updateTodoItem(id, updates);
  }

  async delete(id: string): Promise<void> {
    return this.client.deleteTodoItem(id);
  }
}

export const todoService = new TodoService();
```

### Custom Hook for API Calls

```tsx
// hooks/useApi.ts
import { useState, useCallback } from 'react';

interface UseApiOptions<T> {
  onSuccess?: (data: T) => void;
  onError?: (error: Error) => void;
}

export const useApi = <T,>(
  apiCall: (...args: any[]) => Promise<T>,
  options?: UseApiOptions<T>
) => {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const execute = useCallback(async (...args: any[]) => {
    try {
      setLoading(true);
      setError(null);
      const result = await apiCall(...args);
      setData(result);
      options?.onSuccess?.(result);
      return result;
    } catch (err) {
      const error = err as Error;
      setError(error);
      options?.onError?.(error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, [apiCall]);

  return { data, loading, error, execute };
};

// Usage
const { data, loading, error, execute } = useApi(todoService.getAll);

useEffect(() => {
  execute(1); // Load todos for list 1
}, []);
```

## Routing

### React Router Setup

```bash
npm install react-router-dom
```

```tsx
// App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { TodoListsPage } from '@features/todo-lists/TodoListsPage';
import { TodoItemsPage } from '@features/todo-items/TodoItemsPage';
import { LoginPage } from '@features/auth/LoginPage';
import { NotFoundPage } from '@components/NotFoundPage';
import { PrivateRoute } from '@components/PrivateRoute';

export const App: FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/todos" replace />} />
        <Route path="/login" element={<LoginPage />} />
        
        <Route element={<PrivateRoute />}>
          <Route path="/todos" element={<TodoListsPage />} />
          <Route path="/todos/:id" element={<TodoItemsPage />} />
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
};
```

### Protected Routes

```tsx
// components/PrivateRoute.tsx
import { FC } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';

export const PrivateRoute: FC = () => {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
};
```

### Navigation

```tsx
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';

export const TodoItem: FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();

  const filter = searchParams.get('filter') || 'all';

  const handleClick = () => {
    navigate('/todos');
    // Or with state
    navigate('/todos', { state: { from: 'item' } });
  };

  const updateFilter = (newFilter: string) => {
    setSearchParams({ filter: newFilter });
  };

  return <div>Todo {id}</div>;
};
```

## Forms

### Controlled Components

```tsx
import { FormEvent, useState } from 'react';
import { CreateTodoItemCommand } from '@types/api-client';

export const TodoForm: FC<{ listId: number; onSubmit: (command: CreateTodoItemCommand) => void }> = ({ 
  listId, 
  onSubmit 
}) => {
  const [title, setTitle] = useState('');
  const [priority, setPriority] = useState(0);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    
    if (!title.trim()) return;

    onSubmit({ listId, title, priority });
    setTitle('');
    setPriority(0);
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder="What needs to be done?"
        maxLength={200}
        required
      />

      <select value={priority} onChange={(e) => setPriority(Number(e.target.value))}>
        <option value={0}>None</option>
        <option value={1}>Low</option>
        <option value={2}>Medium</option>
        <option value={3}>High</option>
      </select>

      <button type="submit" disabled={!title.trim()}>
        Add Todo
      </button>
    </form>
  );
};
```

### Form with Validation (React Hook Form)

```bash
npm install react-hook-form zod @hookform/resolvers
```

```tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const todoSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title too long'),
  priority: z.number().min(0).max(3),
  listId: z.number()
});

type TodoFormData = z.infer<typeof todoSchema>;

export const TodoForm: FC<{ listId: number }> = ({ listId }) => {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<TodoFormData>({
    resolver: zodResolver(todoSchema),
    defaultValues: { listId, priority: 0 }
  });

  const onSubmit = async (data: TodoFormData) => {
    await todoService.create(data);
    reset();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <input
          {...register('title')}
          placeholder="What needs to be done?"
        />
        {errors.title && <span className="error">{errors.title.message}</span>}
      </div>

      <div>
        <select {...register('priority', { valueAsNumber: true })}>
          <option value={0}>None</option>
          <option value={1}>Low</option>
          <option value={2}>Medium</option>
          <option value={3}>High</option>
        </select>
      </div>

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Adding...' : 'Add Todo'}
      </button>
    </form>
  );
};
```

## Styling

### CSS Modules

```tsx
// TodoItem.module.css
.todoItem {
  padding: 1rem;
  border-bottom: 1px solid #ddd;
}

.todoItem.completed {
  opacity: 0.6;
  text-decoration: line-through;
}

.title {
  font-size: 1.2rem;
}

// TodoItem.tsx
import styles from './TodoItem.module.css';

export const TodoItem: FC<{ todo: TodoItemDto }> = ({ todo }) => {
  return (
    <div className={`${styles.todoItem} ${todo.done ? styles.completed : ''}`}>
      <span className={styles.title}>{todo.title}</span>
    </div>
  );
};
```

### Styled Components (Alternative)

```bash
npm install styled-components
npm install -D @types/styled-components
```

```tsx
import styled from 'styled-components';

const TodoItemContainer = styled.div<{ $completed: boolean }>`
  padding: 1rem;
  border-bottom: 1px solid #ddd;
  opacity: ${props => props.$completed ? 0.6 : 1};
  text-decoration: ${props => props.$completed ? 'line-through' : 'none'};
`;

const Title = styled.span`
  font-size: 1.2rem;
  font-weight: bold;
`;

export const TodoItem: FC<{ todo: TodoItemDto }> = ({ todo }) => {
  return (
    <TodoItemContainer $completed={todo.done}>
      <Title>{todo.title}</Title>
    </TodoItemContainer>
  );
};
```

## Testing

### Component Testing (Vitest + Testing Library)

```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event
```

```tsx
// TodoList.test.tsx
import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TodoList } from './TodoList';
import { todoService } from '@services/todoService';

vi.mock('@services/todoService');

describe('TodoList', () => {
  it('renders todos', async () => {
    const mockTodos = [
      { id: '1', title: 'Test Todo', done: false, priority: 0, listId: 1 }
    ];

    vi.mocked(todoService.getAll).mockResolvedValue(mockTodos);

    render(<TodoList listId={1} />);

    await waitFor(() => {
      expect(screen.getByText('Test Todo')).toBeInTheDocument();
    });
  });

  it('toggles todo', async () => {
    const user = userEvent.setup();
    const mockTodos = [
      { id: '1', title: 'Test Todo', done: false, priority: 0, listId: 1 }
    ];

    vi.mocked(todoService.getAll).mockResolvedValue(mockTodos);
    vi.mocked(todoService.update).mockResolvedValue();

    render(<TodoList listId={1} />);

    const checkbox = await screen.findByRole('checkbox');
    await user.click(checkbox);

    expect(todoService.update).toHaveBeenCalledWith('1', { done: true });
  });
});
```

## Performance Optimization

### React.memo

```tsx
import { memo } from 'react';

export const TodoItem = memo<TodoItemProps>(({ todo, onToggle }) => {
  console.log('TodoItem rendered');
  
  return (
    <div>
      <input
        type="checkbox"
        checked={todo.done}
        onChange={() => onToggle(todo.id)}
      />
      {todo.title}
    </div>
  );
});
```

### useMemo and useCallback

```tsx
export const TodoList: FC = () => {
  const [todos, setTodos] = useState<TodoItemDto[]>([]);
  const [filter, setFilter] = useState<'all' | 'active' | 'completed'>('all');

  // Memoize expensive calculation
  const filteredTodos = useMemo(() => {
    switch (filter) {
      case 'active':
        return todos.filter(t => !t.done);
      case 'completed':
        return todos.filter(t => t.done);
      default:
        return todos;
    }
  }, [todos, filter]);

  // Memoize callback to prevent re-renders
  const handleToggle = useCallback((id: string) => {
    setTodos(todos.map(t => 
      t.id === id ? { ...t, done: !t.done } : t
    ));
  }, [todos]);

  return (
    <div>
      {filteredTodos.map(todo => (
        <TodoItem key={todo.id} todo={todo} onToggle={handleToggle} />
      ))}
    </div>
  );
};
```

### Code Splitting

```tsx
import { lazy, Suspense } from 'react';

const TodoList = lazy(() => import('@features/todo-lists/TodoList'));
const TodoDetails = lazy(() => import('@features/todo-items/TodoDetails'));

export const App: FC = () => {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Routes>
        <Route path="/todos" element={<TodoList />} />
        <Route path="/todos/:id" element={<TodoDetails />} />
      </Routes>
    </Suspense>
  );
};
```

## Best Practices Summary

### ✅ DO

- Use functional components
- Use TypeScript for type safety
- Use hooks (useState, useEffect, etc.)
- Extract logic to custom hooks
- Use memoization (useMemo, useCallback) wisely
- Use React.memo for expensive components
- Handle loading and error states
- Clean up effects
- Write tests

### ❌ DON'T

- Don't use class components (unless necessary)
- Don't mutate state directly
- Don't call hooks conditionally
- Don't forget dependency arrays
- Don't over-optimize with memo
- Don't put business logic in components
- Don't skip error handling
- Don't forget key props in lists

## Resources

- [React Documentation](https://react.dev/)
- [Vite Documentation](https://vitejs.dev/)
- [React Router](https://reactrouter.com/)
- [Zustand](https://github.com/pmndrs/zustand)
- [React Hook Form](https://react-hook-form.com/)
- [Testing Library](https://testing-library.com/react)
