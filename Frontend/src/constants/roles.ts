export const ROLES = {
  ADMIN: 'Admin',
  DEALER: 'Dealer',
  VENDEDOR: 'Vendedor',
  COMPRADOR: 'Comprador'
} as const;

export type UserRole =
  (typeof ROLES)[keyof typeof ROLES];