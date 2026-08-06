export const ROLES = {
  DEALER: 'Dealer',
  VENDEDOR: 'Vendedor',
  COMPRADOR: 'Comprador'
} as const;

export type UserRole =
  (typeof ROLES)[keyof typeof ROLES];