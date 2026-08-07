import React, { createContext, useContext, useState } from 'react';
import Spinner from '../components/Spinner';

interface LoadingContextType {
  setLoading: (val: boolean) => void;
}

const LoadingContext = createContext<LoadingContextType>({
  setLoading: () => {},
});

export const LoadingProvider = ({ children }: { children: React.ReactNode }) => {
  const [loading, setLoading] = useState(false);

  return (
    <LoadingContext.Provider value={{ setLoading }}>
      {loading && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-white/80 backdrop-blur-sm">
          <Spinner />
        </div>
      )}
      {children}
    </LoadingContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useLoading = () => useContext(LoadingContext);