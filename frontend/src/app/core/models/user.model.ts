export enum UserRole {
  Admin = 1,
  BusOperator = 2,
  Customer = 3
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}
