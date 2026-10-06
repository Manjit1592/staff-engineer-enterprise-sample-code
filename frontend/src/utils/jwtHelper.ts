import { jwtDecode } from "jwt-decode";

export interface UserClaims {
  sub: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export const getUserFromToken = (): UserClaims | null => {
  const token = localStorage.getItem("token");

  if (!token) return null;

  try {
    const decoded: any = jwtDecode(token);

    return {
      sub: decoded.sub,
      email: decoded.email,
      firstName: decoded.firstName,
      lastName: decoded.lastName,
      role:
        decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || ""
    };
  } catch {
    return null;
  }
};