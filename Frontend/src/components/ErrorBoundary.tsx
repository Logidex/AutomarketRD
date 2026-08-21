import { Component, type ErrorInfo, type ReactNode } from 'react';

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  hayError: boolean;
}

export default class ErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  state: ErrorBoundaryState = { hayError: false };

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hayError: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Error no capturado en la interfaz:', error, info.componentStack);
  }

  render() {
    if (!this.state.hayError) {
      return this.props.children;
    }

    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-[#0c101b] px-6 text-center">
        <h1 className="text-2xl font-bold text-white">
          Algo salió mal
        </h1>
        <p className="max-w-md text-sm text-slate-400">
          Ocurrió un error inesperado al mostrar esta página. Puedes recargar
          o volver al inicio.
        </p>
        <div className="flex gap-3">
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="rounded-lg bg-blue-500 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600"
          >
            Recargar
          </button>
          <button
            type="button"
            onClick={() => window.location.assign('/')}
            className="rounded-lg border border-slate-600 px-4 py-2 text-sm font-semibold text-slate-200 hover:bg-slate-800"
          >
            Ir al inicio
          </button>
        </div>
      </div>
    );
  }
}
